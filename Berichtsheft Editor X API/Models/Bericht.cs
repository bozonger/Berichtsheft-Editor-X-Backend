using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Berichtsheft_Editor_X.Models
{
    public class Bericht
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public string fileName { get; set; }

        public User User;
    }
}