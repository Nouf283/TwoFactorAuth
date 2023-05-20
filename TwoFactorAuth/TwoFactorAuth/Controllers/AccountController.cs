using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TwoFactorAuth.Dtos;
using TwoFactorAuth.Entities;
using TwoFactorAuth.Interfaces;

namespace TwoFactorAuth.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenServices _tokenServices;

        public AccountController(UserManager<AppUser>userManager,
            SignInManager<AppUser> signInManager,ITokenServices tokenServices)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._tokenServices = tokenServices;
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if(user== null)
            {
                return Unauthorized();
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password,false);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }
            return new UserDto
            {
                Email = loginDto.Email,
                UserName = user.UserName,
                Token = _tokenServices.CreateToken(user)
            };
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            var user = new AppUser
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                FirstName = registerDto.UserName,
            };
           
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }
            return new UserDto
            {
                Email = registerDto.Email,
                UserName = user.UserName,
                Token = _tokenServices.CreateToken(user)
            };
        }
    }
}
