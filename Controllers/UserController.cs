using Microsoft.AspNetCore.Mvc;
using Orama_API.DTO;
using Orama_API.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace Orama_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService; // Initializing the service
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(SignUpRequestDTO signUpRequestDto)
        {
            try
            {
                var response = await _userService.RegisterAsync(signUpRequestDto);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        [HttpPost("Authorize")]
        public async Task<IActionResult> LoginAsync(LoginRequestDTO logInRequestDto)
        {
            try
            {
                var response = await _userService.LoginAsync(logInRequestDto);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> PasswordAsync(ChangePasswordRequestDTO changePasswordRequestDto)
        {
            try
            {
                var response = await _userService.PasswordAsync(changePasswordRequestDto);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        // Secure authenticated endpoints - these require JWT token

        [Authorize(Roles = "User")]
        [HttpPatch("UpdateProfile")]
        public async Task<IActionResult> UpdateMyProfileAsync(ProfileUpdateUserDTO profileUpdateUserDto)
        {
            try
            {
                // Get email from the authenticated token using custom claim name
                var emailFromToken = User.FindFirstValue("UserEmail");
                var userIdFromToken = User.FindFirstValue("UserId");
                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(emailFromToken))
                {
                    return Unauthorized(new { message = "Invalid user token, no Email" });
                }
                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "User")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                if (!int.TryParse(userIdFromToken, out int userIdFromJwt))
                {
                    return Unauthorized("Invalid or missing UserId claim in token.");
                }

                var response = await _userService.UpdateProfileAsync(emailFromToken, profileUpdateUserDto);

                if (response == null)
                    return NotFound(new { message = $"User with email {emailFromToken} not found." });

                return Ok(new{ Message = "Profile Updated Successfully",
                          Data = response});
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the profile", error = ex.Message });
            }
        }

        [Authorize(Roles ="User")]
        [HttpGet("MyProfile")]
        public async Task<IActionResult> GetMyProfileAsync()
        {
            try
            {
                // Get email from the authenticated token using custom claim name
                var emailFromToken = User.FindFirstValue("UserEmail");
                var userIdFromToken = User.FindFirstValue("UserId");
                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(emailFromToken))
                {
                    return Unauthorized(new { message = "Invalid user token, no Email claim found" });
                }
                
                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "User")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                if (!int.TryParse(userIdFromToken, out int userIdFromJwt))
                {
                    return Unauthorized("Invalid or missing UserId claim in token.");
                }

                var response = await _userService.GetMyProfileByEmailAsync(emailFromToken);
                
                if (response == null)
                    return NotFound(new { message = $"User with email {emailFromToken} not found." });

                return Ok(new 
                { 
                    message = "Profile retrieved successfully",
                    data = response
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the profile", error = ex.Message });
            }
        }

        [Authorize(Roles = "User")]
        [HttpDelete("DeleteMyProfile")]
        public async Task<IActionResult> DeleteMyProfileAsync()
        {
            try
            {
                
                var userIdFromToken = User.FindFirstValue("UserId");
                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "User")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                if (!int.TryParse(userIdFromToken, out int userIdFromJwt))
                {
                    return Unauthorized("Invalid or missing UserId claim in token.");
                }

                var response = await _userService.DeleteMyProfileAsync(userIdFromJwt);

                if (response == null)
                    return NotFound(new { message = $"User with email {userIdFromJwt} not found." });

                return Ok(new
                {
                    message = "Profile retrieved successfully",
                    data = response
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the profile", error = ex.Message });
            }
        }

    }
}
