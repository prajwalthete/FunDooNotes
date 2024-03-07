using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Models
{
    public class UserLoginModel
    {
        [EmailAddress]
        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}
