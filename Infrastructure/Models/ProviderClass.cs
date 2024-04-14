using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Models
{
    [PrimaryKey(nameof(ProviderId), nameof(ClassId))]
    public class ProviderClass
    {
        public string? ProviderId { get; set; }

        public int? ClassId { get; set; }

        [ForeignKey(nameof(ClassId))]
        [Display(Name = "Class")]
        public Class? Class { get; set; }

        [ForeignKey(nameof(ProviderId))]
        public Provider? Provider { get; set; }
    }
}
