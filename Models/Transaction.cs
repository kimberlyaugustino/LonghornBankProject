using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LonghornBankProject.Models
{
    public class Transaction
    {
        [Display(Name ="Transaction Number")]
        public Int32 TransactionID { get; set; }

        [Display(Name ="Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        
        [Display(Name = "Transaction Type")]
        public string TransactionType { get; set; }

        public string Description { get; set; }

        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public string Comments { get; set; }

        public virtual Product Account { get; set; }
    }
}