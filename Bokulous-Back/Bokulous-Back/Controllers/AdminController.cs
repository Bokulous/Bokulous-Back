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
    }
}
