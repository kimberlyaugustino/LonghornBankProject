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
    public class TransactionsController : BaseController
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
            ViewBag.Balance = GetBalance(product).ToString("C2");
            ViewBag.AccountType = product.AccountType;
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
            TransactionDetail transactiondetail = new TransactionDetail();
            transactiondetail.transaction = transaction;
            if (transaction.Dispute != null)
            {
                transactiondetail.dispute = transaction.Dispute;
                if (transaction.Dispute.Status == "Submitted")
                {
                    ViewBag.message = "Current status of dispute: " + transaction.Dispute.Status + " - " + transaction.Dispute.Comments;
                }
                else
                {
                    ViewBag.message = "Current status of dispute: " + transaction.Dispute.Status + " - " + transaction.Dispute.Comments + " (Resolved by: " +transaction.Dispute.Manager.Email+")";
                }
            }

            var query = from d in db.Transactions
                        select d;
            query = query.Where(d => d.TransactionID != transaction.TransactionID);
            query = query.Where(d => d.Account.ProductID == transaction.Account.ProductID);
            query = query.Where(d => d.TransactionType == transaction.TransactionType);
            IEnumerable<Transaction> transactions = query.ToList();
            transactiondetail.transactions = transactions;

            ViewBag.String = id;
            return View(transactiondetail);
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
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.AllAccounts = GetAllAccounts(product);
            ViewBag.String = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deposit([Bind(Include ="TransactionID,Date,TransactionType,Description,Amount,Comments")] Transaction transaction, Int32 ProductID)
        {
            if (ModelState.IsValid)
            {
                Product product = db.Accounts.Find(ProductID);
                ViewBag.AllAccounts = GetAllAccounts(product);
                if (transaction.Amount <= 0)
                {
                    ViewBag.Error = "Amount must be greater than $0.00.";
                    return View(transaction);
                }

                if (product.AccountType == "IRA")
                {
                    var age = GetAge(product);

                    if (age >= 70)
                    {
                        return RedirectToAction("Error", new string[] {"You must be younger than 70 to contribute to your IRA account."});
                    }

                    decimal contribution = 0;
                    foreach (var item in product.Transactions)
                    {
                        if (item.TransactionType == "Deposit")
                        {
                            contribution += item.Amount;
                        }
                    }

                    if (contribution + transaction.Amount > 5000)
                    {
                        decimal modifiedDeposit = 5000 - contribution;
                        transaction.Amount = modifiedDeposit;
                        ViewBag.Message = "You are only allowed to contribute " + modifiedDeposit.ToString() + " before reaching your $5,000.00 contribution limit.";
                        return View(transaction);
                    }
                }
                transaction.Account = product;
                transaction.TransactionType = "Deposit";
                db.Transactions.Add(transaction);
                product.Balance += transaction.Amount;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = transaction.Account.ProductID });
            }
            return View(transaction);
        }

        [HttpGet]
        public ActionResult Withdrawal(int? id)
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
            ViewBag.String = id;
            return View();
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
                if (transaction.Amount > 0)
                {
                    transaction.Amount = -transaction.Amount;
                }
                Product SelectedProduct = db.Accounts.Find(ProductID);
                transaction.Account = SelectedProduct;
                transaction.TransactionType = "Withdrawal";
                SelectedProduct.Balance += transaction.Amount;
                if (SelectedProduct.Balance < 0)
                {
                    if (SelectedProduct.Balance < -50)
                    {
                        ViewBag.Message = "Transaction exceeds $50 overdraft limit";
                        SelectedProduct.Balance -= transaction.Amount;
                        ModelState.Remove("Amount");
                        transaction.Amount = 50 + SelectedProduct.Balance;
                        return View(transaction);
                    }
                    Transaction fee = new Transaction();
                    fee.Amount = -30;
                    fee.Description = "Overdraft fee";
                    fee.Date = transaction.Date;
                    fee.Account = SelectedProduct;
                    fee.TransactionType = "Fee";
                    db.Transactions.Add(fee);
                    SelectedProduct.Balance += fee.Amount;
                }
                db.Transactions.Add(transaction);
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
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.TransferAccounts = GetAllAccounts();
            ViewBag.AllAccounts = GetAllAccounts(product);
            ViewBag.String = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer([Bind(Include = "TransactionID,Date,TransactionType,Description,Amount,Comments")] Transaction transaction, Int32 ProductID, Int32 TransferID, bool Include)
        {
            Product SelectedProduct = db.Accounts.Find(ProductID);
            ViewBag.AllAccounts = GetAllAccounts(SelectedProduct);
            ViewBag.TransferAccounts = GetAllAccounts();

            if (ModelState.IsValid)
            {
                if (transaction.Amount == 0)
                {
                    ViewBag.Error = "Amount cannot be $0.00.";
                    return View(transaction);
                }
                transaction.Amount = -transaction.Amount;
                SelectedProduct.Balance += transaction.Amount;
                if (SelectedProduct.Balance < 0)
                {
                    if (SelectedProduct.Balance < -50)
                    {
                        ViewBag.Message = "Transaction exceeds $50 overdraft limit";
                        SelectedProduct.Balance -= transaction.Amount;
                        ModelState.Remove("Amount");
                        transaction.Amount = 50 + SelectedProduct.Balance;
                        return View(transaction);
                    }
                    Transaction fee = new Transaction();
                    fee.Amount = -30;
                    fee.Description = "Overdraft fee";
                    fee.Date = transaction.Date;
                    fee.Account = SelectedProduct;
                    fee.TransactionType = "Fee";
                    db.Transactions.Add(fee);
                    SelectedProduct.Balance += fee.Amount;
                }

                if (SelectedProduct.AccountType == "IRA")
                {
                    var age = GetAge(SelectedProduct);
                    if (age < 65)
                    {
                        if (transaction.Amount > 3000)
                        {
                            return View("Error", new string[] { "You can only withdrawal a max of $3,000" });
                        }
                        Transaction fee = new Transaction();
                        fee.Amount = -30;
                        fee.Description = "Service Fee";
                        fee.TransactionType = "Fee";
                        fee.Date = transaction.Date;
                        fee.Account = SelectedProduct;
                        db.Transactions.Add(fee);
                        
                        if (Include)
                        {
                            transaction.Amount = transaction.Amount - 30;
                        }
                    }
                }


                Product TransferProduct = db.Accounts.Find(TransferID);
                Transaction Transfertransaction = new Transaction();
                Transfertransaction.Account = TransferProduct;
                Transfertransaction.TransactionType = "Transfer";
                Transfertransaction.Amount = -transaction.Amount;
                Transfertransaction.Description = "Transfer from " + SelectedProduct.AccountName;
                Transfertransaction.Date = transaction.Date;
                db.Transactions.Add(Transfertransaction);

                transaction.Account = SelectedProduct;
                transaction.TransactionType = "Transfer";
                transaction.Description = "Transfer to " + TransferProduct.AccountName;
                db.Transactions.Add(transaction);

                TransferProduct.Balance += Transfertransaction.Amount;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = transaction.Account.ProductID });
            }
            return View(transaction);
        }
    }
}