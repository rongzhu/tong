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
    public class CitiCreditParser : BaseParser
    {
        private class CitiCreditItem
        {
            [CsvColumn(FieldIndex = 1)]
            public string Status { get; set; }
            [CsvColumn(FieldIndex = 2)]
            public DateTime Date { get; set; }
            [CsvColumn(FieldIndex = 3)]
            public string Description { get; set; }
            [CsvColumn(FieldIndex = 4)]
            public decimal? Debit { get; set; }
            [CsvColumn(FieldIndex = 5)]
            public decimal? Credit { get; set; }
        }

        public override string PaymentMethod
        {
            get { return "Citi Credit"; }
        }

        public override bool CanParse(string content)
        {
            return GetFirstLine(content) == @"""Status"",""Date"",""Description"",""Debit"",""Credit""";
        }

		public override List<Expense> Parse(string content)
		{
			CsvContext cc = new CsvContext();

			var rows = cc.Read<CitiCreditItem>(content.ToReader(), new CsvFileDescription() { FirstLineHasColumnNames = true, EnforceCsvColumnAttribute = true }).ToList();

			return (from r in rows
					where !r.Description.Contains("ONLINE PAYMENT")
					select new Expense()
					{
						Amount = r.Debit ?? -(r.Credit ?? 0),
						Description = r.Description,
						TransactionDate = r.Date,
						PaymentMethod = PaymentMethod
					}).ToList();
		}
    }
}
