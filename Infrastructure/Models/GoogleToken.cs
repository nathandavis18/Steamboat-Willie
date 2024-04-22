using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class GoogleToken
    {
        public GoogleToken()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; }

        [Required]
        public string? UserId {  get; set; }

        [Required]
        public string TokenName {  get; set; }

        [Required]
        public string TokenValue { get; set; }


    }
}
