using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace LonghornBankProject.Models
{
    public class Payee
    {
        [Required]
        public Int32 PayeeID { get; set; }

        [Required(ErrorMessage="Payee name is requred.")]
        public string Name { get; set; }

        [Required(ErrorMessage ="Street is required.")]
        public string Street { get; set; }

        [Required(ErrorMessage ="City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage ="State is required.")]
        public string State { get; set; }

        [Required(ErrorMessage ="Zipcode is required.")]
        public string ZipCode { get; set; }

        [Required(ErrorMessage ="Phone number is required.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage ="Payee type is required.")]
        public string Type { get; set; }

        public virtual List<AppUser> Payers { get; set; }
    }
}