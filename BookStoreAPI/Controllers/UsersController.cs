using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using BookStoreAPI.Contracts;
using BookStoreAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;

        public UsersController(IBookRepository bookRepository,
            ILoggerService logger,
            IMapper mapper,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IConfiguration config)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
        }

        // custom classes
        /// <summary>
        /// Endpoint used to interact with User Register API Controller
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [Route("Register")]
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            var location = GetControllerActionName();
            var username = userDTO.EmailAddress;
            try
            {
                var password = userDTO.Password;
                var user = new IdentityUser { Email = username, UserName = username };
                var result = await _userManager.CreateAsync(user, password);

                _logger.LogInfo($"{location}: Register user {username}");
                if (!result.Succeeded)
                {
                    _logger.LogInfo($"{location}: Register user {username} - Failed");
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"{location}: {error.Code} {error.Description}");
                    }

                    return InternalError($"{location}: Register user {username} - Failed");
                }
                _logger.LogInfo($"{location}: Register user {username} - Success");
                return Ok(new { result.Succeeded });
            }
            catch (Exception e)
            {
                _logger.LogError($"{location}: Register user {username} - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Endpoint used to interact with User Login API Controller
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [Route("Login")]
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            var location = GetControllerActionName();
            var username = userDTO.EmailAddress;
            try
            {
                var password = userDTO.Password;
                _logger.LogInfo($"{location}: Login user {username}");
                var result = await _signInManager.PasswordSignInAsync(username, password, false, false);

                if (result.Succeeded)
                {
                    _logger.LogInfo($"{location}: Login user {username} - Success");
                    var user = await _userManager.FindByNameAsync(username);
                    var tokenString = await GenerateJSONWebToken(user);
                    return Ok(new { token = tokenString });
                }
                _logger.LogInfo($"{location}: Login user {username} - Failed");
                return Unauthorized(userDTO);
            }
            catch (Exception e)
            {
                _logger.LogError($"{location}: Login user {username} - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        // custom classes
        private async Task<string> GenerateJSONWebToken(IdentityUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r)));

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                null,
                //token expired in 15 mins
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );
            return (new JwtSecurityTokenHandler().WriteToken(token));
        }

        private string GetControllerActionName()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Contact your Administrator");
        }
    }
}
