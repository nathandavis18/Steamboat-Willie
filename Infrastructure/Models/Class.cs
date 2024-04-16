using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool IsDisabled { get; set; }
    }
}
