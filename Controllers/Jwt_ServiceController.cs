using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orama_API.DTO;
using Orama_API.Interfaces;
using System.Security.Claims;

namespace Orama_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Jwt_ServiceController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        public Jwt_ServiceController(IJwtService jwtService)
        {
            _jwtService = jwtService; // Initializing the service
        }
        
        [HttpPost("ValidateAndDebugToken")]
        public IActionResult ValidateAndDebugToken([FromBody] string token)
        {
            try
            {
                var debugResult = _jwtService.ValidateAndDebugToken(token);
                return Ok(new { message = "Token debug result", result = debugResult });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error debugging token", error = ex.Message });
            }
        }
        [HttpPost("ValidateTokenClaims")]
        public IActionResult ValidateTokenClaims([FromBody] string token)
        {
            try
            {
                var debugResult = _jwtService.DecodeTokenClaims(token);
                return Ok(new { message = "Token debug result", result = debugResult });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error debugging token", error = ex.Message });
            }
        }

    }
}
