using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Core8MVCWebAppRazor.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Name")]
        [MaxLength(100)]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 1000, ErrorMessage = "Display Order should be within 1 - 1000")]
        public int DisplayOrder { get; set; }
    }
}
