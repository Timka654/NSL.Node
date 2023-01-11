using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WelcomeServer.Data;
using WelcomeServer.Data.Models;
using WelcomeServer.Managers.Interfaces;

namespace WelcomeServer.Controllers
{
    [Controller]
    public class UsersController : Controller
    {
        private readonly IUserManager _userManager;

        public UsersController(IUserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] string username, Guid id)
        {
            var user = await _userManager.SigninAsync(username, id);
            return Ok(user);
        }

    
    }
}
