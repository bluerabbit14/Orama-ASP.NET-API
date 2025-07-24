using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Orama_API.Data;
using Orama_API.DTO;
using Orama_API.Interfaces;
using Orama_API.Domain;
using System.Net;
using System.Net.Mail;

namespace Orama_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;

        public EmailService(UserDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        
        public async Task<bool> IsEmailValidAsync(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> IsEmailRegisteredAsync(string Email)
        {
            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null)
                return false;

            if (!user.IsActive)
                throw new InvalidOperationException("User is not active");

            return true;
        }
        
        public async Task<bool> IsEmailVerifiedAsync(string email)
        {
            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new InvalidOperationException($"User with Email: {email} not found.");
            return user.IsEmailVerified;
        }
        
        public async Task<EmailOTPResponseDTO> SendOTPAsync(string email)
        {
            try
            {
                // Check if user exists
                var user = await _context.UserProfilies
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                    return new EmailOTPResponseDTO
                    {
                        Message = $"User with Email: {email} not found.",
                        Success = false,
                        Email = email
                    };

                if (user.IsEmailVerified)
                    return new EmailOTPResponseDTO
                    {
                        Message = "Email is already verified",
                        Success = true,
                        Email = email
                    };

                // Check for existing non-expired, non-used OTPs
                var currentTime = DateTime.UtcNow;
                var existingOTPs = await _context.OTPs
                    .Where(o => o.Email == email && 
                               !o.IsUsed && 
                               o.ExpiresAt > currentTime)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                // If user has 2 or more non-expired, non-used OTPs, restrict further requests
                if (existingOTPs.Count >= 2)
                {
                    return new EmailOTPResponseDTO
                    {
                        Message = "You already have 2 active OTPs. Please use one of the existing OTPs or wait for them to expire before requesting a new one.",
                        Success = false,
                        Email = email,
                        ErrorType = "TOO_MANY_ACTIVE_OTPS"
                    };
                }

                // Generate 6-digit OTP
                var otp = GenerateOTP();
                var expiry = currentTime.AddMinutes(5); // OTP expires in 5 minutes

                // Create new OTP entity
                var otpEntity = new OTPEntity
                {
                    Email = email,
                    OTP = otp,
                    CreatedAt = currentTime,
                    ExpiresAt = expiry,
                    IsUsed = false,
                    Purpose = "EmailVerification"
                };

                // Save to database
                _context.OTPs.Add(otpEntity);
                await _context.SaveChangesAsync();

                try
                {
                    // Send email
                    await SendEmailAsync(email, "Email Verification OTP", 
                        $"Your verification OTP is: {otp}\n\nThis OTP will expire in 5 minutes.");

                    return new EmailOTPResponseDTO
                    {
                        Message = "OTP sent successfully",
                        Success = true,
                        Email = email
                    };
                }
                catch (Exception emailEx)
                {
                    // If email sending fails, remove the OTP from database
                    _context.OTPs.Remove(otpEntity);
                    await _context.SaveChangesAsync();
                    
                    return new EmailOTPResponseDTO
                    {
                        Message = $"Failed to send email: {emailEx.Message}",
                        Success = false,
                        Email = email
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error in SendOTPAsync for {email}: {ex.Message}");
                return new EmailOTPResponseDTO
                {
                    Message = $"Failed to process OTP request: {ex.Message}",
                    Success = false,
                    Email = email
                };
            }
        }
        
        public async Task<object> VerifyEmailOTPAsync(string email, string otp)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(email))
                {
                    return new
                    {
                        Success = false,
                        Message = "Email address is required.",
                        ErrorType = "INVALID_EMAIL"
                    };
                }

                if (string.IsNullOrWhiteSpace(otp))
                {
                    return new
                    {
                        Success = false,
                        Message = "OTP is required.",
                        ErrorType = "INVALID_OTP"
                    };
                }

                // Check if user exists
                var user = await _context.UserProfilies
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return new
                    {
                        Success = false,
                        Message = $"User with email '{email}' not found.",
                        ErrorType = "USER_NOT_FOUND"
                    };
                }

                // Check if email is already verified
                if (user.IsEmailVerified)
                {
                    return new
                    {
                        Success = true,
                        Message = "Email is already verified.",
                        VerifiedAt = user.LastUpdated
                    };
                }

                // Find the OTP in database
                var currentTime = DateTime.UtcNow;
                var otpEntity = await _context.OTPs
                    .Where(o => o.Email == email && 
                               o.OTP == otp && 
                               !o.IsUsed && 
                               o.ExpiresAt > currentTime)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otpEntity == null)
                {
                    // Check if OTP exists but is expired
                    var expiredOTP = await _context.OTPs
                        .Where(o => o.Email == email && o.OTP == otp)
                        .FirstOrDefaultAsync();

                    if (expiredOTP != null && expiredOTP.ExpiresAt <= currentTime)
                    {
                        return new
                        {
                            Success = false,
                            Message = $"OTP has expired at {expiredOTP.ExpiresAt:HH:mm:ss}. Please request a new OTP.",
                            ErrorType = "OTP_EXPIRED",
                            ExpiryTime = expiredOTP.ExpiresAt,
                            CurrentTime = currentTime
                        };
                    }

                    return new
                    {
                        Success = false,
                        Message = "Invalid OTP. Please check the code and try again.",
                        ErrorType = "OTP_MISMATCH"
                    };
                }

                // Mark OTP as used
                otpEntity.IsUsed = true;
                otpEntity.UsedAt = currentTime;

                // Update user verification status
                user.IsEmailVerified = true;
                user.LastUpdated = currentTime;
                
                await _context.SaveChangesAsync();

                return new
                {
                    Success = true,
                    Message = "Email verified successfully!",
                    VerifiedAt = user.LastUpdated,
                    Email = email
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Message = $"An error occurred during verification: {ex.Message}",
                    ErrorType = "SYSTEM_ERROR"
                };
            }
        }
        
        public async Task<bool> ResendEmailOTPAsync(string email)
        {
            try
            {
                var result = await SendOTPAsync(email);
                return result.Success;
            }
            catch
            {
                return false;
            }
        }
        
        private static readonly Random _random = new Random(); 
        private string GenerateOTP()
        {
            return _random.Next(100000, 999999).ToString();
        }
        
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Get settings from configuration or environment variables
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var smtpServer = smtpSettings["Server"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
            
            // Try to get from environment variables first, then configuration
            var smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") 
                ?? smtpSettings["Username"] 
                ?? throw new InvalidOperationException("SMTP Username not configured");
            
            var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") 
                ?? smtpSettings["Password"] 
                ?? throw new InvalidOperationException("SMTP Password not configured");
            
            var fromEmail = Environment.GetEnvironmentVariable("SMTP_FROMEMAIL") 
                ?? smtpSettings["FromEmail"] 
                ?? smtpUsername;

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
        
        public async Task<object> DebugOTPAsync(EmailOTPRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    throw new ArgumentException("Email is required.");

                var normalizedEmail = request.Email.Trim().ToLowerInvariant();
                var currentTime = DateTime.UtcNow;

                // Get all OTPs for this email from database
                var otps = await _context.OTPs
                    .Where(o => o.Email == normalizedEmail)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var activeOTPs = otps.Where(o => !o.IsUsed && o.ExpiresAt > currentTime).ToList();
                var expiredOTPs = otps.Where(o => o.ExpiresAt <= currentTime).ToList();
                var usedOTPs = otps.Where(o => o.IsUsed).ToList();

                return new
                {
                    message = "OTP database query results",
                    email = normalizedEmail,
                    totalOTPs = otps.Count,
                    activeOTPs = activeOTPs.Count,
                    expiredOTPs = expiredOTPs.Count,
                    usedOTPs = usedOTPs.Count,
                    currentTime = currentTime,
                    activeOTPDetails = activeOTPs.Select(o => new
                    {
                        id = o.Id,
                        otp = o.OTP,
                        createdAt = o.CreatedAt,
                        expiresAt = o.ExpiresAt,
                        timeRemaining = o.ExpiresAt - currentTime
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DebugOTPAsync: {ex.Message}");
                
                return new { message = ex.Message };
            }
        }
    }
}