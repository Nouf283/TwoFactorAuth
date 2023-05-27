using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, ITokenServices tokenServices, IEmailService emailService
            )
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._tokenServices = tokenServices;
            this._emailService = emailService;
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
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
           
            if (!result.Succeeded)
            {
                return Unauthorized();
            }
            if (result.RequiresTwoFactor)
            {
                return
                    new UserDto
                    {
                        Email = loginDto.Email,
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
                    IsTwoFactor=false
                };

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
            };
            try
            {
                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    return Unauthorized();
                }
                confirmationToken = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.PageLink(pageName: "/Account/ConfirmEmail",
                    values: new { userId = user.Id, token = confirmationToken });

                await _emailService.SendAsync("noufawal0311@gmail.com",
                    user.Email,
                    "Please confirm your email",
                    $"Please click on this link to confirm your email address: {confirmationLink}");
            }
            catch(Exception ex)
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
