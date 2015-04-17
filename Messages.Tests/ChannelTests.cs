namespace Messages.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    using Messages.Tests.Models;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChannelTests
    {
        [TestMethod]
        public void ListChannels_EmptyDb_ShouldReturn200Ok_EmptyList()
        {
            // Arrange
            TestingEngine.CleanDatabase();

            // Act
            var httpResponse = TestingEngine.HttpClient.GetAsync("/api/channels").Result;
            var channels = httpResponse.Content.ReadAsAsync<List<ChannelModel>>().Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.AreEqual(httpResponse.Content.Headers.ContentType.MediaType, "application/json");
            Assert.AreEqual(0, channels.Count);
        }

        [TestMethod]
        public void CreateNewChannel_ShouldCreateChannel_Return201Created()
        {
            // Arrange
            TestingEngine.CleanDatabase();
            var channelName = "channel" + DateTime.Now.Ticks;

            // Act
            var httpResponse = TestingEngine.CreateChannelHttpPost(channelName);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
            Assert.IsNotNull(httpResponse.Headers.Location);
            var newChannel = httpResponse.Content.ReadAsAsync<ChannelModel>().Result;
            Assert.IsTrue(newChannel.Id != 0);
            Assert.AreEqual(newChannel.Name, channelName);

            var channelsCountInDb = TestingEngine.GetChannelsCountFromDb();
            Assert.AreEqual(1, channelsCountInDb);
        }

        [TestMethod]
        public void CreateNewChannel_InvalidData_ShouldReturn400BadRequest()
        {
            // Arrange
            TestingEngine.CleanDatabase();
            var invalidChannelName = string.Empty;

            // Act
            var httpResponse = TestingEngine.CreateChannelHttpPost(invalidChannelName);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);
            var channelsCountInDb = TestingEngine.GetChannelsCountFromDb();
            Assert.AreEqual(0, channelsCountInDb);
        }

        [TestMethod]
        public void CreateNewChannel_EmptyBody_ShouldReturn400BadRequest()
        {
            // Arrange
            TestingEngine.CleanDatabase();

            // Act
            var httpResponse = TestingEngine.HttpClient.PostAsync("/api/channels", null).Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);
            var channelsCountInDb = TestingEngine.GetChannelsCountFromDb();
            Assert.AreEqual(0, channelsCountInDb);
        }

        [TestMethod]
        public void CreateNewChannel_NameTooLong_ShouldReturn400BadRequest()
        {
            // Arrange
            TestingEngine.CleanDatabase();
            var tooLongChannelName = new string('a', 101);

            // Act
            var httpResponse = TestingEngine.CreateChannelHttpPost(tooLongChannelName);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);
            var channelsCountInDb = TestingEngine.GetChannelsCountFromDb();
            Assert.AreEqual(0, channelsCountInDb);
        }

        [TestMethod]
        public void CreateDuplicatedChannels_ShouldReturn409Conflict()
        {
            // Arrange
            TestingEngine.CleanDatabase();
            var channelName = "channel" + DateTime.Now.Ticks;

            // Act
            var httpResponseFirst = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpResponseFirst.StatusCode);

            var httpResponseSecond = TestingEngine.CreateChannelHttpPost(channelName);

            // Assert
            Assert.AreEqual(HttpStatusCode.Conflict, httpResponseSecond.StatusCode);
        }

        [TestMethod]
        public void CreateChannels_ListChannels_ShouldListCreatedChannelsAlphabetically()
        {
            // Arrange -> prepare a few channels
            TestingEngine.CleanDatabase();
            var channelsToAdds = new string[]
            {  
                "Channel Omega" + DateTime.Now.Ticks,
                "Channel Alpha" + DateTime.Now.Ticks,
                "Channel Zeta" + DateTime.Now.Ticks,
                "Channel X" + DateTime.Now.Ticks,
                "Channel Psy" + DateTime.Now.Ticks
            };

            // Act -> create a few channels
            foreach (var channelName in channelsToAdds)
            {
                var httpPostResponse = TestingEngine.CreateChannelHttpPost(channelName);
                
                // Assert -> ensure each channel is successfully created
                Assert.AreEqual(HttpStatusCode.Created, httpPostResponse.StatusCode);
            }

            // Assert -> list the channels and assert their count, order and content are correct
            var httpResponse = TestingEngine.HttpClient.GetAsync("/api/channels").Result;
            Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);

            var channelsFromService = httpResponse.Content.ReadAsAsync<List<ChannelModel>>().Result;
            Assert.AreEqual(channelsToAdds.Count(), channelsFromService.Count);

            var sortedChannels = channelsToAdds.OrderBy(c => c).ToList();
            for (int i = 0; i < sortedChannels.Count; i++)
            {
                Assert.IsTrue(channelsFromService[i].Id != 0);
                Assert.AreEqual(sortedChannels[i], channelsFromService[i].Name);
            }
        }

        [TestMethod]
        public void GetChannelById_EmptyDb_ShouldReturn404NotFound()
        {
            // Arrange
            TestingEngine.CleanDatabase();

            // Act
            var httpResponse = TestingEngine.HttpClient.GetAsync("/api/channels/1").Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode);
        }

        [TestMethod]
        public void GetChannelById_ExistingChannel_ShouldReturnTheChannel()
        {
            // Arrange -> create a new channel
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpPostResponse = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponse.StatusCode);
            var postedChannel = httpPostResponse.Content.ReadAsAsync<ChannelModel>().Result;

            // Act -> find the channel by its ID
            var httpResponse = TestingEngine.HttpClient.GetAsync("/api/channels/" + postedChannel.Id).Result;

            // Assert -> the channel by ID holds correct data
            Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
            var channelFromService = httpResponse.Content.ReadAsAsync<ChannelModel>().Result;
            Assert.IsTrue(channelFromService.Id != 0);
            Assert.AreEqual(channelFromService.Name, channelName);
        }

        [TestMethod]
        public void EditExistingChannel_ShouldReturn200OK_Modify()
        {
            // Arrange -> create a new channel
            TestingEngine.CleanDatabase();
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpPostResponse = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponse.StatusCode);
            var postedChannel = httpPostResponse.Content.ReadAsAsync<ChannelModel>().Result;

            // Act -> edit the above created channel
            var channelNewName = "Edited " + channelName;
            var httpPutResponse = TestingEngine.EditChannelHttpPut(postedChannel.Id, channelNewName);

            // Assert -> the PUT result is 200 OK
            Assert.AreEqual(HttpStatusCode.OK, httpPutResponse.StatusCode);

            // Assert the service holds the modified channel
            var httpGetResponse = TestingEngine.HttpClient.GetAsync("/api/channels").Result;
            var channelsFromService = httpGetResponse.Content.ReadAsAsync<List<ChannelModel>>().Result;
            Assert.AreEqual(HttpStatusCode.OK, httpGetResponse.StatusCode);
            Assert.AreEqual(1, channelsFromService.Count);
            Assert.AreEqual(postedChannel.Id, channelsFromService.First().Id);
            Assert.AreEqual(channelNewName, channelsFromService.First().Name);
        }

        [TestMethod]
        public void EditChannel_InvalidData_ShouldReturn400BadRequest()
        {
            // Arrange -> create a new channel
            TestingEngine.CleanDatabase();
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpPostResponse = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponse.StatusCode);
            var postedChannel = httpPostResponse.Content.ReadAsAsync<ChannelModel>().Result;

            // Act -> try to edit the above created channel
            var channelNewName = String.Empty;
            var httpPutResponse = TestingEngine.EditChannelHttpPut(postedChannel.Id, channelNewName);

            // Assert -> the PUT result is 400 Bad Request
            Assert.AreEqual(HttpStatusCode.BadRequest, httpPutResponse.StatusCode);
        }

        [TestMethod]
        public void EditChannel_EmptyBody_ShouldReturn400BadRequest()
        {
            // Arrange -> create a new channel
            TestingEngine.CleanDatabase();
            var channelName = "channel" + DateTime.Now.Ticks;
            var httpPostResponse = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponse.StatusCode);
            var postedChannel = httpPostResponse.Content.ReadAsAsync<ChannelModel>().Result;

            // Act -> try to edit the above created channel
            var httpPutResponse = TestingEngine.HttpClient.PutAsync(
                "/api/channels/" + postedChannel.Id, null).Result;
            // Assert -> the PUT result is 400 Bad Request
            Assert.AreEqual(HttpStatusCode.BadRequest, httpPutResponse.StatusCode);
        }

        [TestMethod]
        public void EditNotExistingChannel_ShouldReturn404NotFond()
        {
            // Arrange -> clear the database
            TestingEngine.CleanDatabase();

            // Act -> try to edit non-existing channel
            var httpPutResponse = TestingEngine.EditChannelHttpPut(1, "new name");

            // Assert -> the PUT result is 404 Not Found
            Assert.AreEqual(HttpStatusCode.NotFound, httpPutResponse.StatusCode);
        }

        [TestMethod]
        public void EditChannel_DuplicatedName_ShouldReturn409Conflict()
        {
            // Arrange -> create two channels
            TestingEngine.CleanDatabase();

            var channelNameFirst = "channel" + DateTime.Now.Ticks;
            var firstChannelHttpResponse = TestingEngine.CreateChannelHttpPost(channelNameFirst);
            Assert.AreEqual(HttpStatusCode.Created, firstChannelHttpResponse.StatusCode);
            var firstChannel = firstChannelHttpResponse.Content.ReadAsAsync<ChannelModel>().Result;

            var channelNameSecond = "channel" + DateTime.Now.Ticks + 1;
            var secondChannelHttpResponse = TestingEngine.CreateChannelHttpPost(channelNameSecond);
            Assert.AreEqual(HttpStatusCode.Created, secondChannelHttpResponse.StatusCode);

            // Act -> try to edit the first channel and duplicate its name with the second channel
            var httpPutResponseFirst = TestingEngine.EditChannelHttpPut(firstChannel.Id, channelNameSecond);

            // Assert -> HTTP status code is 409 (Conflict)
            Assert.AreEqual(HttpStatusCode.Conflict, httpPutResponseFirst.StatusCode);
        }

        [TestMethod]
        public void EditChannel_LeaveSameName_ShouldReturn200OK()
        {
            // Arrange -> create a channel
            TestingEngine.CleanDatabase();

            var channelName = "channel" + DateTime.Now.Ticks;
            var httpPostResponse = TestingEngine.CreateChannelHttpPost(channelName);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponse.StatusCode);
            var channel = httpPostResponse.Content.ReadAsAsync<ChannelModel>().Result;

            // Act -> try to edit the channel and leave its name the same
            var httpPutResponse = TestingEngine.EditChannelHttpPut(channel.Id, channelName);

            // Assert -> HTTP status code is 200 (OK)
            Assert.AreEqual(HttpStatusCode.OK, httpPutResponse.StatusCode);
        }

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
