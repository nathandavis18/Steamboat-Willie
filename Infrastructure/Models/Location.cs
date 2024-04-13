using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        public string? LocationValue { get; set; } 

        public string? Campus { get; set; } //Ogden, Davis, Online, SLCC
    }
}
