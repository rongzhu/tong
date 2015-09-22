using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tongbro;
using tongbro.Models;

namespace tongbro.Models
{
	public class Reporter
	{
		public Dictionary<string, string> CategoryCharToName = new Dictionary<string, string>()
        { 
            { "1", "Household" },
            { "2", "Gas" },
            { "3", "Entertaining" },
            { "4", "Durable" },
            { "5", "Vacation" }
        };

		public List<Tuple<string, DateTime>> GetPaymentMethodLastDates()
		{
			using (var db = new DB_76984_hostedEntities())
			{
				return (from e in db.Expenses
						group e by e.PaymentMethod into grp
						orderby grp.Key
						select new { grp.Key, grp }).AsEnumerable().Select(g => Tuple.Create<string, DateTime>(g.Key, g.grp.Max(e => e.TransactionDate))).ToList();
			}
		}

		public List<Tuple<string, string, decimal>> GetCycleSummary(int monthAgo)
		{
			DateTime cycleStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			cycleStart = cycleStart.AddMonths(-monthAgo);
			DateTime cycleEnd = cycleStart.AddMonths(1);

			using (var db = new DB_76984_hostedEntities())
			{
				var l = (from e in db.Expenses
						 where e.TransactionDate >= cycleStart && e.TransactionDate < cycleEnd
						 group e by e.Category into grp
						 orderby grp.Key
						 select new { grp.Key, grp }).ToList();

				//var l2 = from cc in CategoryCharToName.Keys
				//		 join c in l on cc equals c.Key into g
				//		 from c in g.DefaultIfEmpty()
				//		 select Tuple.Create(cc, CategoryCharToName[cc], c != null ? c.grp.Sum(e => e.Amount) : 0);

				var ret = l.Select(g => Tuple.Create<string, string, decimal>(g.Key, CategoryCharToName[g.Key], g.grp.Sum(e => e.Amount))).ToList();

				if (ret.Count == 0) ret.Add(Tuple.Create("1", CategoryCharToName["1"], 0M));

				return ret;
			}
		}

		public List<Expense> GetTransactions(int monthAgo, string category)
		{
			DateTime cycleStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			cycleStart = cycleStart.AddMonths(-monthAgo);
			DateTime cycleEnd = cycleStart.AddMonths(1);

			using (var db = new DB_76984_hostedEntities())
			{
				var l = (from e in db.Expenses
						 where e.TransactionDate >= cycleStart && e.TransactionDate < cycleEnd && (string.IsNullOrEmpty(category) || e.Category == category)
						 orderby e.TransactionDate descending
						 select e).ToList();
				return l;
			}
		}
	}
}
