using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LINQtoCSV;
using System.Text;
using System.IO;
using tongbro.Models;

namespace tongbro.Models
{
	public class ExpenseEx : Expense
	{
		public bool Duplicate;
        public string Hint { get; set; }

		public ExpenseEx()
		{ }

		public ExpenseEx(Expense exp)
		{
			ExpenseID = exp.ExpenseID;
			TransactionDate = exp.TransactionDate;
			Description = exp.Description;
			Amount = exp.Amount;
			Category = exp.Category;
			PaymentMethod = exp.PaymentMethod;
		}
	}

    public abstract class BaseParser
    {
        abstract public string PaymentMethod { get; }
        public abstract bool CanParse(string content);
		public abstract List<Expense> Parse(string content);

        protected string GetFirstLine(string content)
        {
            int n = content.IndexOf('\n');
            if (n >= 0)
                return content.Substring(0, n).Trim();
            else
                return content;
        }
    }
}
