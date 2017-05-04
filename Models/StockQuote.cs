using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace LonghornBankProject.Models
{
    public class StockQuote
    {
        public Int32 StockQuoteID { get; set; }

        [Display(Name="Stock Symbol")]
        public String Symbol { get; set; }

        [Display(Name="Stock Name")]
        public String Name { get; set; }

        [Display(Name="Previous Close")]
        public Double PreviousClose { get; set; }

        [Display(Name="Last Trade Price")]
        public Double LastTradePrice { get; set; }

        public Double Volume { get; set; }
    }

    public class Stock
    {
        public Int32 StockID { get; set; }

        [Required(ErrorMessage ="Required to have a Ticker Symbol.")]
        [Display(Name ="Ticker Symbol")]
        public string TickerSymbol { get; set; }

        [Required(ErrorMessage = "Required to have a stock type.")]
        [Display(Name ="Stock Type")]
        public string StockType { get; set; }

        [Required(ErrorMessage = "Required to have a name.")]
        public string Name { get; set; }

        [Display(Name ="Stock Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Fee")]
        [DataType(DataType.Currency)]
        public decimal Fee { get; set; }

        [Display(Name="Number of Shares")]
        public int NumberOfShares { get; set; }
    }
        
    public class StockPortfolio
    {
        [ForeignKey("Account")]
        public Int32 StockPortfolioID { get; set; }

        [Display(Name ="Stock Portfolio Status")]
        public string Status { get; set; }

        [Display(Name ="Cash-Value")]
        public decimal CashValue { get; set; }

        public virtual List<Stock> Stocks { get; set; }

        public virtual Product Account { get; set; }

        public void StockValue()
        {
            decimal value = 0;
            foreach (var stock in Stocks)
            {
                value += (stock.Price * stock.NumberOfShares);
            }
            Account.Balance = CashValue + value;
        }
    }
}