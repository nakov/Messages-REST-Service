namespace Messages.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;

    using Messages.Tests.Models;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChannelIntegrationTests
    {
        [TestMethod]
        public void DeleteExistingChannel_ShouldReturn200OK()
        {
            // Arrange -> create a channel
            TestingEngine.CleanDatabase();

            var channelName = "channel" + DateTime.Now.Ticks;
            var httpPostResponse = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponse.StatusCode);
            var channel = httpPostResponse.Content.ReadAsAsync<ChannelModel>().Result;
            Assert.AreEqual(1, TestingEngine.GetChannelsCountFromDb());

            // Act -> delete the channel
            var httpDeleteResponse = TestingEngine.HttpClient.DeleteAsync(
                "/api/channels/" + channel.Id).Result;

            // Assert -> HTTP status code is 200 (OK)
            Assert.AreEqual(HttpStatusCode.OK, httpDeleteResponse.StatusCode);
            Assert.AreEqual(0, TestingEngine.GetChannelsCountFromDb());
        }

        [TestMethod]
        public void DeleteNonExistingChannel_ShouldReturn404NotFound()
        {
            // Arrange -> clean the DB
            TestingEngine.CleanDatabase();

            // Act -> delete the channel
            var httpDeleteResponse = TestingEngine.HttpClient.DeleteAsync("/api/channels/1").Result;

            // Assert -> HTTP status code is 404 (Not Found)
            Assert.AreEqual(HttpStatusCode.NotFound, httpDeleteResponse.StatusCode);
        }

        [TestMethod]
        public void DeleteChannel_WithMessages_ShouldReturn409Conflict()
        {
            // Arrange -> create a channel with a message posted in it
            TestingEngine.CleanDatabase();

            // Create a channel
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpResponseCreateChanel = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpResponseCreateChanel.StatusCode);
            var channel = httpResponseCreateChanel.Content.ReadAsAsync<ChannelModel>().Result;
            Assert.AreEqual(1, TestingEngine.GetChannelsCountFromDb());

            // Post an anonymous message in the channel
            var httpResponsePostMsg = TestingEngine.SendChannelMessageHttpPost(channelName, "message");
            Assert.AreEqual(HttpStatusCode.OK, httpResponsePostMsg.StatusCode);

            // Act -> try to delete the channel with the message
            var httpDeleteResponse = TestingEngine.HttpClient.DeleteAsync(
                "/api/channels/" + channel.Id).Result;

            // Assert -> HTTP status code is 409 (Conflict), channel is not empty
            Assert.AreEqual(HttpStatusCode.Conflict, httpDeleteResponse.StatusCode);
            Assert.AreEqual(1, TestingEngine.GetChannelsCountFromDb());
        }
    }
}
