namespace Messages.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;
    using System.Web.Http.Routing;

    using Messages.Data.Models;
    using Messages.RestServices;
    using Messages.RestServices.Models;
    using Messages.Tests.Mocks;
    using Messages.Tests.Models;

    using Messages.RestServices.Controllers;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnitTestsWithMocking
    {
        [TestMethod]
        public void GetChannelById_ExistingChannel_ShouldReturn200OK_CorrectChannelData()
        {
            // Arrange -> create a few channels
            var dataLayerMock = new MessagesDataMock();
            var channelsMock = dataLayerMock.Channels;
            channelsMock.Add(new Channel() { Id = 1, Name = "Channel #1" });
            channelsMock.Add(new Channel() { Id = 2, Name = "Channel #2" });
            channelsMock.Add(new Channel() { Id = 3, Name = "Channel #3" });

            // Act -> Get channel by ID
            var channelsController = new ChannelsController(dataLayerMock);
            this.SetupControllerForTesting(channelsController, "channels");
            var httpResponse = channelsController.GetChannelById(2)
                .ExecuteAsync(new CancellationToken()).Result;

            // Assert -> HTTP status code 200 (OK) + correct channel data
            Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
            var channel2 = httpResponse.Content.ReadAsAsync<ChannelModel>().Result;
            Assert.AreEqual(2, channel2.Id);
            Assert.AreEqual("Channel #2", channel2.Name);
        }

        [TestMethod]
        public void GetChannelById_NonExistingChannel_ShouldReturn404NotFound()
        {
            // Arrange -> create a few channels
            var dataLayerMock = new MessagesDataMock();
            var channelsMock = dataLayerMock.Channels;
            channelsMock.Add(new Channel() { Id = 1, Name = "Channel #1" });
            channelsMock.Add(new Channel() { Id = 2, Name = "Channel #2" });
            channelsMock.Add(new Channel() { Id = 3, Name = "Channel #3" });

            // Act -> Get channel by ID
            var channelsController = new ChannelsController(dataLayerMock);
            this.SetupControllerForTesting(channelsController, "channels");
            var httpResponse = channelsController.GetChannelById(20)
                .ExecuteAsync(new CancellationToken()).Result;

            // Assert -> HTTP status code 404 (Not Found)
            Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode);
        }

        [TestMethod]
        public void TestGenericRepositoryMock()
        {
            var mockedUsers = new GenericRepositoryMock<User>();
            mockedUsers.Add(new User() { Id = "1a39f06e-e9dc-4eb5-9b9e-93ed2f5e6f96", UserName = "peter" });
            mockedUsers.Add(new User() { Id = "311ecd00-07a2-4b64-908e-c7f376762562", UserName = "maria" });
            mockedUsers.Add(new User() { Id = "44e815b9-dbbe-4544-bdac-b0a490f9d8cc", UserName = "todor" });
            mockedUsers.SaveChanges();
            CollectionAssert.AreEqual(
                mockedUsers.All().Select(u => u.UserName).ToArray(),
                new string[] { "peter", "maria", "todor" }
            );
            Assert.AreEqual("todor", mockedUsers.Find("44e815b9-dbbe-4544-bdac-b0a490f9d8cc").UserName);

            mockedUsers.Remove(new User() { Id = "311ecd00-07a2-4b64-908e-c7f376762562" });
            mockedUsers.SaveChanges();
            CollectionAssert.AreEqual(
                mockedUsers.All().Select(u => u.UserName).ToArray(),
                new string[] { "peter", "todor" }
            );

            mockedUsers.Update(new User() { Id = "1a39f06e-e9dc-4eb5-9b9e-93ed2f5e6f96", UserName = "george"});
            mockedUsers.SaveChanges();
            CollectionAssert.AreEqual(
                mockedUsers.All().Select(u => u.UserName).ToArray(),
                new string[] { "george", "todor" }
            );
        }

        [TestMethod]
        public void TestGenericUserStoreMock()
        {
            var mockedUsers = new GenericUserStoreMock<User>();
            mockedUsers.CreateAsync(new User() { Id = "1a39f06e-e9dc-4eb5-9b9e-93ed2f5e6f96", UserName = "peter" }).Wait();
            mockedUsers.CreateAsync(new User() { Id = "311ecd00-07a2-4b64-908e-c7f376762562", UserName = "maria" }).Wait();
            mockedUsers.CreateAsync(new User() { Id = "44e815b9-dbbe-4544-bdac-b0a490f9d8cc", UserName = "todor" }).Wait();
            CollectionAssert.AreEqual(
                mockedUsers.AllUsers.Select(u => u.UserName).ToArray(),
                new string[] { "peter", "maria", "todor" }
            );
            Assert.AreEqual(
                "todor", 
                mockedUsers.FindByIdAsync("44e815b9-dbbe-4544-bdac-b0a490f9d8cc").Result.UserName);

            mockedUsers.DeleteAsync(new User() { Id = "311ecd00-07a2-4b64-908e-c7f376762562" }).Wait();
            CollectionAssert.AreEqual(
                mockedUsers.AllUsers.Select(u => u.UserName).ToArray(),
                new string[] { "peter", "todor" }
            );

            mockedUsers.UpdateAsync(new User() { Id = "1a39f06e-e9dc-4eb5-9b9e-93ed2f5e6f96", UserName = "george" }).Wait();
            CollectionAssert.AreEqual(
                mockedUsers.AllUsers.Select(u => u.UserName).ToArray(),
                new string[] { "george", "todor" }
            );
        }

        private void SetupControllerForTesting(ApiController controller, string controllerName)
        {
            string serverUrl = "http://sample-url.com";

            // Setup the Request object of the controller
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(serverUrl)
            };
            controller.Request = request;

            // Setup the configuration of the controller
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
            controller.Configuration = config;

            // Apply the routes to the controller
            controller.RequestContext.RouteData = new HttpRouteData(
                route: new HttpRoute(),
                values: new HttpRouteValueDictionary
                {
                    { "controller", controllerName }
                });
        }

        //
        // This test will not run corectly, because we cannot mock the "POST /api/token" acion
        //
        // [TestMethod]
        public void Login_ValidUser_ShouldReturn200OkSessionToken()
        {

            // Arrange -> mock the data layer
            var dataLayerMock = new MessagesDataMock();
            var userStoreMock = dataLayerMock.UserStore;
            var userManagerMock = new ApplicationUserManager(userStoreMock);
            string username = "peter";
            string password = "s@m3-P@$$W0rd";
            userManagerMock.CreateAsync(new User() { UserName = username }, password);

            var accountController = new AccountController(dataLayerMock);
            this.SetupControllerForTesting(accountController, "user");

            // Act -> Get channel by ID
            var userModel = new UserAccountBindingModel()
            {
                Username = username,
                Password = password
            };
            var httpResponse = accountController.LoginUser(userModel)
                .Result.ExecuteAsync(new CancellationToken()).Result;

            // Assert -> HTTP status code 200 (OK) + correct user data
            Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
            var userSession = httpResponse.Content.ReadAsAsync<UserSessionModel>().Result;
            Assert.AreEqual(username, userSession.UserName);
            Assert.IsNotNull(userSession.Access_Token);
        }
    }
}
