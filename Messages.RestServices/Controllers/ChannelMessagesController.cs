namespace Messages.RestServices.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;

    using Messages.Data.Models;
    using Messages.Data.UnitOfWork;
    using Messages.RestServices.Models;

    using Microsoft.AspNet.Identity;

    [RoutePrefix("api")]
    public class ChannelMessagesController : ApiController
    {
        private readonly IMessagesData db;

        public ChannelMessagesController() : this(new MessagesData())
        {
        }

        public ChannelMessagesController(IMessagesData data)
        {
            this.db = data;
        }

        // GET: api/channel-messages/channelName
        [HttpGet]
        [Route("channel-messages/{channelName}")]
        public IHttpActionResult GetChannelMessages(string channelName, [FromUri] string limit = null)
        {
            var channel = db.Channels.All().FirstOrDefault(c => c.Name == channelName);
            if (channel == null)
            {
                return this.NotFound();
            }

            IQueryable<ChannelMessage> messages = db.ChannelMessages.All()
                .Where(m => m.Channel.Id == channel.Id)
                .OrderByDescending(m => m.DateSent)
                .ThenByDescending(m => m.Id);

            if (limit != null)
            {
                int limitCount = 0;
                int.TryParse(limit, out limitCount);
                if (limitCount >= 1 && limitCount <= 1000)
                {
                    messages = messages.Take(limitCount);
                }
                else
                {
                    return this.BadRequest("Limit should be integer in range [1..1000].");
                }
            }

            return this.Ok(
                messages.Select(m => new MessageViewModel()
                {
                    Id = m.Id,
                    Text = m.Text,
                    DateSent = m.DateSent,
                    Sender = (m.SenderUser != null) ? m.SenderUser.UserName : null
                }));
        }

        // POST: api/channel-messages/channelName
        [HttpPost]
        [Route("channel-messages/{channelName}")]
        public IHttpActionResult SendChannelMessage(
            string channelName, ChannelMessageBindingModel channelMessageData)
        {
            if (channelMessageData == null)
            {
                return BadRequest("Missing message data.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var channel = db.Channels.All().FirstOrDefault(c => c.Name == channelName);
            if (channel == null)
            {
                return this.NotFound();
            }

            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.db.Users.Find(currentUserId);

            var message = new ChannelMessage()
            {
                Text = channelMessageData.Text,
                Channel = channel,
                DateSent = DateTime.Now,
                SenderUser = currentUser
            };
            db.ChannelMessages.Add(message);
            db.SaveChanges();

            if (message.SenderUser == null)
            {
                return this.Ok(
                    new
                    {
                        message.Id,
                        Message = "Anonymous message sent successfully to channel " + channelName + "."
                    }
                );
            }

            return this.Ok(
                new
                {
                    message.Id,
                    Sender = message.SenderUser.UserName,
                    Message = "Message sent successfully to channel " + channelName + "."
                }
            );
        }
    }
}
