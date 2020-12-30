using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NextLevelTrainingApi.Models;
using MongoDB.Driver;
using NextLevelTrainingApi.Services.Interfaces;
using NextLevelTrainingApi.DAL.Interfaces;

namespace NextLevelTrainingApi.Services
{
    public class UserService: IUserService
    {

        private IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public List<Users> Get()
        { return users.Find(user => true).ToList(); }

        public Users Get(string id)
        { return users.Find<Users>(user => user.Id == id).FirstOrDefault(); }

        public Users Create(Users user)
        {
            users.InsertOne(user);
            return user;
        }

        public void Update(string id, Users userIn)
        {
            users.ReplaceOne(user => user.Id == id, userIn);
        }

        public void Remove(Users userIn)
        {
            users.DeleteOne(user => user.Id == userIn.Id);
        }

        public void Remove(string id)
        {
            users.DeleteOne(user => user.Id == id);
        }
    }
}
