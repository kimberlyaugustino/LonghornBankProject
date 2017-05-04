using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using LonghornBankProject.Models;
using System.Net;
using System.Data.Entity;

namespace LonghornBankProject.Controllers
{
    public class PayBillsController : BaseController
    {
        private AppDbContext db = new AppDbContext();


        [HttpGet]
        public ActionResult PayBills()
        {
            string id = User.Identity.GetUserId();
            AppUser user = db.Users.Find(id);

            var model = new List<PayeeTransaction>();
            foreach (var payee in user.Payees)
            {
                PayeeTransaction item = new PayeeTransaction();
                item.Name = payee.Name;
                item.Type = payee.Type;
                model.Add(item);
            }

            ViewBag.List = model;
            ViewBag.AllAccounts = GetAllAccounts();
            return View(model.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayBills(PayeeTransaction model, Int32 ProductID)
        {
            if (ModelState.IsValid)
            {
                if (model.Amount <= 0)
                {
                    ViewBag.Error = "Amount cannot be $0.00 or less.";
                    return RedirectToAction("PayBills");
                }
                Product SelectedProduct = db.Accounts.Find(ProductID);
                Transaction transaction = new Transaction();
                transaction.Amount = -model.Amount;
                transaction.Date = model.Date;
                transaction.Account = SelectedProduct;
                transaction.Description = "Payment to " + model.Name + " for " + model.Type + " bill";
                transaction.TransactionType = "Bill Payment";

                SelectedProduct.Balance += transaction.Amount;
                if (SelectedProduct.Balance < 0)
                {
                    if (SelectedProduct.Balance < -50)
                    {
                        ViewBag.Message = "Transaction exceeds $50 overdraft limit";
                        return RedirectToAction("Withdrawal", "Transactions", new { transaction = transaction, ProductID = ProductID });
                    }
                    Transaction fee = new Transaction();
                    fee.Amount = -30;
                    fee.Description = "Overdraft fee";
                    fee.Date = model.Date;
                    fee.Account = SelectedProduct;
                    fee.TransactionType = "Overdraft Fee";
                    db.Transactions.Add(fee);
                    SelectedProduct.Balance += fee.Amount;
                }
                db.Transactions.Add(transaction);
                db.Entry(SelectedProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Transactions", new { id = ProductID });
            }
            return RedirectToAction("PayBills");

        }

        [HttpGet]
        public ActionResult CreatePayee()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePayee([Bind(Include = "PayeeID,Name,Street,City,State,ZipCode,PhoneNumber,Type,Payers,transactions")]Payee payee)
        {
            if (ModelState.IsValid)
            {
                string id = User.Identity.GetUserId();
                AppUser user = db.Users.Find(id);
                user.Payees.Add(payee);
                db.Payees.Add(payee);
                db.SaveChanges();
                return RedirectToAction("PayBills");
            }
            return View(payee);
        }

        [HttpGet]
        public ActionResult AddPayee()
        {
            ViewBag.AllPayees = GetAllPayees();
            return View();
        }

        [HttpPost]
        public ActionResult AddPayee(int[] SelectedPayees)
        {
            string id = User.Identity.GetUserId();
            AppUser user = db.Users.Find(id);
            if (ModelState.IsValid)
            {
                if (SelectedPayees != null)
                {
                    foreach (int PayeeID in SelectedPayees)
                    {
                        Payee payeeToAdd = db.Payees.Find(PayeeID);
                        user.Payees.Add(payeeToAdd);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("PayBills");
            }
            ViewBag.AllPayees = GetAllPayees();
            return View();
        }

        public ActionResult PayeeDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            return View(payee);
        }

        [HttpGet]
        public ActionResult PayeeEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            return View(payee);
        }

        [HttpPost]
        public ActionResult PayeeEdit([Bind(Include = "PayeeID, Name, Street, City, State, ZipCode, PhoneNumber, Type, Payers, transactions")] Payee payee)
        {
            if (ModelState.IsValid)
            {
                Payee payeeToChange = db.Payees.Find(payee.PayeeID);
                payeeToChange.Name = payee.Name;
                payeeToChange.Street = payee.Street;
                payeeToChange.City = payee.City;
                payeeToChange.State = payee.State;
                payeeToChange.ZipCode = payee.ZipCode;
                db.Entry(payeeToChange).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("PayBills");
            }
            return View(payee);
        }
    }
}