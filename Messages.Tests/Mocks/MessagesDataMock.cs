namespace Messages.Tests.Mocks
{
    using Messages.Data.Models;
    using Messages.Data.Repositories;
    using Messages.Data.UnitOfWork;

    using Microsoft.AspNet.Identity;

    public class MessagesDataMock : IMessagesData
    {
        private GenericRepositoryMock<User> usersMock = new GenericRepositoryMock<User>();
        private GenericRepositoryMock<Channel> channelsMock = new GenericRepositoryMock<Channel>();
        private GenericRepositoryMock<ChannelMessage> channelMessagesMock = new GenericRepositoryMock<ChannelMessage>();
        private GenericRepositoryMock<UserMessage> userMessagesMock = new GenericRepositoryMock<UserMessage>();
        private GenericUserStoreMock<User> userStoreMock = new GenericUserStoreMock<User>();

        public bool ChangesSaved { get; set; }

        public IRepository<User> Users {
            get { return this.usersMock; }
        }

        public IRepository<Channel> Channels
        {
            get { return this.channelsMock; }
        }

        public IRepository<ChannelMessage> ChannelMessages
        {
            get { return this.channelMessagesMock; }
        }

        public IRepository<UserMessage> UserMessages
        {
            get { return this.userMessagesMock; }
        }

        public IUserStore<User> UserStore
        {
            get { return this.userStoreMock; }
        }

        public void SaveChanges()
        {
            this.ChangesSaved = true;
        }
    }
}
