using Bokulous_Back.Helpers;
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
        private UserHelpers UserHelpers;

        public AdminController(BokulousDbService bokulousDbService)
        {
            _bokulousDbService = bokulousDbService;
            UserHelpers = new(_bokulousDbService);
        }

        [HttpPut("InactivateUser")]
        public async Task<ActionResult> InactivateUser(string userId, string adminId, string adminPassword)
        {
            var user = await _bokulousDbService.GetUserAsync(userId);
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (user is null)
                return NotFound("User could not be found");

            if (admin is null)
                return NotFound("Admin could not be found");

            if (!await UserHelpers.CheckIsAdmin(adminId, adminPassword))
                return Forbid("Failed admin check");

            user.IsActive = false;

            _bokulousDbService.UpdateUserAsync(userId, user);

            var check = await _bokulousDbService.GetUserAsync(userId);

            if(check.IsActive)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            return Ok();
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
    }
}
