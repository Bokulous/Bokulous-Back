using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bokulous_Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private BokulousDbService _bokulousDbService;
        private UserHelpers UserHelpers;

        public AdminController(BokulousDbService bokulousDbService)
        {
            _bokulousDbService = bokulousDbService;
            UserHelpers = new(_bokulousDbService);
        }

        [HttpPut("ChangeUserPass")]
        public async Task<ActionResult> ChangeUserPass(string userId, string userNewPassword, string adminId, string adminPassword)
        {
            var user = await _bokulousDbService.GetUserAsync(userId);

            if (!(await UserHelpers.CheckIsAdmin(adminId, adminPassword)))
                return Forbid();

            if (user is null || user.Id is null)
                return NotFound("User not found");

            if (!UserHelpers.CheckIsPasswordValid(userNewPassword))
                return BadRequest("Invalid password");

            user.Password = userNewPassword;

            await _bokulousDbService.UpdateUserAsync(user.Id, user);

            return Ok();
        }

        [HttpGet("FindUser")]
        public async Task<ActionResult<List<User>>> FindUser(string keyword, string adminId, string adminPassword)
        {
            if (!await UserHelpers.CheckIsAdmin(adminId, adminPassword))
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            if (string.IsNullOrEmpty(keyword))
                return NotFound("Missing a keyword");

            var allUsers = await _bokulousDbService.GetUserAsync();
            if (allUsers.Count == 0 || allUsers is null)
                return NotFound("No users found");

            var users = allUsers.Where(x => x.Username.Contains(keyword)).ToList();

            if (users.Count == 0 || users is null)
                return NotFound("No user mathing the name found");

            return Ok(users);
        }
    }
}