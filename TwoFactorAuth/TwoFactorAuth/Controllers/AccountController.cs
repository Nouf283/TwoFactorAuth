﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TwoFactorAuth.Dtos;
using TwoFactorAuth.Entities;
using TwoFactorAuth.Interfaces;
using WebApp.Services;

namespace TwoFactorAuth.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenServices _tokenServices;
        private readonly IEmailService _emailService;
        public EmailMFA EmailMFA { get; set; }

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, ITokenServices tokenServices, IEmailService emailService
            )
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._tokenServices = tokenServices;
            this._emailService = emailService;
            this.EmailMFA = new EmailMFA();
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized();
            }
            var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, true, false);
              
          //  var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
           
            if (result.Succeeded)
            {
                return Unauthorized();
            }
            else
            {
                if (!result.RequiresTwoFactor)
                {
                    var user1 = await _userManager.FindByEmailAsync(loginDto.Email);

                    this.EmailMFA.SecurityCode = string.Empty;
                    this.EmailMFA.RememberMe = true;

                    // Generate the code
                    var securityCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                    // Send to the user
                    await _emailService.SendAsync("noufawal0311@gmail.com",
                        loginDto.Email,
                        "My Web App's OTP",
                        $"Please use this code as the OTP: {securityCode}");

                    return
                        new UserDto
                        {
                            Email = loginDto.Email,
                            UserName = user.UserName,
                            Token = _tokenServices.CreateToken(user),
                            IsTwoFactor = true
                        };

                }
                else
                {
                    return new UserDto
                    {
                        Email = loginDto.Email,
                        UserName = user.UserName,
                        Token = _tokenServices.CreateToken(user),
                        IsTwoFactor = false
                    };

                }

            }
            
           
        }

        [HttpPost]
        [Route("twoFactorLoginPost")]
        public async Task<ActionResult<UserDto>> TwoFactorLoginPost(Credential credential)
        {
            //  if (!ModelState.IsValid) return Page();
            var user = await _userManager.FindByEmailAsync(credential.Email);
            var result = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", credential.Securitycode);
            //var result = await _signInManager.TwoFactorSignInAsync("Email",
            //    credential.Securitycode,
            //    true,
            //    false);

            if (result== true)
            {
                return new UserDto
                {
                    Email = credential.Email,
                    UserName = user.UserName,
                    Token = _tokenServices.CreateToken(user),
                    Id = user.Id
                };
                //return null;
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            string confirmationToken = null;
            var user = new AppUser
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                FirstName = registerDto.UserName,
                TwoFactorEnabled=true
            };
            try
            {
                var clientURI = "https://localhost:44312/Account/ConfirmEmail";
                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    return Unauthorized();
                }
                confirmationToken = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var param = new Dictionary<string, string?>
                        {
                            {"token", confirmationToken },
                            {"userId", user.Id }
                        };
                var callback = QueryHelpers.AddQueryString(clientURI, param);

                await _emailService.SendAsync("cleinttest123@gmail.com",
                    user.Email,
                    "Please confirm your email",
                    $"Please click on this link to confirm your email address: {callback}");
            }
            catch (Exception ex)
            {
                new Exception(ex.Message);
            }

            return new UserDto
            {
                Email = registerDto.Email,
                UserName = user.UserName,
                // Token = _tokenServices.CreateToken(user),
                Token = confirmationToken,
                Id = user.Id
            };
        }

        [HttpGet]
        [Route("ConfirmEmail")]
        public async Task<ActionResult<UserDto>> ConfirmEmail([FromQuery] string token, [FromQuery] string userId)
        {
            var userEntity = await _userManager.FindByIdAsync(userId);
            var confirmResult = await _userManager.ConfirmEmailAsync(userEntity, token);

            return null;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = HttpContext.User?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;
            //  var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            return new UserDto
            {
                Email = user.Email,
                UserName = user.UserName,
                Token = _tokenServices.CreateToken(user)
            };

        }
    }
}

public class Credential
{
    public string Email { get; set; }
    public string  Securitycode { get; set; }

}