using Bokulous_Back.Helpers;
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

        [HttpDelete("PurgeBook")]
        public async Task<ActionResult> PurgeBook(string bookId, string adminId, string password)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);
            var book = await _bokulousDbService.GetBookAsync(bookId);

            if (admin is null)
                return NotFound("User not found");

            if (book is null)
                return NotFound("Book not found");

            if (!await UserHelpers.CheckIsAdmin(admin.Id, admin.Password))
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            await _bokulousDbService.RemoveBookAsync(book.Id);

            return Ok();
        }
    }
}