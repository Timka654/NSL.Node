using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WelcomeServer.Data;
using WelcomeServer.Data.DTO;
using WelcomeServer.Data.Models;
using WelcomeServer.Managers;
using WelcomeServer.Managers.Interfaces;

namespace WelcomeServer.Controllers
{
    [Controller]
    public class UsersController : Controller
    {
        private readonly IUserManager _userManager;
        private readonly LobbyManager _lobbyManager;

        public UsersController(IUserManager userManager, LobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
            _userManager = userManager;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] UserCredentialDTO userCredential)
        {
            var user = await _userManager.SigninAsync(userCredential.Username, userCredential.Password);
            if (user == null)
            {
                return BadRequest("Username or password is incorrect");
            }

            
            var temporaryKeyForSocket = await _lobbyManager.RegisterTemporaryKey(userCredential.Username);
            return Ok(temporaryKeyForSocket);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Signup([FromBody] UserCredentialDTO userCredential)
        {
            var user = await _userManager.SignupAsync(userCredential.Username, userCredential.Password);
            if (user == null)
            {
                return BadRequest("Username is already taken");
            }

            return Ok(user);
        }


    }
}
