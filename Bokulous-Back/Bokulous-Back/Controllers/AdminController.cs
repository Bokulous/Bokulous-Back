﻿using Bokulous_Back.Services;
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