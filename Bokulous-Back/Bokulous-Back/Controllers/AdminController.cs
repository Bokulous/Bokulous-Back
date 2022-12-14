using Bokulous_Back.Helpers;
using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Bokulous_Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private IBokulousDbService _bokulousDbService;
        private IBokulousMailService? _bokulousMailService;
        private UserHelpers UserHelpers;

        public AdminController(IBokulousDbService bokulousDbService, IBokulousMailService bokulousMailService)
        {
            _bokulousDbService = bokulousDbService;
            _bokulousMailService = bokulousMailService;
            UserHelpers = new(_bokulousDbService);
        }

        [HttpGet("GetBooksAdmin")]
        public async Task<ActionResult<List<Book>>> GetBooksAdmin()
        {
            var books = await _bokulousDbService.GetBookAsync();
            if (books is null)
                return NotFound();

            return Ok(books);
        }

        [HttpPut("InactivateUser/{userId}/{adminId}/{adminPassword}")]
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

            await _bokulousDbService.UpdateUserAsync(userId, user);

            var check = await _bokulousDbService.GetUserAsync(userId) ?? throw new Exception("Db connection error");

            if (check.IsActive)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            return Ok();
        }

        [HttpPut("ChangeUserPass/{userId}/{userNewPassword}/{adminId}/{adminPassword}")]
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

        [HttpDelete("PurgeBook/{bookId}/{adminId}/{password}")]
        public async Task<ActionResult> PurgeBook(string bookId, string adminId, string password)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);
            var book = await _bokulousDbService.GetBookAsync(bookId);

            if (admin is null)
                return NotFound("User not found");

            if (book is null)
                return NotFound("Book not found");

            if (!await UserHelpers.CheckIsAdmin(admin.Id, password))
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            if (!await UserHelpers.CheckIsAdmin(admin.Id, admin.Password))
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            await _bokulousDbService.RemoveBookAsync(book.Id);

            return Ok();
        }

        [HttpDelete("PurgeEmptyBooks")]
        public async Task<IActionResult> PurgeEmptyBooks(string adminId, string password)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);
            var books = await _bokulousDbService.GetBookAsync();

            if (admin is null)
                return NotFound("User not found");

            if (books is null)
                return NotFound("No Books found");

            if (!await UserHelpers.CheckIsAdmin(admin.Id, password))
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            books.ForEach(async (book) =>
            {
                if (book.InStorage < 1)
                    await _bokulousDbService.RemoveBookAsync(book.Id);
            });

            books = await _bokulousDbService.GetBookAsync();
            bool removedAll = true;

            books.ForEach(book =>
            {
                if (book.InStorage < 1)
                    removedAll = true;
            });

            if (!removedAll)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            return Ok();
        }

        [HttpPut("SetAmount")]
        public async Task<IActionResult> SetAmount(int amount, string bookId, string? adminId, string password)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);
            var book = await _bokulousDbService.GetBookAsync(bookId);

            if (admin is null)
                return NotFound("User not found");

            if (book is null)
                return NotFound("Book not found");

            if (!await UserHelpers.CheckIsAdmin(admin.Id, password))
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            book.InStorage = amount;

            await _bokulousDbService.UpdateBookAsync(bookId, book);

            if ((await _bokulousDbService.GetBookAsync(book.Id)).InStorage != amount)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            return Ok();
        }

        [HttpGet("ListUsers/{adminId}/{password}")]
        public async Task<ActionResult<List<User>>> ListUsers(string adminId, string password)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (admin is null)
                return NotFound("User not found");

            if (!await UserHelpers.CheckIsAdmin(admin.Id, password))
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            var users = await _bokulousDbService.GetUserAsync();

            return Ok(users);
        }

        [HttpPut("BlockUser/{userId}/{adminId}/{password}")]
        public async Task<ActionResult> BlockUser(string userId, string adminId, string password)
        {
            var user = await _bokulousDbService.GetUserAsync(userId);
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (user is null)
                return NotFound("User could not be found");

            if (admin is null)
                return NotFound("Admin could not be found");

            if (!await UserHelpers.CheckIsAdmin(adminId, password))
                return Forbid("Failed admin check");

            user.IsBlocked = true;

            await _bokulousDbService.UpdateUserAsync(userId, user);

            var check = await _bokulousDbService.GetUserAsync(userId);

            if (!check.IsBlocked)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            return Ok();
        }

        [HttpPut("UnblockUser/{userId}/{adminId}/{password}")]
        public async Task<ActionResult> UnblockUser(string userId, string adminId, string password)
        {
            var user = await _bokulousDbService.GetUserAsync(userId);
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (user is null)
                return NotFound("User could not be found");

            if (admin is null)
                return NotFound("Admin could not be found");

            if (!await UserHelpers.CheckIsAdmin(adminId, password))
                return Forbid("Failed admin check");

            user.IsBlocked = false;

            await _bokulousDbService.UpdateUserAsync(userId, user);

            var check = await _bokulousDbService.GetUserAsync(userId);

            if (check.IsBlocked)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            return Ok();
        }

        [HttpPut("Demote/{userId}/{adminId}/{password}")]
        public async Task<ActionResult> Demote(string userId, string adminId, string password)
        {
            var user = await _bokulousDbService.GetUserAsync(userId);
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (user is null)
                return NotFound("User could not be found");

            if (admin is null)
                return NotFound("Admin could not be found");

            if (!await UserHelpers.CheckIsAdmin(adminId, password))
                return Forbid("Failed admin check");

            user.IsAdmin = false;

            await _bokulousDbService.UpdateUserAsync(userId, user);

            var check = await _bokulousDbService.GetUserAsync(userId);

            if (check.IsAdmin)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            return Ok();
        }

        [HttpPut("Promote/{userId}/{adminId}/{password}")]
        public async Task<ActionResult> Promote(string userId, string adminId, string password)
        {
            var user = await _bokulousDbService.GetUserAsync(userId);
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (user is null)
                return NotFound("User could not be found");

            if (admin is null)
                return NotFound("Admin could not be found");

            if (!await UserHelpers.CheckIsAdmin(adminId, password))
                return Forbid("Failed admin check");

            user.IsAdmin = true;

            await _bokulousDbService.UpdateUserAsync(userId, user);

            var check = await _bokulousDbService.GetUserAsync(userId);

            if (!check.IsAdmin)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            return Ok();
        }

        [HttpPost("BestCustomer")]
        public async Task<ActionResult<User>> BestCustomer(string adminId, string password)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (admin is null)
                return NotFound("Admin could not be found");

            if (!await UserHelpers.CheckIsAdmin(adminId, password))
                return Forbid("Failed admin check");

            var users = (await _bokulousDbService.GetUserAsync());
            var orders = new List<dynamic>();

            foreach (var user in users)
            {
                orders.Add(new { User = user, Sum = user.Previous_Orders.Sum(order => order.Price) });
            }

            var result = orders.Select(order => new { User = (User)order.User, Sum = (double)order.Sum }).MaxBy(order => order.Sum);

            Console.WriteLine(result.User);

            return Ok(result.User);
        }

        [HttpGet("FindUser/{keyword}/{adminId}/{adminPassword}")]
        public async Task<ActionResult<List<User>>> FindUser(string keyword, string adminId, string adminPassword)
        {
            if (!await UserHelpers.CheckIsAdmin(adminId, adminPassword))
                return new StatusCodeResult(StatusCodes.Status403Forbidden);

            if (string.IsNullOrEmpty(keyword))
                return NotFound("Missing a keyword");

            var allUsers = await _bokulousDbService.GetUserAsync();
            if (allUsers.Count == 0 || allUsers is null)
                return NotFound("No users found");

            var users = allUsers.Where(x => x.Username.ToLower().Contains(keyword.ToLower())).ToList();

            if (users.Count == 0 || users is null)
                return NotFound("No user mathing the name found");

            return Ok(users);
        }

        [HttpPost("SoldItems")]
        public async Task<ActionResult<double>> SoldItems(string adminId, string password)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (admin is null)
                return NotFound("Admin could not be found");

            if (!await UserHelpers.CheckIsAdmin(adminId, password))
                return Forbid("Failed admin check");

            var users = await _bokulousDbService.GetUserAsync();
            double sum = 0;

            users.ForEach(user =>
            {
                var soldItems = user.Previous_Books_Sold.ToList();

                soldItems.ForEach(item => sum += item.Price);
            });

            return Ok(sum);
        }
        [HttpPost("SoldItemsYear")]
        public async Task<ActionResult<double>> SoldItems(int year, string adminId, string password)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (admin is null)
                return NotFound("Admin could not be found");

            if (!await UserHelpers.CheckIsAdmin(adminId, password))
                return Forbid("Failed admin check");

            var users = await _bokulousDbService.GetUserAsync();
            double sum = 0;

            users.ForEach(user =>
            {
                var soldItems = user.Previous_Books_Sold.ToList();

                soldItems.ForEach(item =>
                {
                    if (item.Date.Year == year)
                        sum += item.Price;
                });
            });

            return Ok(sum);
        }
        [HttpPost("SoldItemsMonth")]
        public async Task<ActionResult<double>> SoldItems(int year, int month, string adminId, string password)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (admin is null)
                return NotFound("Admin could not be found");

            if (!await UserHelpers.CheckIsAdmin(adminId, password))
                return Forbid("Failed admin check");

            var users = await _bokulousDbService.GetUserAsync();
            double sum = 0;

            users.ForEach(user =>
            {
                var soldItems = user.Previous_Books_Sold.ToList();

                soldItems.ForEach(item =>
                {
                    if (item.Date.Year == year && item.Date.Month == month)
                        sum += item.Price;
                });
            });

            return Ok(sum);
        }
        [HttpPost("SoldItemsDay")]
        public async Task<ActionResult<double>> SoldItems(int year, int month, int day, string adminId, string password)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            if (admin is null)
                return NotFound("Admin could not be found");

            if (!await UserHelpers.CheckIsAdmin(adminId, password))
                return Forbid("Failed admin check");

            var users = await _bokulousDbService.GetUserAsync();
            double sum = 0;

            users.ForEach(user =>
            {
                var soldItems = user.Previous_Books_Sold.ToList();

                soldItems.ForEach(item =>
                {
                    if (item.Date.Year == year && item.Date.Month == month && item.Date.Day == day)
                        sum += item.Price;
                });
            });

            return Ok(sum);
        }
    }
}