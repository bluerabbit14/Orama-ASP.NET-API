using Orama_API.Domain;
using Orama_API.DTO;

namespace Orama_API.Interfaces
{
    public interface IAdminService
    {
        Task<SignUpResponseDTO> RegisterAsync(SignUpRequestDTO sigUpRequestDto);
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO logInRequestDto);
        Task<IEnumerable<UserProfile>> GetAllUserAsync();
        Task<IEnumerable<UserProfile>> GetAllAdminAsync();
        Task<UserProfile?> GetUserByIdAsync(int id);
        Task<UserProfile?> GetUserByEmailAsync(string email);
        Task<UserProfile?> GetUserByPhoneAsync(string phone);
        Task<UserStatusResponseDTO?> AlterUserStatusAsync(int id);
        Task<UserProfile> UpdateUserProfileAsync(int id,ProfileUpdateUserDTO profileUpdateUser);
        Task<object> DeleteUserAsync(int id);

    }
}
