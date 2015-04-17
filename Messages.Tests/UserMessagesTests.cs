﻿namespace Messages.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    using Messages.Tests.Models;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UserMessagesTests
    {
        [TestMethod]
        public void ListUserMessages_EmptyDb_ShouldReturn200Ok_EmptyList()
        {
            // Arrange -> create a new user
            TestingEngine.CleanDatabase();
            var userSession = TestingEngine.RegisterUser("maria", "P@ssW01345");

            // Act -> list user messages
            var httpResponseMessages = 
                TestingEngine.GetPersonalMessagesForUserHttpGet(userSession.Access_Token);

            // Assert -> expect empty list of messages
            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessages.StatusCode);
            var messages = httpResponseMessages.Content.ReadAsAsync<List<MessageModel>>().Result;
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void SendAnonymousPersonalMessage_ListPersonalMessages_ShouldReturn200Ok_MessagesList()
        {
            // Arrange -> register a user
            TestingEngine.CleanDatabase();
            var username = "maria";
            var userSession = TestingEngine.RegisterUser(username, "P@ssW01345");
            var messages = new string[] {
                "Hello Maria " + DateTime.Now.Ticks,
                "Hello Maria (again) " + DateTime.Now.Ticks + 1,
                "Hello Maria (one more time) " + DateTime.Now.Ticks + 2
            };

            // Act -> send several messages to the user
            foreach (var message in messages)
            {
                var httpResponse =
                    TestingEngine.SendPersonalMessageHttpPost(username, message);
                Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
            }

            // Assert -> expect to get the messages correctly
            var httpResponseMessages = 
                TestingEngine.GetPersonalMessagesForUserHttpGet(userSession.Access_Token);
            var messagesFromService = 
                httpResponseMessages.Content.ReadAsAsync<List<MessageModel>>().Result;
            Assert.AreEqual(messages.Count(), messagesFromService.Count);

            // Expect the messages to arrive ordered by date (from the last to the first)
            messagesFromService.Reverse();
            for (int i = 0; i < messages.Length; i++)
            {
                Assert.IsTrue(messagesFromService[i].Id > 0);
                Assert.IsNull(messagesFromService[i].Sender);
                Assert.IsNotNull(messagesFromService[i].DateSent);
                Assert.IsTrue((DateTime.Now - messagesFromService[0].DateSent) < TimeSpan.FromMinutes(1));
                Assert.AreEqual(messages[i], messagesFromService[i].Text);
            }
        }

        [TestMethod]
        public void SendAnonymousPersonalMessage_InvalidRecipient_ShouldReturn400BadRequest()
        {
            // Arrange
            TestingEngine.CleanDatabase();
            var username = "invalidUser";

            // Act -> send a message to invalid recipient user
            var httpResponse = TestingEngine.SendPersonalMessageHttpPost(username, "Some message");

            // Assert -> 400 (Bad Request)
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        }

        [TestMethod]
        public void SendAnonymousPersonalMessage_InvalidData_ShouldReturn400BadRequest()
        {
            // Arrange
            TestingEngine.CleanDatabase();

            // Act -> POST an empty (null) message
            var httpResponseNullData = TestingEngine.HttpClient.PostAsync(
                "/api/user/personal-messages", null).Result;

            // Assert -> 400 (Bad Request)
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponseNullData.StatusCode);

            // Act -> send an empty message to empty recipient
            var httpResponseEmptyData = TestingEngine.SendPersonalMessageHttpPost("", "");

            // Assert -> 400 (Bad Request)
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponseEmptyData.StatusCode);
        }

        [TestMethod]
        public void SendPersonalMessage_ListPersonalMessages_ShouldReturn200Ok_MessagesList()
        {
            // Arrange -> register sender and recipient users
            TestingEngine.CleanDatabase();
            string senderUsername = "maria";
            var senderUserSession = TestingEngine.RegisterUser(senderUsername, "P@ssW01345");
            string recipientUsername = "peter";
            var recipientSession = TestingEngine.RegisterUser(recipientUsername, "#testAZx$27");
            var messages = new string[] {
                "Hello Peter " + DateTime.Now.Ticks,
                "Hello Peter (again) " + DateTime.Now.Ticks + 1,
                "Hello Peter (one more time) " + DateTime.Now.Ticks + 2
            };

            // Act -> send several messages to the user
            foreach (var message in messages)
            {
                var httpResponse = TestingEngine.SendPersonalMessageHttpPost(
                    senderUserSession.Access_Token, recipientUsername, message);
                Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
            }

            // Assert -> expect to get the messages correctly
            var httpResponseMessages =
                TestingEngine.GetPersonalMessagesForUserHttpGet(recipientSession.Access_Token);
            var messagesFromService =
                httpResponseMessages.Content.ReadAsAsync<List<MessageModel>>().Result;
            Assert.AreEqual(messages.Count(), messagesFromService.Count);

            // Expect the messages to arrive ordered by date (from the last to the first)
            messagesFromService.Reverse();
            for (int i = 0; i < messages.Length; i++)
            {
                Assert.IsTrue(messagesFromService[i].Id > 0);
                Assert.AreEqual(senderUsername, messagesFromService[i].Sender);
                Assert.IsNotNull(messagesFromService[i].DateSent);
                Assert.IsTrue((DateTime.Now - messagesFromService[0].DateSent) < TimeSpan.FromMinutes(1));
                Assert.AreEqual(messages[i], messagesFromService[i].Text);
            }
        }
    }
}
