namespace Berichtsheft_Editor_X.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Roles { get; set; }

        public List<Bericht> Berichte { get; set; } = new();
    }
}
