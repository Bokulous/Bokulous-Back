using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bokulous_Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private BokulousDbService _bokulousDbService;

        public UsersController(BokulousDbService bokulousDbService)
        {
            _bokulousDbService = bokulousDbService;
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

        [HttpGet("ShowProfile/{id:length(24)}")]
        public async Task<ActionResult<User>> ShowProfile(User user)
        {
            var profile = await _bokulousDbService.GetUserAsync(user.Id);

            if (profile is null)
                return NotFound();

            profile.Password = null;

            return Ok(profile);
        }

        // editprofile
        //parametrar- Username, email, password,description (behövs?)
        [HttpGet("EditProfile/{id:length(24)}")]
        public async Task<ActionResult<User>> EditProfile(string id, User user)
        {
            var updatedProfile = await _bokulousDbService.UpdateUserAsync(id, user.Id);

            //if (profile is null)
            //    return NotFound();

            //profile.Password = null;

            return Ok(updatedProfile);
        }
    }
}
