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
	public class ChaseCheckingParser : BaseParser
    {
		private class ChaseCheckingItem
        {
            [CsvColumn(FieldIndex = 1)]
            public DateTime TransactionDate { get; set; }
            [CsvColumn(FieldIndex = 2)]
            public string Type { get; set; }
            [CsvColumn(FieldIndex = 3)]
            public string Description { get; set; }
            [CsvColumn(FieldIndex = 4)]
            public decimal Amount { get; set; }
            [CsvColumn(FieldIndex = 5)]
            public string Padding1 { get; set; }
            [CsvColumn(FieldIndex = 6)]
            public string Padding2 { get; set; }
        }

		private class ChaseCheckingItemAutoPost
		{
			[CsvColumn(FieldIndex = 1)]
			public string Padding1 { get; set; }
			[CsvColumn(FieldIndex = 2)]
			public DateTime TransactionDate { get; set; }
            [CsvColumn(FieldIndex = 3)]
            public string Type { get; set; }
			[CsvColumn(FieldIndex = 4)]
			public string Description { get; set; }
			[CsvColumn(FieldIndex = 5)]
			public string Debit { get; set; }
			[CsvColumn(FieldIndex = 6)]
			public string Credit { get; set; }
		}
		
		public override string PaymentMethod
        {
            get { return "Chase Checking"; }
        }

        public override bool CanParse(string content)
        {
			string firstLine = GetFirstLine(content);
			if (firstLine.StartsWith("RONGCHCK"))
				return true;
			else
			{
				string[] parts = firstLine.Split('\t');
				return (parts.Length >= 2 && new string[] { "ACH Debit", "Check", "ACH Credit", "Deposit", "Account Transfer", "Loan Payment" }.Contains(parts[1].Trim()));
			}
        }

        public override List<Expense> Parse(string content)
        {
			List<Expense> exps;
			if (content.StartsWith("RONGCHCK"))
			{
				//e.g.
				//RONGCHCK,  12/30/2015  ,COSTCO GAS #0690 LAGUNA NIGUE CA     058441  12/30,$25.44  ,  
				CsvContext cc = new CsvContext();
				var rows = cc.Read<ChaseCheckingItemAutoPost>(content.ToReader(), new CsvFileDescription() { FirstLineHasColumnNames = false, EnforceCsvColumnAttribute = true }).ToList();
				exps = (from r in rows
						select new Expense()
						{
							Amount = decimal.Parse((r.Credit.Trim().HasContent() ? r.Credit : r.Debit).Trim().TrimStart('$')),
							Description = (r.Type.Trim() == "Check" ? "[CHECK] " : "") + r.Description,
							TransactionDate = r.TransactionDate,
							PaymentMethod = PaymentMethod
						}).ToList();
			}
			else
			{
				//e.g.
				//07/21/2014   	ACH Debit	SO CAL EDISON CO BILL PAYMT 349794602 WEB ID: 4951240335	$34.85  	  	$10,780.36  
				content = content.Replace(",", "").Replace('\t', ',');			//remove ',' in $10,780.36
				CsvContext cc = new CsvContext();
				var rows = cc.Read<ChaseCheckingItem>(content.ToReader(), new CsvFileDescription() { FirstLineHasColumnNames = false, EnforceCsvColumnAttribute = true });
				exps = (from r in rows
						select new Expense()
						{
							Amount = r.Amount,
							Description = (r.Type == "Check" ? "[CHECK] " : "") + r.Description,
							TransactionDate = r.TransactionDate,
							PaymentMethod = PaymentMethod
						}).ToList();
			}

			return exps.Where(r => r.Description.Contains("COSTCO") || r.Description.Contains("SO CAL EDISON CO") || r.Description.Contains("SO CAL GAS") ||
						r.Description.Contains("AMERICAN EXPRESS ACH PMT") || r.Description.Contains("THE GAS COMPANY") ||
						r.Description.Contains("Auto Loan 3103") || r.Description.StartsWith("[CHECK]")).ToList();
        }

    }
}
