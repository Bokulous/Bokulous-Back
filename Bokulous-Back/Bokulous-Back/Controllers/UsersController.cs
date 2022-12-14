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

        Random rnd = new Random();
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
        public async Task<ActionResult<User>> GetUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _bokulousDbService.GetUserAsync(id);

            if (user is null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser(User newUser)
        {
            const string ADDRESS = "https://localhost:7204/api";


            if (newUser is null)
                return BadRequest();

            var users = await _bokulousDbService.GetUserAsync();

            foreach (var u in users)
            {
                if(u.Username == newUser.Username)
                    return BadRequest("User with that username already exists");
                if(u.Mail == newUser.Mail)
                    return BadRequest("User with that email already exists");
            }

            if (!userHelper.CheckIsUsernameValid(newUser.Username))
                return BadRequest("Username is not valid");
            
            if (!userHelper.CheckIsPasswordValid(newUser.Password))
                return BadRequest("Password is not valid");

            string code = rnd.Next(100000, 999999).ToString();
            newUser.ActivationCode = code;

            await _bokulousDbService.CreateUserAsync(newUser);
            _bokulousMailService.SendEmail(newUser.Mail, "Welcome to Bokulous", "Welcome to Bokulous, " + newUser.Username + "! Verify your account here: <a href='" + ADDRESS + "/ActivateAccount/" + newUser.Id + "/" + newUser.ActivationCode + "'>here</a>");
            return Ok(newUser);
        }

        [HttpPut("ChangePassword")]
        public async Task<ActionResult> ChangePassword([FromBody] User userNewPassword)
        {
            if (string.IsNullOrEmpty(userNewPassword.Id))
            {
                return NotFound("User not found");
            }
            if (!userHelper.CheckIsPasswordValid(userNewPassword.Password))
            {
                return BadRequest("Invalid password");
            }

            var user = await _bokulousDbService.GetUserAsync(userNewPassword.Id);
            if (user is null)
            {
                return NotFound("User not found");
            }

            user.Password = userNewPassword.Password;
            await _bokulousDbService.UpdateUserAsync(user.Id, user);

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
        public async Task<ActionResult> EditProfile([FromBody] User userEdit)
        {
            var user = await _bokulousDbService.GetUserAsync(userEdit.Id);

            if (user.Password == userEdit.Password)
            {
                user.Username = userEdit.Username;
                user.Mail = userEdit.Mail;

                await _bokulousDbService.UpdateUserAsync(user.Id, user);

                return Ok(user);
            }

            return Forbid("Wrong password");
        }

        [HttpPost("ForgotPassword")]
        public async Task<ActionResult> ForgotPassword([FromBody]RequestForgotPassword data)
        {
            var currentUser = await _bokulousDbService.GetUserMailAsync(data.Mail);

            if (currentUser != null)
            {
                var newPassword = "newpassword" + rnd.Next(100, 1000).ToString();
                currentUser.Password = newPassword;
                await _bokulousDbService.UpdateUserAsync(currentUser.Id, currentUser);
                _bokulousMailService.SendEmail(data.Mail, "New password", $"Your new password is: {newPassword}");
                return Ok(currentUser);
            }

            return NotFound("Mail does not exist");
        }

        [HttpPost("ForgotUsername")]
        public async Task<ActionResult> ForgotUsername([FromBody]RequestForgotPassword data)
        {
            var currentUser = await _bokulousDbService.GetUserMailAsync(data.Mail);

            if (currentUser != null)
            {
                var newUsername = "newusername" + rnd.Next(100, 1000).ToString();
                currentUser.Username = newUsername;
                await _bokulousDbService.UpdateUserAsync(currentUser.Id, currentUser);
                _bokulousMailService.SendEmail(data.Mail, "New username", "Your new username is: " + newUsername);
                return Ok(currentUser);
            }

            return NotFound("Mail does not exist");
        }

        [HttpGet("ActivateAccount/{id}/{code}")]
        public async Task<ActionResult> ActivateAccount(string id, string code)
        {
            var user = await _bokulousDbService.GetUserAsync(id);

            if (user is null)
                return NotFound();

            if (user.ActivationCode == code)
            {
                user.IsActive = true;
                await _bokulousDbService.UpdateUserAsync(user.Id, user);
                return Ok();
            }
            else
            {
                return Forbid("Wrong code");
            }

            return BadRequest();
        }
    }
}