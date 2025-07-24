using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orama_API.DTO;
using Orama_API.Interfaces;
using Orama_API.Services;
using System.Security.Claims;

namespace Orama_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AdminController:ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService; // Initializing the service
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(SignUpRequestDTO sigUpRequestDto)
        {
            try
            {
                var response = await _adminService.RegisterAsync(sigUpRequestDto);
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
                var response = await _adminService.LoginAsync(logInRequestDto);
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

        [Authorize(Roles ="Admin")]
        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllUserAsync()
        {
            try
            {
                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "Admin")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                var response = await _adminService.GetAllUserAsync();
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

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllAdmin")]
        public async Task<IActionResult> GetAllAdminAsync()
        {
            try
            {
                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "Admin")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                var response = await _adminService.GetAllAdminAsync();
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

        [Authorize(Roles = "Admin")]
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            try
            {
                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "Admin")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                var response = await _adminService.GetUserByIdAsync(id);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetByEmail")]
        public async Task<IActionResult> GetUserByEmailAsync([FromQuery] string email)
        {
            try
            {
                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "Admin")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                var response = await _adminService.GetUserByEmailAsync(email);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetByPhone")]
        public async Task<IActionResult> GetUserByPhoneAsync([FromQuery] string phone)
        {
            try
            {
                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "Admin")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                var response = await _adminService.GetUserByPhoneAsync(phone);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("AlterUserStatus/{id}")]
        public async Task<IActionResult> AlterUserStatusAsync(int id)
        {
            try
            {
                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "Admin")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                var response = await _adminService.AlterUserStatusAsync(id);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateUserProfile/{id}")]
        public async Task<IActionResult> UpdateMyProfileAsync(int id,ProfileUpdateUserDTO profileUpdateUserDto)
        {
            try
            {

                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "User")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                var response = await _adminService.UpdateUserProfileAsync(id, profileUpdateUserDto);

                if (response == null)
                    return NotFound(new { message = $"User with Id: {id} not found." });

                return Ok(new
                {
                    Message = "Profile Updated Successfully",
                    Data = response
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
        
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            try
            {
                var roleFromToken = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

                if (string.IsNullOrEmpty(roleFromToken) || roleFromToken != "Admin")
                {
                    return Forbid("Access denied: your role is not authorized to access this resource.");
                }

                var response = await _adminService.DeleteUserAsync(id);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
