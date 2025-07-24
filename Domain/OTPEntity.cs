using System.ComponentModel.DataAnnotations;

namespace Orama_API.Domain
{
    public class OTPEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string OTP { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime? UsedAt { get; set; }
        
        [StringLength(50)]
        public string? Purpose { get; set; } = "EmailVerification";
    }
} 