using Orama_API.Domain;
using Orama_API.DTO;
using System.Threading.Tasks;

namespace Orama_API.Interfaces
{
    public interface IUserService
    {
        Task<SignUpResponseDTO> RegisterAsync(SignUpRequestDTO sigUpRequestDto);
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO logInRequestDto);
        Task<ChangePasswordResponseDTO> PasswordAsync(ChangePasswordRequestDTO changePasswordRequestDto);
        Task<ProfileUpdateUserDTO> UpdateProfileAsync(string Email,ProfileUpdateUserDTO profileUpdateUser);
        Task<UserProfile> GetMyProfileByEmailAsync(string Email);
        Task<Object> DeleteMyProfileAsync(int id);
    }
}
