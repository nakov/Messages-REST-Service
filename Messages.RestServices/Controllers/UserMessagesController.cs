namespace Messages.RestServices.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;

    using Messages.Data.Models;
    using Messages.Data.UnitOfWork;
    using Messages.RestServices.Models;

    using Microsoft.AspNet.Identity;

    [RoutePrefix("api/user")]
    public class UserMessagesController : ApiController
    {
        private readonly IMessagesData db;

        public UserMessagesController() : this(new MessagesData())
        {
        }

        public UserMessagesController(IMessagesData data)
        {
            this.db = data;
        }

        // GET: api/user/personal-messages
        [Authorize]
        [HttpGet]
        [Route("personal-messages")]
        public IHttpActionResult GetPersonalMessages()
        {
            var currentUsername = User.Identity.GetUserName();

            IQueryable<UserMessage> messages = db.UserMessages.All()
                .Where(m => m.RecipientUser.UserName == currentUsername)
                .OrderByDescending(m => m.DateSent)
                .ThenByDescending(m => m.Id);

            return this.Ok(
                messages.Select(m => new MessageViewModel()
                {
                    Id = m.Id,
                    Text = m.Text,
                    DateSent = m.DateSent,
                    Sender = (m.SenderUser != null) ? m.SenderUser.UserName : null
                }));
        }

        // POST: api/user/personal-messages
        [HttpPost]
        [Route("personal-messages")]
        public IHttpActionResult SendPersonalMessage(UserMessageBindingModel messageData)
        {
            if (messageData == null)
            {
                return BadRequest("Missing message data.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var recipientUser = this.db.Users.All()
                .FirstOrDefault(u => u.UserName == messageData.Recipient);
            if (recipientUser == null)
            {
                return BadRequest("Recipient user " + messageData.Recipient + " does not exists.");
            }

            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.db.Users.Find(currentUserId);

            var message = new UserMessage()
            {
                Text = messageData.Text,
                DateSent = DateTime.Now,
                SenderUser = currentUser,
                RecipientUser = recipientUser
            };
            db.UserMessages.Add(message);
            db.SaveChanges();

            if (message.SenderUser == null)
            {
                return this.Ok(
                    new
                    {
                        message.Id,
                        Message = "Anonymous message sent successfully to user " + recipientUser.UserName + "."
                    }
                );
            }

            return this.Ok(
                new
                {
                    message.Id, 
                    Sender = message.SenderUser.UserName,
                    Message = "Message sent successfully to user " + recipientUser.UserName + "."
                }
            );
        }
    }
}
