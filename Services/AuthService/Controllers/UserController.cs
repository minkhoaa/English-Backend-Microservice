using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AuthService.Dto;
using AuthService.IService;
using AuthService.Service;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPost("/register")]
        public async Task<IActionResult> Register(RequestRegister model)
        {

            var result = await _userService.RegisterAsync(model);
            return (result.Succeeded == true) ? Ok(new { Success = true, Message = "Create account successfully" })
            : BadRequest(new { Success = false, Message = result.Errors.Select(e => e.Description).ToList() }) ;

        }
    }
}