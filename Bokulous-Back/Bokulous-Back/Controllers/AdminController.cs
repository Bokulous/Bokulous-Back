using Bokulous_Back.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bokulous_Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private BokulousDbService _bokulousDbService;

        public AdminController(BokulousDbService bokulousDbService)
        {
            _bokulousDbService = bokulousDbService;
        }

        private async Task<bool> CheckIsAdmin(string adminId, string adminPassword)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            return admin is not null && admin.Password == adminPassword && admin.IsAdmin;
        }

        private bool CheckIsPasswordValid(string password)
        {
            const int PASS_LENGTH = 6;

            if (password is null)
                return false;

            if (password.Length < PASS_LENGTH)
                return false;

            return true;
        }


        public async Task<ActionResult<bool>> ChangeUserPass(string userId, string userNewPassword, string adminId, string adminPassword)
        {
            var user = await _bokulousDbService.GetUserAsync(userId);

            if (!(await CheckIsAdmin(adminId, adminPassword)))
                return Forbid();

            if (user is null || user.Id is null)
                return NotFound("User not found");

            if (CheckIsPasswordValid(userNewPassword))
                return BadRequest("Invalid password");

            user.Password = userNewPassword;

            await _bokulousDbService.UpdateUserAsync(user.Id, user);

            return Ok(true);
        }

        //[HttpGet("GetUsers")]
        //public async Task<ActionResult<List<User>>> GetUsers()
        //{
        //    var users = await _bokulousDbService.GetUserAsync();

        //    if (users is null)
        //        return NotFound();

        //    return Ok(users);
        //}

        //[HttpGet("GetUser/{id:length(24)}")]
        //public async Task<ActionResult<List<User>>> GetUser(string id)
        //{
        //    var user = await _bokulousDbService.GetUserAsync(id);

        //    if (user is null)
        //        return NotFound();

        //    return Ok(user);
        //}


        //[HttpPost("AddUser")]
        //public async Task<ActionResult> AddUser(User newUser)
        //{
        //    await _bokulousDbService.CreateUserAsync(newUser);

        //    return Ok();
        //}
    }
}
