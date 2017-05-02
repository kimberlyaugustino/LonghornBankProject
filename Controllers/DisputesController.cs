using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LonghornBankProject.Models;
using Microsoft.AspNet.Identity;

namespace LonghornBankProject.Controllers
{
    public enum Resolve
    {
        Accepted,
        Rejected,
        Adjusted
    }

    public class DisputesController : BaseController
    {
        private AppDbContext db = new AppDbContext();

        // GET: Disputes
        public ActionResult Index()
        {
            return View(db.Disputes.ToList());
        }

        // GET: Disputes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dispute dispute = db.Disputes.Find(id);
            if (dispute == null)
            {
                return HttpNotFound();
            }
            return View(dispute);
        }

        // GET: Disputes/Create
        public ActionResult Create(int? id)
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
            if (transaction.Dispute != null)
            {
                return View("Error", new string[] { "You can only dispute a transaction once." });
            }
            ViewBag.transaction = id;
            ViewBag.DisputeID = new SelectList(db.Transactions, "TransactionID", "TransactionType");
            return View();
        }

        // POST: Disputes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Comments,Amount,Delete")] Dispute dispute, Int32 TransactionID)
        {
            if (ModelState.IsValid)
            {
                if (dispute.Amount <= 0)
                {
                    return View(dispute);
                }
                string id = User.Identity.GetUserId();
                dispute.Customer = db.Users.Find(id);
                dispute.Transaction = db.Transactions.Find(TransactionID);
                if (dispute.Transaction.TransactionType == "Withdrawal")
                {
                    dispute.Amount = -dispute.Amount;
                }
                dispute.Status = "Submitted";
                db.Disputes.Add(dispute);
                db.SaveChanges();
                ViewBag.Dispute = dispute.Status + " dispute";
                return RedirectToAction("ConfirmDispute", new { id = TransactionID});
            }

            ViewBag.DisputeID = new SelectList(db.Transactions, "TransactionID", "TransactionType", dispute.DisputeID);
            return View(dispute);
        }

        public ActionResult ConfirmDispute(int? id)
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
            ViewBag.id = id;
            return View();
        }

        [HttpGet]
        public ActionResult ResolveDispute(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dispute dispute = db.Disputes.Find(id);
            if (dispute == null)
            {
                return HttpNotFound();
            }
            Transaction transaction = db.Transactions.Find(dispute.DisputeID);
            ResolveDispute model = new ResolveDispute();
            model.dispute = dispute;
            model.transaction = transaction;
            ViewBag.id = transaction.Account.ProductID;
            ViewBag.disputeid = dispute.DisputeID;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResolveDispute([Bind(Include = "transaction")]ResolveDispute model, Resolve SelectedResolve, Int32 ProductID, string NewAmount, Int32 DisputeID)
        {
            if (ModelState.IsValid)
            {
                string id = User.Identity.GetUserId();
                AppUser manager = db.Users.Find(id);

                Product SelectedProduct = db.Accounts.Find(ProductID);
                Transaction transactionToChange = db.Transactions.Find(DisputeID);
                Dispute disputeToChange = db.Disputes.Find(DisputeID);
                transactionToChange.Comments = model.transaction.Comments;

                if (SelectedResolve == Resolve.Accepted)
                {
                    transactionToChange.Amount = disputeToChange.Amount;
                    
                    //How does this affect the other transfer transaction
                }
                else if (SelectedResolve == Resolve.Adjusted)
                {
                    if (NewAmount == "" || NewAmount == null)
                    {
                        return View(model);
                    }
                    decimal newAmount = Convert.ToDecimal(NewAmount);

                    if (newAmount <= 0)
                    {
                        return View(model);
                    }

                    if (transactionToChange.TransactionType == "Withdrawal")
                    {
                        newAmount = -newAmount;
                    }

                    transactionToChange.Amount = newAmount;

                }

                SelectedProduct.Balance = GetBalance(SelectedProduct);
                disputeToChange.Status = SelectedResolve.ToString();
                disputeToChange.Manager = manager;
                db.Entry(disputeToChange).State = EntityState.Modified;
                db.Entry(transactionToChange).State = EntityState.Modified;
                db.Entry(SelectedProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Disputes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dispute dispute = db.Disputes.Find(id);
            if (dispute == null)
            {
                return HttpNotFound();
            }
            return View(dispute);
        }

        // POST: Disputes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Dispute dispute = db.Disputes.Find(id);
            db.Disputes.Remove(dispute);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
