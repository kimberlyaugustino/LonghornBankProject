using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using LonghornBankProject.Models;
using System.Net;

namespace LonghornBankProject.Controllers
{
    public class TransactionsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Accounts.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            var query = from t in db.Transactions
                        select t;
            query = query.Where(t => t.Account.ProductID == id);
            query = query.OrderBy(t => t.TransactionID);
            List<Transaction> accountTransactions = query.ToList();
            ViewBag.ProductID = product.ProductID;
            ViewBag.AccountName = product.AccountName;
            ViewBag.Balance = product.Balance;
            return View(accountTransactions);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.String = id;
            return View(transaction);
        }

        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Accounts.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // GET: Transaction/Deposit
        public ActionResult Deposit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Accounts.Find(id);
            Transaction transaction = new Transaction();
            product.Transactions.Add(transaction);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.AllAccounts = GetAllAccounts(product);
            ViewBag.String = id;
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deposit([Bind(Include ="TransactionID,Date,TransactionType,Description,Amount,Comments")] Transaction transaction, Int32 ProductID)
        {
            Product product = db.Accounts.Find(ProductID);
            ViewBag.AllAccounts = GetAllAccounts(product);
            if (ModelState.IsValid)
            {
                if (transaction.Amount <= 0)
                {
                    ViewBag.Error = "Amount must be greater than $0.00.";
                    return View(transaction);
                }
                Product SelectedProduct = db.Accounts.Find(ProductID);
                transaction.Account = SelectedProduct;
                transaction.TransactionType = "Deposit";
                db.Transactions.Add(transaction);
                SelectedProduct.Balance = SelectedProduct.Balance + transaction.Amount;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = transaction.Account.ProductID });
            }
            return View(transaction);
        }

        public ActionResult Withdrawal(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Accounts.Find(id);
            Transaction transaction = new Transaction();
            product.Transactions.Add(transaction);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.String = id;
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Withdrawal([Bind(Include = "TransactionID,Date,TransactionType,Description,Amount,Comments")] Transaction transaction, Int32 ProductID)
        {
            if (ModelState.IsValid)
            {
                if (transaction.Amount == 0)
                {
                    ViewBag.Error = "Amount cannot be $0.00.";
                    return View(transaction);
                }
                if (transaction.Amount < 0)
                {
                    transaction.Amount = Math.Abs(transaction.Amount);
                }
                Product SelectedProduct = db.Accounts.Find(ProductID);
                transaction.Account = SelectedProduct;
                transaction.TransactionType = "Withdrawal";
                transaction.Amount = -transaction.Amount;
                db.Transactions.Add(transaction);
                SelectedProduct.Balance = SelectedProduct.Balance + transaction.Amount;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = transaction.Account.ProductID });
            }
            return View(transaction);
        }

        public ActionResult Transfer(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Accounts.Find(id);
            Transaction transaction = new Transaction();
            product.Transactions.Add(transaction);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.TransferAccounts = GetAllAccounts();
            ViewBag.String = id;
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer([Bind(Include = "TransactionID,Date,TransactionType,Description,Amount,Comments")] Transaction transaction, Int32 ProductID, Int32 TransferID)
        {
            if (ModelState.IsValid)
            {
                if (transaction.Amount == 0)
                {
                    ViewBag.Error = "Amount cannot be $0.00.";
                    ViewBag.TransferAccounts = GetAllAccounts();
                    return View(transaction);
                }
                Product SelectedProduct = db.Accounts.Find(ProductID);
                transaction.Account = SelectedProduct;
                transaction.TransactionType = "Transfer";
                transaction.Amount = -transaction.Amount;
                db.Transactions.Add(transaction);

                Product TransferProduct = db.Accounts.Find(TransferID);
                TransferProduct.Balance = TransferProduct.Balance + transaction.Amount;
                SelectedProduct.Balance = SelectedProduct.Balance + transaction.Amount;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = transaction.Account.ProductID });
            }
            ViewBag.TransferAccounts = GetAllAccounts();
            return View(transaction);
        }

        public SelectList GetAllAccounts()
        {
            var query = from a in db.Accounts
                        select a;
            query = query.Where(x => x.AccountType == "Checking" || x.AccountType == "Savings");
            List<Product> allAccounts = query.ToList();
            SelectList list = new SelectList(allAccounts, "ProductID", "AccountName");
            return list;
        }

        public SelectList GetAllAccounts(Product product)
        {
            var query = from a in db.Accounts
                        select a;
            query = query.Where(x => x.Customer.Id == product.Customer.Id);
            query = query.Where(x => x.AccountType == "Checking" || x.AccountType == "Savings");
            List<Product> allAccounts = query.ToList();
            SelectList list = new SelectList(allAccounts, "ProductID", "AccountName", product.ProductID);
            return list;
        }
    }
}