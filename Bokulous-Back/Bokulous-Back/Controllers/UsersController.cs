using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Mvc;
using Bokulous_Back.Helpers;

namespace Bokulous_Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private BokulousDbService _bokulousDbService;
        private UserHelpers UserHelpers;

        public UsersController(BokulousDbService bokulousDbService)
        {
            _bokulousDbService = bokulousDbService;
            UserHelpers = new(_bokulousDbService);
        }

        [HttpGet("GetUsers")]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            var users = await _bokulousDbService.GetUserAsync();

            if (users is null)
                return NotFound();

            return Ok(users);
        }

        [HttpGet("GetUser/{id:length(24)}")]
        public async Task<ActionResult<List<User>>> GetUser(string id)
        {
            var user = await _bokulousDbService.GetUserAsync(id);

            if (user is null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("AddUser")]
        public async Task<ActionResult> AddUser(User newUser)
        {
            await _bokulousDbService.CreateUserAsync(newUser);

            return Ok();
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(string id, string newPassword)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("User not found");
            }
            if (!UserHelpers.CheckIsPasswordValid(newPassword))
            {
                return BadRequest("Invalid password");
            }

            var user = await _bokulousDbService.GetUserAsync(id);
            if (user is null)
            {
                return NotFound("User not found");
            }

            user.Password = newPassword;

            await _bokulousDbService.UpdateUserAsync(user.Id, user);

            return Ok();
        }
    }
}
