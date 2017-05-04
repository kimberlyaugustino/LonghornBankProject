using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

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

        [Display(Name ="Transaction Description")]
        public string Description { get; set; }

        [Display(Name ="Transaction Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Display(Name ="Manager Comments")]
        public string Comments { get; set; }

        public virtual Product Account { get; set; }
        public virtual Dispute Dispute { get; set; }
    }

    public class TransactionDetail
    {
        public Transaction transaction { get; set; }
        public Dispute dispute { get; set; }
        public IEnumerable<Transaction> transactions { get; set; }
    }

    public class PayeeTransaction
    {
        public string Name { get; set; }
        public string Type { get; set; }

        [Display(Name="Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name="Transaction Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }
    }
}