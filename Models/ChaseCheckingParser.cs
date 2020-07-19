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
			[CsvColumn(FieldIndex = 1, Name = "Details")]
			public string Padding1 { get; set; }
			[CsvColumn(FieldIndex = 2, Name = "Posting Date")]
			public DateTime TransactionDate { get; set; }
			[CsvColumn(FieldIndex = 3)]
			public string Description { get; set; }
			[CsvColumn(FieldIndex = 4)]
			public decimal Amount { get; set; }
			[CsvColumn(FieldIndex = 5)]
			public string Type { get; set; }
			[CsvColumn(FieldIndex = 6, Name = "Balance")]
			public string Padding2 { get; set; }
			[CsvColumn(FieldIndex = 7, Name = "Check or Slip #")]
			public string Padding3 { get; set; }
			[CsvColumn(FieldIndex = 8)]
			public string ExtraColumn { get; set; }
		}

		//private class ChaseCheckingItemAutoPost
		//{
		//	[CsvColumn(FieldIndex = 1)]
		//	public string Padding1 { get; set; }
		//	[CsvColumn(FieldIndex = 2)]
		//	public DateTime TransactionDate { get; set; }
		//	[CsvColumn(FieldIndex = 3)]
		//	public string Description { get; set; }
		//	[CsvColumn(FieldIndex = 4)]
		//	public decimal Amount { get; set; }
		//	[CsvColumn(FieldIndex = 5)]
		//	public string Type { get; set; }
		//	[CsvColumn(FieldIndex = 6)]
		//	public string Padding2 { get; set; }
		//	[CsvColumn(FieldIndex = 7)]
		//	public string Padding3 { get; set; }
		//}
		
		public override string PaymentMethod
        {
            get { return "Chase Checking"; }
        }

        public override bool CanParse(string content)
        {
			string firstLine = GetFirstLine(content);
			return firstLine.StartsWith("RONGCHCK") || firstLine == "Details,Posting Date,Description,Amount,Type,Balance,Check or Slip #";
        }

        public override List<Expense> Parse(string content)
        {
			List<Expense> exps = null;
			if (content.StartsWith("RONGCHCK"))
			{
				//e.g.
				//RONGCHCK,  12/30/2015  ,COSTCO GAS #0690 LAGUNA NIGUE CA     058441  12/30,$25.44  ,  
				//CsvContext cc = new CsvContext();
				//var rows = cc.Read<ChaseCheckingItemAutoPost>(content.ToReader(), new CsvFileDescription() { FirstLineHasColumnNames = false, EnforceCsvColumnAttribute = true }).ToList();
				//exps = (from r in rows
				//		select new Expense()
				//		{
				//			Amount = -r.Amount,
				//			Description = "[" + r.Type + "]" + r.Description,
				//			TransactionDate = r.TransactionDate,
				//			PaymentMethod = PaymentMethod
				//		}).ToList();
			}
			else
			{
				//e.g.
				//DEBIT,05/24/2016,"SO CAL GAS       PAID SCGC  1800098836      WEB ID: 1992052494",-24.43,ACH_DEBIT,11102.73,,

				//Chase bug: the column name row has 1 column less than data's
				content = content.Replace("Details,Posting Date,Description,Amount,Type,Balance,Check or Slip #", "Details,Posting Date,Description,Amount,Type,Balance,Check or Slip #,ExtraColumn");

				CsvContext cc = new CsvContext();
				var rows = cc.Read<ChaseCheckingItem>(content.ToReader(), new CsvFileDescription() { FirstLineHasColumnNames = true, EnforceCsvColumnAttribute = true });
				exps = (from r in rows
						select new Expense()
						{
							Amount = -r.Amount,
							Description = "[" + r.Type + "] " + r.Description,
							TransactionDate = r.TransactionDate,
							PaymentMethod = PaymentMethod
						}).ToList();
			}

			var keywords = ("COSTCO,SO CAL EDISON,SO CAL GAS,AMERICAN EXPRESS ACH PMT,THE GAS COMPANY,SPECTRUM MONTESO,CHECK_PAID,MNWD-WTR BILL,CITI AUTOPAY,DEBIT_CARD," +
				"KW MUSIC STUDIOS,QuickPay with Zelle,CHASE CREDIT CRD AUTOPAY")
				.Split(',');

			return exps.Where(r => keywords.Any(kw => r.Description.Contains(kw))).ToList();
        }

    }
}
