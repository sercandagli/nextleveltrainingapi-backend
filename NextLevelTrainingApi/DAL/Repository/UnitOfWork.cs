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

    }
}
