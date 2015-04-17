namespace Messages.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using Messages.Tests.Models;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChannelMessagesTests
    {
        [TestMethod]
        public void ListChannelMessages_EmptyDb_ShouldReturn200Ok_EmptyList()
        {
            // Arrange -> create a new channel
            TestingEngine.CleanDatabase();
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpResponseCreateChannel = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpResponseCreateChannel.StatusCode);

            // Act -> list channel messages
            var urlMessages = "/api/channel-messages/" + WebUtility.UrlEncode(channelName);
            var httpResponseMessages = TestingEngine.HttpClient.GetAsync(urlMessages).Result;

            // Assert -> expect empty list of messages
            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessages.StatusCode);
            var messages = httpResponseMessages.Content.ReadAsAsync<List<MessageModel>>().Result;
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void ListChannelMessages_NonExistingChannel_ShouldReturn404NotFound()
        {
            // Arrange
            TestingEngine.CleanDatabase();
            var channelName = "non-existing-channel";

            // Act
            var urlMessages = "/api/channel-messages/" + WebUtility.UrlEncode(channelName);
            var httpResponseMessages = TestingEngine.HttpClient.GetAsync(urlMessages).Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, httpResponseMessages.StatusCode);
        }

        [TestMethod]
        public void ListChannelMessages_ExistingChannel_ShouldReturn200OK_SortedMessagesByDate()
        {
            // Arrange -> create a chennel and send a few messages to it
            TestingEngine.CleanDatabase();
            
            // Create a channel
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpResponseCreateChannel = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpResponseCreateChannel.StatusCode);

            // Send a few messages to the channel
            string firstMsg = "First message";
            var httpResponseFirstMsg = TestingEngine.SendChannelMessageHttpPost(channelName, firstMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseFirstMsg.StatusCode);
            Thread.Sleep(2);

            string secondMsg = "Second message";
            var httpResponseSecondMsg = TestingEngine.SendChannelMessageHttpPost(channelName, secondMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseSecondMsg.StatusCode);
            Thread.Sleep(2);

            string thirdMsg = "Third message";
            var httpResponseThirdMsg = TestingEngine.SendChannelMessageHttpPost(channelName, thirdMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseThirdMsg.StatusCode);

            // Act -> list the channel messages
            var urlMessages = "/api/channel-messages/" + WebUtility.UrlEncode(channelName);
            var httpResponseMessages = TestingEngine.HttpClient.GetAsync(urlMessages).Result;

            // Assert -> messages are returned correcty, ordered from the last to the first
            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessages.StatusCode);
            var messages = httpResponseMessages.Content.ReadAsAsync<List<MessageModel>>().Result;
            Assert.AreEqual(3, messages.Count);

            // Check the first message
            Assert.IsTrue(messages[2].Id > 0);
            Assert.AreEqual(firstMsg, messages[2].Text);
            Assert.IsTrue((DateTime.Now - messages[2].DateSent) < TimeSpan.FromMinutes(1));
            Assert.IsNull(messages[2].Sender);

            // Check the second message
            Assert.IsTrue(messages[1].Id > 0);
            Assert.AreEqual(secondMsg, messages[1].Text);
            Assert.IsTrue((DateTime.Now - messages[1].DateSent) < TimeSpan.FromMinutes(1));
            Assert.IsNull(messages[1].Sender);

            // Check the third message
            Assert.IsTrue(messages[0].Id > 0);
            Assert.AreEqual(thirdMsg, messages[0].Text);
            Assert.IsTrue((DateTime.Now - messages[0].DateSent) < TimeSpan.FromMinutes(1));
            Assert.IsNull(messages[0].Sender);
        }

        [TestMethod]
        public void ListChannelMessagesWithLimit_ShouldReturn200OK_SortedMessagesByDate()
        {
            // Arrange -> create a chennel and send a few messages to it
            TestingEngine.CleanDatabase();

            // Create a channel
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpResponseCreateChannel = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpResponseCreateChannel.StatusCode);

            // Send a few messages to the channel
            string firstMsg = "First message";
            var httpResponseFirstMsg = TestingEngine.SendChannelMessageHttpPost(channelName, firstMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseFirstMsg.StatusCode);
            Thread.Sleep(2);

            string secondMsg = "Second message";
            var httpResponseSecondMsg = TestingEngine.SendChannelMessageHttpPost(channelName, secondMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseSecondMsg.StatusCode);
            Thread.Sleep(2);

            string thirdMsg = "Third message";
            var httpResponseThirdMsg = TestingEngine.SendChannelMessageHttpPost(channelName, thirdMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseThirdMsg.StatusCode);

            // Act -> list the channel messages with limit 2
            var urlMessages = "/api/channel-messages/" + WebUtility.UrlEncode(channelName) + "?limit=2";
            var httpResponseMessages = TestingEngine.HttpClient.GetAsync(urlMessages).Result;

            // Assert -> messages are returned correcty, ordered from the last to the first
            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessages.StatusCode);
            var messages = httpResponseMessages.Content.ReadAsAsync<List<MessageModel>>().Result;
            Assert.AreEqual(2, messages.Count);

            // Check the second message
            Assert.IsTrue(messages[1].Id > 0);
            Assert.AreEqual(secondMsg, messages[1].Text);
            Assert.IsTrue((DateTime.Now - messages[1].DateSent) < TimeSpan.FromMinutes(1));
            Assert.IsNull(messages[1].Sender);

            // Check the third message
            Assert.IsTrue(messages[0].Id > 0);
            Assert.AreEqual(thirdMsg, messages[0].Text);
            Assert.IsTrue((DateTime.Now - messages[0].DateSent) < TimeSpan.FromMinutes(1));
            Assert.IsNull(messages[0].Sender);
        }

        [TestMethod]
        public void ListChannelMessagesWithInvalidLimit_ShouldReturn400BadRequest()
        {
            // Arrange -> create a chennel and send a few messages to it
            TestingEngine.CleanDatabase();

            // Create a channel
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpResponseCreateChannel = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpResponseCreateChannel.StatusCode);

            // Send a few messages to the channel
            string firstMsg = "First message";
            var httpResponseFirstMsg = TestingEngine.SendChannelMessageHttpPost(channelName, firstMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseFirstMsg.StatusCode);
            Thread.Sleep(2);

            string secondMsg = "Second message";
            var httpResponseSecondMsg = TestingEngine.SendChannelMessageHttpPost(channelName, secondMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseSecondMsg.StatusCode);

            // Act -> list the channel messages with limit 1001
            var urlMessages = "/api/channel-messages/" + WebUtility.UrlEncode(channelName); ;
            var httpResponse = TestingEngine.HttpClient.GetAsync(urlMessages + "?limit=1001").Result;

            // Assert -> 400 (Bad Request)
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);

            // Act -> list the channel messages with limit 0
            httpResponse = TestingEngine.HttpClient.GetAsync(urlMessages + "?limit=0").Result;

            // Assert -> 400 (Bad Request)
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);

            // Act -> list the channel messages with limit "invalid"
            httpResponse = TestingEngine.HttpClient.GetAsync(urlMessages + "?limit=invalid").Result;

            // Assert -> 400 (Bad Request)
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        }

        [TestMethod]
        public void SendAnonymousChannelMessage_ShouldListMesagesCorectly()
        {
            // Arrange -> create a new channel
            TestingEngine.CleanDatabase();
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpResponseCreateChannel = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpResponseCreateChannel.StatusCode);

            // Act -> send a few anonymous messages to the channel
            string firstMsg = "First message";
            var httpResponseFirstMsg = TestingEngine.SendChannelMessageHttpPost(channelName, firstMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseFirstMsg.StatusCode);
            Thread.Sleep(2);

            string secondMsg = "Second message";
            var httpResponseSecondMsg = TestingEngine.SendChannelMessageHttpPost(channelName, secondMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseSecondMsg.StatusCode);

            // Act -> list the channel messages
            var urlMessages = "/api/channel-messages/" + WebUtility.UrlEncode(channelName);
            var httpResponseMessages = TestingEngine.HttpClient.GetAsync(urlMessages).Result;

            // Assert -> messages are returned correcty, ordered from the last to the first
            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessages.StatusCode);
            var messages = httpResponseMessages.Content.ReadAsAsync<List<MessageModel>>().Result;
            Assert.AreEqual(2, messages.Count);

            // Check the first message
            Assert.IsTrue(messages[1].Id > 0);
            Assert.AreEqual(firstMsg, messages[1].Text);
            Assert.IsTrue((DateTime.Now - messages[1].DateSent) < TimeSpan.FromMinutes(1));
            Assert.IsNull(messages[1].Sender);

            // Check the second message
            Assert.IsTrue(messages[0].Id > 0);
            Assert.AreEqual(secondMsg, messages[0].Text);
            Assert.IsTrue((DateTime.Now - messages[0].DateSent) < TimeSpan.FromMinutes(1));
            Assert.IsNull(messages[0].Sender);
        }

        [TestMethod]
        public void SendChannelMessage_InvalidMessageData_ShouldReturn400BadRequest()
        {
            // Arrange -> create a chennel and send a few messages to it
            TestingEngine.CleanDatabase();

            // Create a channel
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpResponseCreateChannel = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpResponseCreateChannel.StatusCode);

            // Act -> try to send a message with empty HTTP POST body
            var sendMsgUrl = "/api/channel-messages/" + WebUtility.UrlEncode(channelName);
            var httpResponseNullMsg = TestingEngine.HttpClient.PostAsync(sendMsgUrl, null).Result;

            // Assert -> 400 (Bad Request)
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponseNullMsg.StatusCode);

            // Act -> try to send a message with empty text
            var httpResponseEmptyMsg = TestingEngine.SendChannelMessageHttpPost(channelName, "");

            // Assert -> 400 (Bad Request)
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponseEmptyMsg.StatusCode);
        }

        [TestMethod]
        public void SendChannelMessage_NonExitingChannel_ShouldReturn404NotFound()
        {
            // Arrange
            TestingEngine.CleanDatabase();
            var channelName = "non-existing-channel";

            // Act -> try to send a message to non-existing channel
            var httpResponse = TestingEngine.SendChannelMessageHttpPost(channelName, "msg");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode);
        }

        [TestMethod]
        public void SendChannelMessage_FromExisitingUser_ShouldListMessagesCorectly()
        {
            // Arrange -> create a channel
            TestingEngine.CleanDatabase();
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpResponseCreateChannel = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpResponseCreateChannel.StatusCode);

            // Arrange -> register two users
            var userSessionPeter = TestingEngine.RegisterUser("peter", "pAssW@rd#123456");
            var userSessionMaria = TestingEngine.RegisterUser("maria", "SECret#76^%asf!");

            // Act -> send a few messages to the channel (from the registered users and anonymous)
            string firstMsg = "A message from Peter";
            var httpResponseFirstMsg = TestingEngine.SendChannelMessageFromUserHttpPost(
                channelName, firstMsg, userSessionPeter.Access_Token);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseFirstMsg.StatusCode);
            Thread.Sleep(2);

            string secondMsg = "Anonymous message";
            var httpResponseThirdMsg = TestingEngine.SendChannelMessageHttpPost(channelName, secondMsg);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseThirdMsg.StatusCode);
            Thread.Sleep(2);

            string thirdMsg = "A message from Maria";
            var httpResponseSecondMsg = TestingEngine.SendChannelMessageFromUserHttpPost(
                channelName, thirdMsg, userSessionMaria.Access_Token);
            Assert.AreEqual(HttpStatusCode.OK, httpResponseSecondMsg.StatusCode);

            // Act -> list the channel messages
            var urlMessages = "/api/channel-messages/" + WebUtility.UrlEncode(channelName);
            var httpResponseMessages = TestingEngine.HttpClient.GetAsync(urlMessages).Result;

            // Assert -> messages are returned correcty, ordered from the last to the first
            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessages.StatusCode);
            var messages = httpResponseMessages.Content.ReadAsAsync<List<MessageModel>>().Result;
            Assert.AreEqual(3, messages.Count);

            // Check the first message
            Assert.IsTrue(messages[2].Id > 0);
            Assert.AreEqual(firstMsg, messages[2].Text);
            Assert.IsTrue((DateTime.Now - messages[2].DateSent) < TimeSpan.FromMinutes(1));
            Assert.AreEqual("peter", messages[2].Sender);

            // Check the second message
            Assert.IsTrue(messages[1].Id > 0);
            Assert.AreEqual(secondMsg, messages[1].Text);
            Assert.IsTrue((DateTime.Now - messages[1].DateSent) < TimeSpan.FromMinutes(1));
            Assert.IsNull(messages[1].Sender);

            // Check the third message
            Assert.IsTrue(messages[0].Id > 0);
            Assert.AreEqual(thirdMsg, messages[0].Text);
            Assert.IsTrue((DateTime.Now - messages[0].DateSent) < TimeSpan.FromMinutes(1));
            Assert.AreEqual("maria", messages[0].Sender);
        }
    }
}
