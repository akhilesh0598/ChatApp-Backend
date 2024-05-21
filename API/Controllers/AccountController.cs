using API.Dtos.Requests;
using API.Dtos.Responses;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;

        public AccountController(UserManager<ApplicationUser> userManager,
            TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login(LoginRequestDto loginDto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);

            if (user == null)
                return Unauthorized();

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                return Unauthorized();
            }
            return CreateUserObject(user);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(RegisterRequestDto registerDto)
        {
            if (await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
            {
                ModelState.AddModelError("email", "email taken");
                return ValidationProblem();
            }

            var user = new ApplicationUser
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = Guid.NewGuid().ToString()
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (result.Succeeded)
            {
                return CreateUserObject(user);
            }
            return BadRequest(result.Errors);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));
            return CreateUserObject(user);

        }

        private UserResponseDto CreateUserObject(ApplicationUser user)
        {
            return new UserResponseDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = _tokenService.CreateToken(user),
                Username = user.UserName
            };
        }
    }
}
