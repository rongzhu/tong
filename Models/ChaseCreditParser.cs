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
    public class ChaseCreditParser : BaseParser
    {
        private class ChaseCreditItem
        {
            public string Padding1 { get; set; }
            [CsvColumn(FieldIndex = 1, Name = "Transaction Date")]
            public DateTime TransactionDate { get; set; }
            [CsvColumn(FieldIndex = 2, Name = "Post Date")]
            public DateTime PostDate { get; set; }
            [CsvColumn(FieldIndex = 3)]
            public string Description { get; set; }
			[CsvColumn(FieldIndex = 4)]
			public string Category { get; set; }
			[CsvColumn(FieldIndex = 5)]
            public string Type { get; set; }
            [CsvColumn(FieldIndex = 6)]
            public decimal Amount { get; set; }
			[CsvColumn(FieldIndex = 7)]
			public decimal Memo { get; set; }
		}

		private class ChaseCreditItemAutoPost
		{
			[CsvColumn(FieldIndex = 1)]
			public string Padding1 { get; set; }
			[CsvColumn(FieldIndex = 2)]
			public DateTime TransactionDate { get; set; }
			[CsvColumn(FieldIndex = 3)]
			public string Description { get; set; }
			[CsvColumn(FieldIndex = 4)]
			public string Amount { get; set; }
		}

		public override string PaymentMethod
        {
            get { return "Chase Credit"; }
        }

		public override bool CanParse(string content)
		{
			string firstLine = GetFirstLine(content);
			if (firstLine.StartsWith("RONGCHCC"))
				return true;
			else
                return firstLine == "Transaction Date,Post Date,Description,Category,Type,Amount,Memo";
        }

        public override List<Expense> Parse(string content)
		{
			//fix Chase csv encoding error for ',' in "99 RANCH MARKET, #"
			content = content.Replace("99 RANCH MARKET, #", "99 RANCH MARKET #");

			List<Expense> exps;
			if (content.StartsWith("RONGCHCC"))
			{
				//e.g.
				//RONGCHCC,11/15/2015,Amazon.com,$7.55
				CsvContext cc = new CsvContext();
				var rows = cc.Read<ChaseCreditItemAutoPost>(content.ToReader(), new CsvFileDescription() { FirstLineHasColumnNames = false, EnforceCsvColumnAttribute = true }).ToList();
				exps = (from r in rows
						select new Expense()
						{
							Amount = decimal.Parse(r.Amount.Replace("$", "")),
							Description = r.Description,
							TransactionDate = r.TransactionDate,
							PaymentMethod = PaymentMethod
						}).ToList();
			}
			else
			{
				CsvContext cc = new CsvContext();
				var rows = cc.Read<ChaseCreditItem>(content.ToReader());
				exps = (from r in rows
						select new Expense()
						{
							Amount = -r.Amount,
							Description = r.Description,
							TransactionDate = r.TransactionDate,
							PaymentMethod = PaymentMethod
						}).ToList();

			}

			return exps.Where(r => !r.Description.Contains("Payment Thank You")).ToList();
		}

    }
}
