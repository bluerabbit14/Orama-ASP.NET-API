using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Orama_API.DTO
{
    public class LoginResponseDTO
    {
        [Required]
        public string Message { get; set; }
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public DateTime? Logintime { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenValidity { get; set; }
    }
}
