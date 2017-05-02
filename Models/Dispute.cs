using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LonghornBankProject.Controllers;

namespace LonghornBankProject.Models
{
    public class Dispute
    {
        [ForeignKey("Transaction")]
        public Int32 DisputeID { get; set; }

        [Required(ErrorMessage ="Must provide reason for dispute.")]
        public string Comments { get; set; }

        [Required(ErrorMessage ="Please enter new amount.")]
        [Display(Name ="Disputed Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        public bool Delete { get; set; }

        public string Status { get; set; }

        public virtual AppUser Customer { get; set; }
        public virtual Transaction Transaction { get; set; }
        public virtual AppUser Manager { get; set; }
    }

    public class ResolveDispute
    {
        public Dispute dispute { get; set; }
        public Transaction transaction { get; set;}
    }
}