using Microsoft.AspNetCore.Mvc;
using Orama_API.Data;
using Orama_API.DTO;
using Orama_API.Interfaces;
using Orama_API.Services;

namespace Orama_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Email_ServiceController:ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly UserDbContext _context;
        public Email_ServiceController(IEmailService emailService, UserDbContext context)
        {
            _emailService = emailService;
            _context = context;
        }
        [HttpPost("IsEmailValid")]
        public async Task<IActionResult> IsEmailValidAsync(string Email)
        {
            try
            {
                var response = await _emailService.IsEmailValidAsync(Email);
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
        [HttpPost("IsEmailRegistered")]
        public async Task<IActionResult> IsEmailRegisteredAsync(string Email)
        {
            try
            {
                var response = await _emailService.IsEmailRegisteredAsync(Email);
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

        [HttpGet("IsEmailVerify/{Email}")]
        public async Task<IActionResult> IsEmailVerifiedAsync(string Email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Email))
                    return BadRequest(new { message = "Email is required" });

                var response = await _emailService.IsEmailVerifiedAsync(Email);
                return Ok(new { Verified = response });
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

        [HttpPost("SendEmailOTP")]
        public async Task<IActionResult> SendOTPAsync([FromBody] EmailOTPRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { message = "Email is required" });

                var response = await _emailService.SendOTPAsync(request.Email);
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

        [HttpPost("VerifyEmailOTP")]
        public async Task<IActionResult> VerifyEmailOTPAsync([FromBody] EmailOTPVerifyDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { message = "Email is required" });

                if (string.IsNullOrWhiteSpace(request.OTP))
                    return BadRequest(new { message = "OTP is required" });

                var response = await _emailService.VerifyEmailOTPAsync(request.Email, request.OTP);
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
        [HttpPost("ResendEmailOTP")]
        public async Task<IActionResult> ResendEmailOTPAsync([FromBody] EmailOTPRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { message = "Email is required" });

                var response = await _emailService.ResendEmailOTPAsync(request.Email);
                return Ok(new
                {
                    message = response ? "OTP resent successfully" : "Failed to resend OTP",
                    success = response
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
        }
        [HttpPost("DebugOTP")]
        public async Task<IActionResult> DebugOTPAsync([FromBody] EmailOTPRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { message = "Email is required" });

                var response = await _emailService.DebugOTPAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }        
    }
}
