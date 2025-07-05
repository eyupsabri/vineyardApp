namespace Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<UserDevice> UserDevices { get; set; }
        public string? CurrentJwtId { get; set; }
        public string? RefreshJwtId { get; set; }
    }
}
