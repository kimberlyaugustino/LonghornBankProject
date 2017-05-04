using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LonghornBankProject.Controllers;

namespace LonghornBankProject.Models
{
    public class Product
    {
        public Int32 ProductID { get; set; }

        [Display(Name ="Account Name")]
        public String AccountName { get; set; }

        [Display(Name ="Account Type")]
        public string AccountType { get; set; }

        [Required(ErrorMessage ="Must create account with an initial balance.")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        public bool Activated { get; set; }

        public virtual AppUser Customer { get; set; }

        public virtual StockPortfolio Portfolio { get; set; }

        public virtual List<Transaction> Transactions { get; set; }

        public Product()
        {
        }

        public Product(AccountType SelectedProduct, AppUser customer, decimal balance)
        {
            AccountType = SelectedProduct.ToString();
            AccountName = "Longhorn " + SelectedProduct.ToString();
            Balance = balance;
            Customer = customer;
        }
    }
}