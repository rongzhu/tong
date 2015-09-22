using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using tongbro.Models;

namespace tongbro.Controllers
{
    [Authorize]
    public class ExpenseDataController : ApiController
    {
        private Dictionary<string, string> CategoryCharToName = new Dictionary<string, string>()
        { 
            { "1", "Household" },
            { "2", "Gas" },
            { "3", "Entertainment" },
            { "4", "Durable" },
            { "5", "Vacation" }
        };

        public class ExpensesResponse
        {
            public class Category
            {
                public string ID;
                public int Amount;
            }

            public Expense[] Expenses;
            public Category[] Categories;
        }

        [Route("monthly/{year:int}/{month:int}")]
        public ExpensesResponse GetMonthlyData(int year, int month)
        {
            string category = null;

            DateTime cycleStart = new DateTime(year, month, 1);
            DateTime cycleEnd = cycleStart.AddMonths(1);

            using (var db = new DB_76984_hostedEntities())
            {
                var exps = (from e in db.Expenses
                            where e.TransactionDate >= cycleStart && e.TransactionDate < cycleEnd && (string.IsNullOrEmpty(category) || e.Category == category)
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

        [Route("expensedata/lastchargesdates")]
        public object[] GetLastChargesDates()
        {
            using (var db = new DB_76984_hostedEntities())
            {
                return (from e in db.Expenses
                        group e by e.PaymentMethod into g
                        orderby g.Max(x => x.TransactionDate) descending
                        select new { Method = g.Key, Date = g.Max(x => x.TransactionDate) }).ToArray();
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
                Regex regex = new Regex(@"\b" + h.Keyword + @"\b");
                if (regex.IsMatch(desc)) return h.Category;
            }

            return amount < 20 ? "1" : "";
        }

        [Route("expensedata/parseraw")]
        [HttpPost]
        public object ParseRaw([FromBody] string raw)
        {
            BaseParser parser = new List<BaseParser>() {
                    new ChaseCreditParser(),
                    new CapitalOneParser(), 
                    new AmExParser(),
                    new CitiCreditParser(),
			        new ChaseCheckingParser()
                }.FirstOrDefault(p => p.CanParse(raw));

            return parser != null ? (object)parser.Parse(raw).Select(x => { x.Category = PredictCategory(x.Description, x.Amount); return x; }) : -1;
        }

        public class ExpenseInput : Expense
        {
            public string Hint { get; set; }
        }

        public class SaveResponse
        {
            public string[] badHints;
            public ExpenseInput[] duplicateExpenses;
        }

        [Route("expensedata/save")]
        [HttpPost]
        public SaveResponse Save([FromBody] ExpenseInput[] expenses)
        {
            using (var db = new DB_76984_hostedEntities())
            {
                var existingHints = db.Hints.Select(x => x.Keyword).ToArray();

                var descs = expenses.Select(x => x.Description);
                var potential_dups = db.Expenses.Where(x => descs.Contains(x.Description)).AsEnumerable();
                var dups = expenses.Where(x => potential_dups.Any(y => y.TransactionDate == x.TransactionDate && y.Description == x.Description
                    && y.Amount == x.Amount && y.PaymentMethod == x.PaymentMethod)).ToArray();

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
                    //chars that need escaping for use in Regex
                    string escs = @".^$+?()[{\|";
                    StringBuilder sb = new StringBuilder();
                    foreach (char c in e.Hint.Trim())
                    {
                        if (escs.Contains(c)) sb.Append(@"\");
                        sb.Append(c);
                    }

                    string hint = sb.ToString();
                    if (!existingHints.Contains(hint) && !newHints.Contains(hint))
                    {
                        var re = new Regex(@"\b" + hint + @"\b");

                        //check for conflict
                        var firstConflict = db.Expenses.Where(x => x.Description.Contains(hint) && x.Category != e.Category).AsEnumerable()
                                .FirstOrDefault(x => re.IsMatch(x.Description));

                        if (firstConflict != null)
                        {
                            badHints.Add(string.Format("\"{0}\" matches \"{1}\" but is of category {2}", firstConflict.Description, hint, firstConflict.Category));
                        }
                        else
                        {
                            db.Hints.Add(new Hint() { Category = e.Category, Keyword = hint });
                            newHints.Add(hint);
                        }
                    }
                }

                db.SaveChanges();

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

        [Route("expensedata/logout")]
        [HttpGet]
        public bool Logout()
        {
            FormsAuthentication.SignOut();
            return true;
        }
    }
}
