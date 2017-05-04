using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using LonghornBankProject.Models;
using System.Net;
using LonghornBankProject.Utilities;
using System.Data.Entity;

namespace LonghornBankProject.Controllers
{
    public class BaseController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public SelectList GetAllAccounts()
        {
            string id = User.Identity.GetUserId();
            var query = from a in db.Accounts
                        select a;
            query = query.Where(a => a.Customer.Id == id);
            query = query.Where(x => x.AccountType != "Stock");
            List<Product> allAccounts = query.ToList();
            SelectList list = new SelectList(allAccounts, "ProductID", "AccountName");
            return list;
        }

        public SelectList GetAllAccounts(Product product)
        {
            var query = from a in db.Accounts
                        select a;
            query = query.Where(a => a.Customer.Id == product.Customer.Id);
            query = query.Where(a => a.AccountType != "Stock");
            List<Product> allAccounts = query.ToList();
            SelectList list = new SelectList(allAccounts, "ProductID", "AccountName", product.ProductID);
            return list;
        }

        public SelectList GetAllPayees()
        {
            string id = User.Identity.GetUserId();
            AppUser user = db.Users.Find(id);

            List<Payee> other = (((db.Payees).ToList()).Except(user.Payees)).ToList();
            SelectList list = new SelectList(other, "PayeeID", "Name");
            return list;
        }

        public static int GetAge(Product product)
        {
            var age = DateTime.UtcNow.Year - product.Customer.DOB.Year;
            if (DateTime.UtcNow < product.Customer.DOB)
            {
                age = age - 1;
            }
            return age;
        }
        
        public decimal GetBalance(Product product)
        {
            decimal Balance = 0;
            foreach (var amount in product.Transactions)
            {
                Balance += amount.Amount;
            }
            return Balance;
        }

    }
}