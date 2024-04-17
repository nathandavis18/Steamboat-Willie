using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? LocationValue { get; set; }

        [Required]
        public string? Campus { get; set; } //Ogden, Davis, Online, SLCC
    }
}
