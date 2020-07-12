using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextLevelTrainingApi.Services;
using NextLevelTrainingApi.Models;

namespace NextLevelTrainingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private UserService userService;

        public UsersController(UserService _userService)
        {
            userService = _userService;
        }

        [HttpGet]
        public ActionResult<List<Users>> Get() =>
            userService.Get();
        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public ActionResult<Users> Get(string id)
        {
            var user = userService.Get(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public ActionResult<Users> Create(Users user)
        {
            userService.Create(user);

            return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Users bookIn)
        {
            var book = userService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            userService.Update(id, bookIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var book = userService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            userService.Remove(book.Id);

            return NoContent();
        }
    }
}
