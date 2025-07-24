using Microsoft.EntityFrameworkCore;
using Orama_API.Data;
using Orama_API.Domain;
using Orama_API.DTO;
using Orama_API.Interfaces;

namespace Orama_API.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserDbContext _context;
        private readonly IJwtService _jwtService;
        public AdminService(UserDbContext context, IJwtService jwtService)
        {
            _context = context; //we are injecting the UserDbContext into the UserService
            _jwtService = jwtService;
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
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.UserProfilies.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var result = new SignUpResponseDTO
            {
                Message = "User Registered Successfully",
                UserId = newUser.UserId,
                Role = newUser.Role,
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

            if(!(user.Role =="Admin"))
                throw new InvalidOperationException("Only Admin login are authorize");

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
        public async Task<IEnumerable<UserProfile>> GetAllUserAsync()
        {
            return await _context.UserProfilies
                .Where(u => u.Role == "User")
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<UserProfile>> GetAllAdminAsync()
        {
            return await _context.UserProfilies
                .Where(u => u.Role == "Admin")
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<UserProfile?> GetUserByIdAsync(int id)
        {
            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.UserId == id);
                
            if (user == null)
                throw new InvalidOperationException($"User with ID {id} not found.");
                
            return user;
        }
        
        public async Task<UserProfile?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.");
                
            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.Email == email);
                
            if (user == null)
                throw new InvalidOperationException($"User with email '{email}' not found.");
                
            return user;
        }
        
        public async Task<UserProfile?> GetUserByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone number cannot be null or empty.");
                
            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.Phone == phone);
                
            if (user == null)
                throw new InvalidOperationException($"User with phone number '{phone}' not found.");
                
            return user;
        } 
        public async Task<UserStatusResponseDTO?> AlterUserStatusAsync(int id)
        {
            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.UserId == id);
                
            if (user == null)
                throw new InvalidOperationException($"User with ID {id} not found.");
                
            // Toggle the IsActive status
            user.IsActive = !user.IsActive;
            user.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return new UserStatusResponseDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                IsActive = user.IsActive,
                LastUpdated = user.LastUpdated,
                Message = $"User status updated successfully. User is now {(user.IsActive ? "Active" : "Inactive")}."
            };
        }
        public async Task<UserProfile> UpdateUserProfileAsync(int id,ProfileUpdateUserDTO profileUpdateUser)
        {
            var user = await _context.UserProfilies
               .FirstOrDefaultAsync(u => u.UserId == id);

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
            return user;
        }
        public async Task<object> DeleteUserAsync(int id)
        {
            var user = await _context.UserProfilies
                .FirstOrDefaultAsync(u => u.UserId == id);
                
            if (user == null)
                throw new InvalidOperationException($"User with ID {id} not found.");
                
            // Store user information before deletion for response
            var deletedUser = new
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                DeletedAt = DateTime.UtcNow,
                Message = $"User '{user.Name}' with email '{user.Email}' has been successfully deleted."
            };
            
            // Remove the user from the database
            _context.UserProfilies.Remove(user);
            await _context.SaveChangesAsync();
            
            return deletedUser;
        }
    }
}
