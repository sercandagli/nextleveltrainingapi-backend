using MongoDB.Driver;
using NextLevelTrainingApi.DAL.Entities;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Repository
{
    public class UnitOfWork : IUnitOfWork
    {

        private IMongoDatabase _mongoDatabase;

        public UnitOfWork(INextLevelDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _mongoDatabase = client.GetDatabase(settings.DatabaseName);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private IGenericRepository<Users> userRepository;
        public IGenericRepository<Users> UserRepository
        {
            get { return userRepository ?? (userRepository = new GenericRepository<Users>(_mongoDatabase)); }
        }

        private IGenericRepository<Message> messageRepository;
        public IGenericRepository<Message> MessageRepository
        {
            get { return messageRepository ?? (messageRepository = new GenericRepository<Message>(_mongoDatabase)); }
        }

        private IGenericRepository<Post> postRepository;
        public IGenericRepository<Post> PostRepository
        {
            get { return postRepository ?? (postRepository = new GenericRepository<Post>(_mongoDatabase)); }
        }

        private IGenericRepository<PostCode> postCodeRepository;
        public IGenericRepository<PostCode> PostCodeRepository
        {
            get { return postCodeRepository ?? (postCodeRepository = new GenericRepository<PostCode>(_mongoDatabase)); }
        }

        private IGenericRepository<Booking> bookingRepository;
        public IGenericRepository<Booking> BookingRepository
        {
            get { return bookingRepository ?? (bookingRepository = new GenericRepository<Booking>(_mongoDatabase)); }
        }

        private IGenericRepository<ErrorLog> errorLogRepository;
        public IGenericRepository<ErrorLog> ErrorLogRepository
        {
            get { return errorLogRepository ?? (errorLogRepository = new GenericRepository<ErrorLog>(_mongoDatabase)); }
        }

        private IGenericRepository<HashTag> hashTagRepository;
        public IGenericRepository<HashTag> HashTagRepository
        {
            get { return hashTagRepository ?? (hashTagRepository = new GenericRepository<HashTag>(_mongoDatabase)); }
        }

        private IGenericRepository<Notification> notificationRepository;
        public IGenericRepository<Notification> NotificationRepository
        {
            get { return notificationRepository ?? (notificationRepository = new GenericRepository<Notification>(_mongoDatabase)); }
        }

    }
}
