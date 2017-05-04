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
    public enum TransactionType { All, Deposit, Withdrawal, Transfer, Fee };
    public enum PriceRange { All, r0_100, r100_200, r200_300, r300plus, custom_price_range }
    public enum DateRange { All, Last15Days, Last30Days, Last45Days, Last60Days, custom_date_range }

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

            ViewBag.ProductID = product.ProductID;
            ViewBag.AccountName = product.AccountName;
            ViewBag.Balance = product.Balance.ToString("C2");
            ViewBag.AccountType = product.AccountType;
            return View(product.Transactions.ToList());
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
                db.Entry(product).State = EntityState.Modified;
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
                db.Entry(SelectedProduct).State = EntityState.Modified;
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
                db.Entry(TransferProduct).State = EntityState.Modified;
                db.Entry(SelectedProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = transaction.Account.ProductID });
            }
            return View(transaction);
        }

        public ActionResult SearchResults(string SearchString, TransactionType SelectedTransType, PriceRange SelectedPriceRange, decimal? min, decimal? max, string SelectedTransNumber, DateRange SelectedDateRange, string custom_max_date)
        {
            var query = from a in db.Transactions
                        select a;

            //trans number

            if (SelectedTransNumber != null && SelectedTransNumber != "")
            {
                int vari;
                vari = Convert.ToInt32(SelectedTransNumber);
                query = query.Where(a => a.TransactionID == vari);
            }


            //description
            if (SearchString != null && SearchString != "")
            { query = query.Where(a => a.Description.Contains(SearchString) || a.Description.Contains(SearchString)); }

            //price range
            if (SelectedPriceRange == PriceRange.All)
            {
                query = query.Where(a => a.Amount != 0);
            }
            else if (SelectedPriceRange == PriceRange.r0_100)
            {
                query = query.Where(a => a.Amount <= 100 && a.Amount >= 0);
            }
            else if (SelectedPriceRange == PriceRange.r100_200)
            {
                query = query.Where(a => a.Amount <= 200 && a.Amount >= 100);
            }
            else if (SelectedPriceRange == PriceRange.r200_300)
            {
                query = query.Where(a => a.Amount <= 300 && a.Amount >= 200);
            }
            else if (SelectedPriceRange == PriceRange.r300plus)
            {
                query = query.Where(a => a.Amount >= 300);
            }
            else if (SelectedPriceRange == PriceRange.custom_price_range)
            {
                query = query.Where(a => a.Amount >= min && a.Amount <= max);
            }


            //trans number
            if (SelectedTransNumber != null && SelectedTransNumber != "")
            {
                int vari1 = Convert.ToInt32(SelectedTransNumber);
                query = query.Where(a => a.TransactionID == vari1);

            }

            //date range
            DateTime date;

            if (SelectedDateRange == DateRange.All)
            { }
            else if (SelectedDateRange == DateRange.Last15Days)
            {
                date = DateTime.Today.AddDays(-15);
                query = query.Where(a => a.Date >= date && a.Date <= DateTime.Today);
            }
            else if (SelectedDateRange == DateRange.Last30Days)
            {
                date = DateTime.Today.AddDays(-30);
                query = query.Where(a => a.Date >= date && a.Date <= DateTime.Today);
            }
            else if (SelectedDateRange == DateRange.Last45Days)
            {
                date = DateTime.Today.AddDays(-45);
                query = query.Where(a => a.Date >= date && a.Date <= DateTime.Today);
            }
            else if (SelectedDateRange == DateRange.Last60Days)
            {
                date = DateTime.Today.AddDays(-60);
                query = query.Where(a => a.Date >= date && a.Date <= DateTime.Today);
            }
            else if (SelectedDateRange == DateRange.custom_date_range)
            {
                int vari3 = Convert.ToInt32(custom_max_date);
                date = DateTime.Today.AddDays(-vari3);
                query = query.Where(a => a.Date >= date && a.Date <= DateTime.Today);
            }

            //trans type
            if (SelectedTransType == TransactionType.All)
            { }
            else if (SelectedTransType == TransactionType.Deposit)
            {
                query = query.Where(a => a.TransactionType == TransactionType.Deposit.ToString());
            }
            else if (SelectedTransType == TransactionType.Transfer)
            {
                query = query.Where(a => a.TransactionType == TransactionType.Transfer.ToString());
            }
            else if (SelectedTransType == TransactionType.Withdrawal)
            {
                query = query.Where(a => a.TransactionType == TransactionType.Withdrawal.ToString());
            }
            else if (SelectedTransType == TransactionType.Fee)
            {
                query = query.Where(a => a.TransactionType == TransactionType.Fee.ToString());
            }


            List<Transaction> SelectedTransactions = query.ToList();
            SelectedTransactions.OrderByDescending(a => a.TransactionID).ThenByDescending(a => a.TransactionType).ThenByDescending(a => a.Description).ThenByDescending(a => a.Amount).ThenByDescending(a => a.Date);
            ViewBag.TransactionsShown = "Showing " + SelectedTransactions.Count() + " out of " + db.Transactions.Count();
            return View("Index", SelectedTransactions);
        }
    }
}