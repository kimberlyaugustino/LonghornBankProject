using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LonghornBankProject.Models;
using LonghornBankProject.Utilities;

namespace LonghornBankProject.Controllers
{
    public class StocksController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Stocks
        public ActionResult Index()
        {
            return View(db.Stocks.ToList());
        }

        public ActionResult StockQuotes()
        {
            List<StockQuote> Quotes = new List<StockQuote>();
            DateTime now = DateTime.Now;
            StockQuote sq1 = GetQuote.GetStock("AAPL", now);
            Quotes.Add(sq1);

            StockQuote sq2 = GetQuote.GetStock("GOOG", now);
            Quotes.Add(sq2);

            StockQuote sq3 = GetQuote.GetStock("LUV", now);
            Quotes.Add(sq3);

            StockQuote sq4 = GetQuote.GetStock("AMZN", now);
            Quotes.Add(sq4);

            StockQuote sq5 = GetQuote.GetStock("TXN", now);
            Quotes.Add(sq5);

            StockQuote sq6 = GetQuote.GetStock("HSY", now);
            Quotes.Add(sq6);

            StockQuote sq7 = GetQuote.GetStock("V", now);
            Quotes.Add(sq7);

            StockQuote sq8 = GetQuote.GetStock("NKE", now);
            Quotes.Add(sq8);

            StockQuote sq9 = GetQuote.GetStock("VWO", now);
            Quotes.Add(sq9);

            StockQuote sq10 = GetQuote.GetStock("CORN", now);
            Quotes.Add(sq10);

            StockQuote sq11 = GetQuote.GetStock("F", now);
            Quotes.Add(sq11);

            StockQuote sq12 = GetQuote.GetStock("BAC", now);
            Quotes.Add(sq12);

            StockQuote sq13 = GetQuote.GetStock("VNQ", now);
            Quotes.Add(sq13);

            StockQuote sq14 = GetQuote.GetStock("KMX", now);
            Quotes.Add(sq14);

            StockQuote sq15 = GetQuote.GetStock("DIA", now);
            Quotes.Add(sq15);

            StockQuote sq16 = GetQuote.GetStock("SPY", now);
            Quotes.Add(sq16);

            StockQuote sq17 = GetQuote.GetStock("BEN", now);
            Quotes.Add(sq17);

            StockQuote sq18 = GetQuote.GetStock("PGSCX", now);
            Quotes.Add(sq18);

            return View(Quotes);
        }

        // GET: Stocks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        // GET: Stocks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Stocks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "StockID,TickerSymbol,StockType,Name,Price,Fee")] Stock stock)
        {
            if (ModelState.IsValid)
            {
                db.Stocks.Add(stock);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(stock);
        }

        // GET: Stocks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        // POST: Stocks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "StockID,TickerSymbol,StockType,Name,Price,Fee")] Stock stock)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stock).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stock);
        }

        // GET: Stocks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        // POST: Stocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Stock stock = db.Stocks.Find(id);
            db.Stocks.Remove(stock);
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
