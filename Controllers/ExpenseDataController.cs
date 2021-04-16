﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Net.Http;
//using System.Web.Mvc;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Configuration;
using tongbro.Models;

namespace tongbro.Controllers
{
    [Authorize]
    public class ExpenseDataController : ApiController
    {
        public class ExpensesResponse
        {
            public class Category
            {
                public string ID;
                public decimal Amount;
				public decimal YTM;
            }

            public Expense[] Expenses;
            public Category[] Categories;
			public Category[] CategoriesYTM;
		}

        [Route("monthly/{year:int}/{month:int}")]
        public ExpensesResponse GetMonthlyData(int year, int month)
        {
            DateTime cycleStart = new DateTime(year, month, 1);
            DateTime cycleEnd = cycleStart.AddMonths(1);
			DateTime yearStart = new DateTime(year, 1, 1);

            using (var db = new DB_76984_hostedEntities())
            {
                var exps = (from e in db.Expenses
                            where e.TransactionDate >= cycleStart && e.TransactionDate < cycleEnd
                            orderby e.TransactionDate descending
                            select e);

                var cats = (from e in exps
                            group e by e.Category into g
                            orderby g.Key
							select new ExpensesResponse.Category { ID = g.Key, Amount = g.Sum(x => x.Amount) }).ToArray();

				var catsYtm = (from e in db.Expenses.Where(x => x.TransactionDate >= yearStart && x.TransactionDate < cycleEnd)
							   group e by e.Category into g
							   orderby g.Key
							   select new { ID = g.Key, Amount = g.Sum(x => x.Amount) }).ToArray();

				foreach(var c in cats)
				{
					c.YTM = catsYtm.Where(x => x.ID == c.ID).Single().Amount;
				}

				return new ExpensesResponse
                {
                    Expenses = exps.ToArray(),
                    Categories = cats
                };
            }
        }

		public class LastCharge
		{
			public string Method;
			public DateTime Date;
		}

		[Route("expensedata/lastchargesdates")]
		public LastCharge[] GetLastChargesDates()
		{
			using (var db = new DB_76984_hostedEntities())
			{
				return (from e in db.Expenses
						group e by e.PaymentMethod into g
						orderby g.Max(x => x.TransactionDate) descending
						select new LastCharge { Method = g.Key, Date = g.Max(x => x.TransactionDate) }).ToArray();
			}
		}

        private List<Hint> _hints = null;

        private string PredictCategory(string desc, decimal amount)
        {
            if (_hints == null)
            {
                using (var db = new DB_76984_hostedEntities())
                    _hints = (from h in db.Hints select h).ToList();
            }

            desc = desc.ToLower();
            foreach (var h in _hints)
            {
                // Regex regex = new Regex(@"\b" + h.Keyword + @"\b", RegexOptions.IgnoreCase);
                Regex regex = new Regex(h.Keyword, RegexOptions.IgnoreCase);
                if (regex.IsMatch(desc))
					return h.Category;
            }

            return Math.Abs(amount) < 35 ? "1" : "";
        }

		private IEnumerable<ExpenseEx> FilterDuplicates(IEnumerable<ExpenseEx> expenses)
		{
			using (var db = new DB_76984_hostedEntities())
			{
				var descs = expenses.Select(x => x.Description);
				var potential_dups = db.Expenses.Where(x => descs.Contains(x.Description)).ToList();
				return expenses.Where(x => potential_dups.Any(y => y.TransactionDate == x.TransactionDate && y.Description == x.Description
					&& y.Amount == x.Amount && y.PaymentMethod == x.PaymentMethod)).AsEnumerable();
			}
		}

		private List<ExpenseEx> Parse(string raw)
		{
			BaseParser parser = new List<BaseParser>() {
                    new ChaseCreditParser(),
                    new CapitalOneParser(), 
                    new AmExParser(),
                    new CitiCreditParser(),
			        new ChaseCheckingParser()
                }.FirstOrDefault(p => p.CanParse(raw));

			if (parser == null) return null;

			try
			{
				var expenses = parser.Parse(raw).Select(x => new ExpenseEx(x) { Category = PredictCategory(x.Description, x.Amount) }).ToList();

				//if it's Chase Checking, only leave the ones after the last charge
				if (parser.GetType().Name == "ChaseCheckingParser")
				{
					DateTime lastCheckingDate = GetLastChargesDates().Single(lc => lc.Method == "Chase Checking").Date;
					expenses = expenses.Where(exp => exp.TransactionDate >= lastCheckingDate).ToList();
				}

				foreach (var exp in FilterDuplicates(expenses))
				{
					exp.Duplicate = true;
				}

				expenses.ForEach(exp => exp.Description = Util.LimitLen(exp.Description, 100));

				return expenses;
			}
			catch(LINQtoCSV.AggregatedException ex)
			{
				throw new Exception(string.Join("\r\n", ex.m_InnerExceptionsList.Select(e => e.Message).ToArray()));
			}
		}

        [Route("expensedata/parseraw")]
        [HttpPost]
        public object ParseRaw([FromBody] string raw)
        {
			return Parse(raw);
        }

