using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Core8MVC.Models.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Title")]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author {  get; set; }

        [Required]
        [DisplayName("List Price")]
        [Range(1, 1000, ErrorMessage = "List Price should be within 1 - 1000")]
        public double ListPrice { get; set; }

        [Required]
        [DisplayName("List Price for 1-50")]
        [Range(1, 1000, ErrorMessage = "Price should be within 1 - 1000")]
        public double Price { get; set; }

        [Required]
        [DisplayName("List Price for 50-100")]
        [Range(1, 1000, ErrorMessage = "Price should be within 1 - 1000")]
        public double Price50 { get; set; }

        [Required]
        [DisplayName("List Price for 100+")]
        [Range(1, 1000, ErrorMessage = "Price should be within 1 - 1000")]
        public double Price100 { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        [ValidateNever]
        public Category Category { get; set; }
        [ValidateNever]
        public string? ImageURL { get; set; }
    }
}
