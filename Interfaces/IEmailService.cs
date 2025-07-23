
using Orama_API.DTO;

namespace Orama_API.Interfaces
{
    public interface IEmailService
    {
        Task<bool> IsEmailValidAsync(string email);
        Task<bool> IsEmailRegisteredAsync(string Email);
        Task<bool> IsEmailVerifiedAsync(string email);
        Task<EmailOTPResponseDTO> SendOTPAsync(string email);
        Task<object> VerifyEmailOTPAsync(string email, string otp);
        Task<bool> ResendEmailOTPAsync(string email);
        Task<object> DebugOTPAsync(EmailOTPRequestDTO request);
    }
}