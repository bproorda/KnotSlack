﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using signalrApi.Data;
using signalrApi.Hubs;
using signalrApi.Models.DTO;
using signalrApi.Models.Identity;
using signalrApi.services;
using signalrApi.Repositories.UserChannelRepos;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace signalrApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private knotSlackDbContext _context;
        private readonly IUserManager userManager;
        private IUserChannelRepository userChannelRepository;
        private IChatHub chatHub;

        public UsersController(IUserManager userManager, IChatHub chatHub, knotSlackDbContext _context, IUserChannelRepository userChannelRepository)
        {
            this.userManager = userManager;
            this.chatHub = chatHub;
            this._context = _context;
            this.userChannelRepository = userChannelRepository;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginData login)
        {
            var user = await userManager.FindByNameAsync(login.UserName);
            if (user != null)
            {
                var result = await userManager.CheckPasswordAsync(user, login.Password);
                if (result)
                {
                    user.LoggedIn = true;
                    await userManager.UpdateAsync(user);

                    var channels = await userChannelRepository.GetUserChannels(user);

                    return Ok(new UserWithToken
                    {
                        UserId = user.UserName,
                        Token = userManager.CreateToken(user),
                        Channels = channels,

                    });
                }

                await userManager.AccessFailedAsync(user);
            }
            return Unauthorized();
        }
        [Authorize]
        //To update LoggedIn prop in db
        [HttpPost("Logout")]
        public async Task<string> Logout(userDTO userInfo)
        {
            var user = await userManager.FindByNameAsync(userInfo.Username);
            if (user != null)
            {
                user.LoggedIn = false;
                await userManager.UpdateAsync(user);

                //comment out if using postman
                
                await chatHub.DisplayUsers();
                return user.UserName;

            }
            return "User Not Found";
        }

        [HttpPost("Register")]

        public async Task<IActionResult> Register(RegisterData register)
        {
            var user = new ksUser
            {
                Email = register.Email,
                UserName = register.Email,
                LoggedIn = true,

            };

            var result = await userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "registration failed",
                    errors = result.Errors
                });
            }

            return Ok(new UserWithToken
            {
                UserId = user.Id,
                Token = userManager.CreateToken(user)
            });


        }

        [Authorize]
        [HttpGet("Self")]
        public async Task<IActionResult> Self()
        {
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                var usernameClaim = identity.FindFirst("UserId");
                var userId = usernameClaim.Value;

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized();
                }

                return Ok(new
                {
                    user.Email,

                });
            }
            return Unauthorized();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                UserId = user.Id,
                user.Email,

            });
        }

        [HttpPut("userId")]

        public async Task<IActionResult> UpdateUser(string userId, ksUser data)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();


            await userManager.UpdateAsync(user);

            return Ok(new
            {
                UserId = user.Id,
                user.Email,

            });
        }
        
        [HttpGet("users")]
        public async Task<userListDTO[]> users()
        {
                var users = await _context.Users
                .Select(user => new userListDTO
                    {
                     Username = user.Email,
                     LoggedIn = user.LoggedIn
                        }).ToListAsync();

            return users.ToArray();
        }
    }
}
