using Microsoft.EntityFrameworkCore;
using Orama_API.Data;
using Orama_API.Domain;
using Orama_API.DTO;
using Orama_API.Interfaces;

namespace Orama_API.Services
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _context; //here we are using the UserDbContext to interact with the database
        private readonly IJwtService _jwtService;
        public UserService(UserDbContext context,IJwtService jwtService)
        {
            _context = context; //we are injecting the UserDbContext into the UserService
            _jwtService = jwtService; //we are injecting the IJwtService into the UserService
        }
        public async Task<SignUpResponseDTO> RegisterAsync(SignUpRequestDTO signUpRequestDto)
        {
            if (string.IsNullOrWhiteSpace(signUpRequestDto.Name))
                throw new ArgumentException("Name is required.");

            if (string.IsNullOrWhiteSpace(signUpRequestDto.Email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(signUpRequestDto.Password))
                throw new ArgumentException("Password is required.");


            var existingUser = await _context.UserProfilies.FirstOrDefaultAsync(u =>
                (!string.IsNullOrEmpty(signUpRequestDto.Email) && u.Email == signUpRequestDto.Email));

            if (existingUser != null)
            {
                if (!string.IsNullOrEmpty(signUpRequestDto.Email) && existingUser.Email == signUpRequestDto.Email)
                {
                    var conflictMessage = "Email is already registered.";
                    throw new InvalidOperationException(conflictMessage);
                }
            }
            var newUser = new UserProfile
            {
                Name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(signUpRequestDto.Name.ToLower()),
                Email = signUpRequestDto.Email,
                Password = signUpRequestDto.Password,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.UserProfilies.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var result = new SignUpResponseDTO
            {
                Message = "User Registered Successfully",
                UserId = newUser.UserId,
                Role=newUser.Role,
                Email = newUser.Email ?? string.Empty,
                CreatedAt = newUser.CreatedAt,
            };
            return result;
        }
        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO logInRequestDto)
        {
            if (string.IsNullOrWhiteSpace(logInRequestDto.Email))
                throw new ArgumentException("Email is required field.");

            if (string.IsNullOrWhiteSpace(logInRequestDto.Password))
                throw new ArgumentException("Password is required field.");

            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.Email == logInRequestDto.Email);

            if (user == null)
                throw new InvalidOperationException("User does not exist");

            if (user.IsActive == false)
                throw new InvalidOperationException("User is not active");

            if (user.Password != logInRequestDto.Password) // Corrected line
                throw new InvalidOperationException("Invalid Password");

            if (!(user.Role == "User"))
                throw new InvalidOperationException("Only User login are authorize");

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
            var tokenExpiry = _jwtService.GetTokenExpiration(token);

            var result = new LoginResponseDTO()
            {
                Message = "User Logged in successfully",
                UserId = user.UserId,
                Email = user.Email,
                Logintime = DateTime.UtcNow,
                Token = token,
                TokenValidity = (DateTime)tokenExpiry
            };
            return result;
        }
        public async Task<ChangePasswordResponseDTO> PasswordAsync(ChangePasswordRequestDTO changePasswordRequestDto)
        {
            if (string.IsNullOrWhiteSpace(changePasswordRequestDto.Email))
                throw new ArgumentException("Email is required field.");

            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.Email == changePasswordRequestDto.Email);

            if (user == null)
                throw new InvalidOperationException("Email is not Registered");

            if (user.IsActive == false)
                throw new InvalidOperationException("User is not active");

            user.Password = changePasswordRequestDto.NewPassword;
            user.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var result = new ChangePasswordResponseDTO()
            {
                Email = changePasswordRequestDto.Email,
                Message = "Password changed successfully"
            };
            return result;
        }
        public async Task<ProfileUpdateUserDTO> UpdateProfileAsync( string Email,ProfileUpdateUserDTO profileUpdateUser)
        {
            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.Email ==Email);

            if (user == null)
                throw new InvalidOperationException("Email is not Registered");

            if (user.IsActive == false)
                throw new InvalidOperationException("User is not active");

            // Update only the fields that are provided (partial update)
            if (!string.IsNullOrWhiteSpace(profileUpdateUser.ImageUrl))
                user.ImageUrl = profileUpdateUser.ImageUrl;

            if (!string.IsNullOrWhiteSpace(profileUpdateUser.Name))
                user.Name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(profileUpdateUser.Name.ToLower());

            if (!string.IsNullOrWhiteSpace(profileUpdateUser.Address))
                user.Address = profileUpdateUser.Address;

            if (!string.IsNullOrWhiteSpace(profileUpdateUser.Pincode))
                user.Pincode = profileUpdateUser.Pincode;

            if (!string.IsNullOrWhiteSpace(profileUpdateUser.DateOfBirth))
                user.DateOfBirth = profileUpdateUser.DateOfBirth;

            if (!string.IsNullOrWhiteSpace(profileUpdateUser.Gender))
                user.Gender = profileUpdateUser.Gender;

            if (!string.IsNullOrWhiteSpace(profileUpdateUser.LanguagePreference))
                user.LanguagePreference = profileUpdateUser.LanguagePreference;

            if (!string.IsNullOrWhiteSpace(profileUpdateUser.Bio))
                user.Bio = profileUpdateUser.Bio;

            if (!string.IsNullOrWhiteSpace(profileUpdateUser.SocialHandle))
                user.SocialHandle = profileUpdateUser.SocialHandle;

            user.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Return the updated profile data
            return new ProfileUpdateUserDTO
            {
                ImageUrl = user.ImageUrl,
                Name = user.Name,
                Address = user.Address,
                Pincode = user.Pincode,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                LanguagePreference = user.LanguagePreference,
                Bio = user.Bio,
                SocialHandle = user.SocialHandle
            };
        }
        public async Task<UserProfile> GetMyProfileByEmailAsync(string Email)
        {
            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.IsActive == false)
                throw new InvalidOperationException("User is not active");

            return user;
        }
        public async Task<object> DeleteMyProfileAsync(int id)
        {
            var user= await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.IsActive == false)
                throw new InvalidOperationException("User is not active");

            var deletedUser = new
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                CreatedAt=user.CreatedAt,
                DeletedAt = DateTime.UtcNow,
                Message = $"User '{user.Name}' with email '{user.Email}' has been successfully deleted."
            };

            _context.UserProfilies.Remove(user);
            await _context.SaveChangesAsync();

            return deletedUser;
        }
        
    }
}
