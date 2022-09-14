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

        [HttpPut("ChangeUserPass")]
        public async Task<ActionResult<bool>> ChangeUserPass(string userId, string userNewPassword, string adminId, string adminPassword)
        {
            var user = await _bokulousDbService.GetUserAsync(userId);

            if (!(await UserHelpers.CheckIsAdmin(adminId, adminPassword)))
                return Forbid();

            if (user is null || user.Id is null)
                return NotFound("User not found");

            if (UserHelpers.CheckIsPasswordValid(userNewPassword))
                return BadRequest("Invalid password");

            user.Password = userNewPassword;

            await _bokulousDbService.UpdateUserAsync(user.Id, user);

            return Ok(true);
        }
    }
}
