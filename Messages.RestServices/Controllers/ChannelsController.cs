namespace Messages.RestServices.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web.Http;

    using Messages.Data.Models;
    using Messages.Data.UnitOfWork;
    using Messages.RestServices.Models;

    [RoutePrefix("api")]
    public class ChannelsController : ApiController
    {
        private readonly IMessagesData db;

        public ChannelsController() : this(new MessagesData())
        {
        }

        public ChannelsController(IMessagesData data)
        {
            this.db = data;
        }

        // GET: api/channels
        [HttpGet]
        [Route("channels")]
        public IHttpActionResult GetChannels()
        {
            var channels = db.Channels.All().OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name
                });
            return this.Ok(channels);
        }

        // GET: api/channels/{id}
        [HttpGet]
        [Route("channels/{id:int}")]
        public IHttpActionResult GetChannelById(int id)
        {
            Channel channel = db.Channels.Find(id);
            if (channel == null)
            {
                return this.NotFound();
            }

            return Ok(new
            {
                channel.Id, 
                channel.Name
            });
        }

        // POST: api/channels
        [HttpPost]
        [Route("channels")]
        public IHttpActionResult CreateChannel(ChannelBindingModel channelData)
        {
            if (channelData == null)
            {
                return BadRequest("Missing channel data.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (db.Channels.All().Any(c => c.Name == channelData.Name))
            {
                return this.Content(HttpStatusCode.Conflict,
                    new { Message = "Duplicated channel name: " + channelData.Name } );
            }

            var channel = new Channel() { Name = channelData.Name };
            db.Channels.Add(channel);
            db.SaveChanges();

            return this.CreatedAtRoute(
                "DefaultApi", 
                new { controller = "channels", id = channel.Id },
                new { channel.Id, channel.Name });
        }

        // PUT: api/channels/{id}
        [HttpPut]
        [Route("channels/{id:int}")]
        public IHttpActionResult EditChannel(int id, ChannelBindingModel channelData)
        {
            if (channelData == null)
            {
                return BadRequest("Missing channel data.");
            }

            var channel = db.Channels.Find(id);
            if (channel == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (db.Channels.All().Any(c => c.Name == channelData.Name && c.Id != id))
            {
                return this.Content(HttpStatusCode.Conflict,
                    new { Message = "Duplicated channel name: " + channelData.Name });
            }

            channel.Name = channelData.Name;
            db.Channels.Update(channel);
            db.SaveChanges();

            return this.Ok(
                new
                {
                    Message = "Channel #" + id + " edited successfully."
                }
            );
        }

        // DELETE: api/channels/{id}
        [HttpDelete]
        [Route("channels/{id:int}")]
        public IHttpActionResult DeleteChannel(int id)
        {
            Channel channel = db.Channels.Find(id);
            if (channel == null)
            {
                return NotFound();
            }

            if (channel.Messages.Any())
            {
                return this.Content(HttpStatusCode.Conflict,
                    new { Message = "Cannot delete channel #" + id + " because it is not empty." });
            }

            db.Channels.Remove(channel);
            db.SaveChanges();

            return Ok(new
            {
                Message = "Channel #" + id + " deleted."
            });
        }
    }
}
