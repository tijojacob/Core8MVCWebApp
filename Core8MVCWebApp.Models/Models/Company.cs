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
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Name")]
        [MaxLength(100)]
        public string Name { get; set; }
        [DisplayName("Street Address")]
        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State {  get; set; }
        [DisplayName("Postal Code")]
        public string PostalCode {  get; set; }

        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

    }
}
