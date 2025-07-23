namespace Orama_API.DTO
{
    public class DeleteUserResponseDTO
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime DeletedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
} 