using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bokulous_Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IBokulousDbService _bokulousDbService;
        private IBokulousMailService _bokulousMailService;
        private UserHelpers userHelper;

        public UsersController(IBokulousDbService bokulousDbService, IBokulousMailService bokulousMailService)
        {
            _bokulousDbService = bokulousDbService;
            _bokulousMailService = bokulousMailService;
            userHelper = new(_bokulousDbService);
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

        [HttpPost("Register")]
        public async Task<ActionResult> Register(User newUser)
        {
            if (!userHelper.CheckIsUsernameValid(newUser.Username))
                return BadRequest("Username is not valid");

            if (!userHelper.CheckIsPasswordValid(newUser.Password))
                return BadRequest("Password is not valid");

            await _bokulousDbService.CreateUserAsync(newUser);

            return Ok();
        }

        [HttpGet("ShowProfile/{id:length(24)}")]
        public async Task<ActionResult<User>> ShowProfile(User user)
        {
            var profile = await _bokulousDbService.GetUserAsync(user.Id);

            if (profile is null)
                return NotFound();

            profile.Password = "";

            return Ok(profile);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] User userLogin)
        {
            var user = await _bokulousDbService.LoginAsync(userLogin);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound("User not found");
        }

        [HttpPost("EditProfile")]
        public async Task<ActionResult> EditProfile(string id, string username, string email, string password)
        {
            var user = await _bokulousDbService.GetUserAsync(id);

            if (user.Password == password)
            {
                user.Username = username;
                user.Mail = email;

                await _bokulousDbService.UpdateUserAsync(user.Id, user);

                return Ok(user);
            }

            return Forbid("Wrong password");
        }
    }
}