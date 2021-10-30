namespace P3D.Legacy.Server.UI.Shared.Models
{
    public class LoginResult
    {
        public bool Successful { get; set; } = default!;
        public string Error { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}