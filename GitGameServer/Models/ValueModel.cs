using System.ComponentModel.DataAnnotations;

namespace GitGameServer.Models
{
    public class ValueModel
    {
        [Display(Name = "X Value")]
        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(".*", ErrorMessage = "How did you do that?")]
        [DataType(DataType.Password, ErrorMessage = "{0} must be a password for a good reason.")]
        public string X { get; set; }

        [Display(Name = "Y Value")]
        [Compare("X", ErrorMessage = "{1} must match {0} for whatever reason.")]
        public string Y { get; set; }
    }
}