        public class SaveResponse
        {
            public string[] badHints;
			public ExpenseEx[] duplicateExpenses;
        }

        [Route("expensedata/save")]
        [HttpPost]
		public SaveResponse Save([FromBody] ExpenseEx[] expenses)
        {
			//Transaction submitted from <input type=date> where the timezone is GMT. Convert back to local.
			foreach (var exp in expenses)
			{
				exp.TransactionDate = exp.TransactionDate.ToLocalTime();
				exp.Description = Util.LimitLen(exp.Description, 100);
			}

			var dups = FilterDuplicates(expenses);

            using (var db = new DB_76984_hostedEntities())
            {
                var existingHints = db.Hints.Select(x => x.Keyword).ToArray();

                foreach (Expense e in expenses.Except(dups))
                {
                    db.Expenses.Add(new Expense()
                        {
                            TransactionDate = e.TransactionDate,
                            Description = e.Description,
                            Amount = e.Amount,
                            Category = e.Category,
                            PaymentMethod = e.PaymentMethod
                        });
                }

                List<string> newHints = new List<string>();
                List<string> badHints = new List<string>();

                foreach (var e in expenses.Where(x => x.Hint.HasContent()))
                {
                    string hint = Regex.Escape(e.Hint);
                    if (!existingHints.Contains(hint) && !newHints.Contains(hint))
                    {
                        // var re = new Regex(@"\b" + hint + @"\b");
                        var re = new Regex(hint);

                        //check for conflict
                        //var firstConflict = db.Expenses.Where(x => x.Description.Contains(hint) && x.Category != e.Category).AsEnumerable()
                        //        .FirstOrDefault(x => re.IsMatch(x.Description));

                        //if (firstConflict != null)
                        //{
                        //    badHints.Add(string.Format("\"{0}\" matches \"{1}\" but is of category {2}", firstConflict.Description, hint, firstConflict.Category));
                        //}

                        string existingPrediction = PredictCategory(e.Description, e.Amount);
                        if (existingPrediction.HasContent() && existingPrediction != e.Category)
                        {
                            badHints.Add(string.Format($"Existing prediction is {existingPrediction} but new prediction is {e.Category}"));
                        }
                        else
                        {
                            db.Hints.Add(new Hint() { Category = e.Category, Keyword = hint });
                            newHints.Add(hint);
                        }
                    }
                }

				try
				{
					db.SaveChanges();
				}
				catch (Exception ex)
				{
					int i = 0;
				}

                return new SaveResponse { badHints = badHints.ToArray(), duplicateExpenses = dups.ToArray() };
            }
        }

        public class SearchRequest
        {
            public DateTime? startDate;
            public DateTime? endDate;
            public string textToSearch;
            public string[] selectedCategories;
        }

        [Route("expensedata/search")]
        [HttpPost]
        public ExpensesResponse Search([FromBody] SearchRequest req)
        {
            if (req.endDate.HasValue) req.endDate = req.endDate.Value.AddDays(1);
            if (req.selectedCategories == null) req.selectedCategories = new string[0];

            using (var db = new DB_76984_hostedEntities())
            {
                bool useCategories = req.selectedCategories.Length > 0;

                var exps = (from e in db.Expenses
                            where (req.startDate == null || e.TransactionDate >= req.startDate) && 
                                    (req.endDate == null || e.TransactionDate < req.endDate) && 
                                    (string.IsNullOrEmpty(req.textToSearch) || e.Description.Contains(req.textToSearch)) &&
                                    (!useCategories || req.selectedCategories.Contains(e.Category))
                            orderby e.TransactionDate descending
                            select e);

                var cats = (from e in exps
                            group e by e.Category into g
                            orderby g.Key
                            select new { ID = g.Key, Amount = g.Sum(x => x.Amount) }).AsEnumerable();

                return new ExpensesResponse
                {
                    Expenses = exps.ToArray(),
                    Categories = cats.Select(c => new ExpensesResponse.Category { ID = c.ID, Amount = (int)Math.Round(c.Amount, MidpointRounding.AwayFromZero) }).ToArray()
                };
            }
        }

		[Route("expensedata/parsetempposted")]
		[HttpGet]
		public object ParseTempPosted(string method)
		{
			var client = new HttpClient();
			string csv = client.GetStringAsync(ConfigurationManager.AppSettings["DataUrl"]).Result;
			var exps = Parse(csv);
			var startDate = GetLastChargesDates().Single(x => x.Method == method).Date.AddDays(-3);
			return exps.Where(x => x.TransactionDate >= startDate && !x.Duplicate);
		}

		[Route("expensedata/postcsv")]
		[HttpPost]
		public bool PostCsv()
		{
			var ctxt = System.Web.HttpContext.Current;
			ctxt.Application["PostedExpenses"] = ctxt.Request.Form["csv"];
			ctxt.Response.AddHeader("Access-Control-Allow-Origin", "*");
			return true;
		}

		[Route("expensedata/logout")]
		[HttpGet]
		public bool Logout()
		{
			FormsAuthentication.SignOut();
			return true;
		}

    }
}
