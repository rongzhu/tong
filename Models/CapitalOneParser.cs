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
    public class CapitalOneParser : BaseParser
    {
        private class CapitalOneItem
        {
			[CsvColumn(FieldIndex = 1, Name = "Transaction Date")]
            public DateTime TransactionDate { get; set; }
			[CsvColumn(FieldIndex = 2, Name = "Posted Date")]
            public string Padding2 { get; set; }
			[CsvColumn(FieldIndex = 3, Name = "Card No.")]
            public string Padding3 { get; set; }
            [CsvColumn(FieldIndex = 4)]
            public string Description { get; set; }
            [CsvColumn(FieldIndex = 5)]
            public string Category { get; set; }
            [CsvColumn(FieldIndex = 6)]
            public decimal? Debit { get; set; }
            [CsvColumn(FieldIndex = 7)]
            public decimal? Credit { get; set; }
        }

        public override string PaymentMethod
        {
            get { return "Capital One"; }
        }

        public override bool CanParse(string content)
        {
			return GetFirstLine(content) == "Transaction Date,Posted Date,Card No.,Description,Category,Debit,Credit";
        }

        public override List<Expense> Parse(string content)
        {
            CsvContext cc = new CsvContext();
            var rows = cc.Read<CapitalOneItem>(content.ToReader());
            return (from r in rows
                    where !r.Description.Contains("ONLINE PYMT")
                    select new Expense()
                    {
                        Amount = r.Debit ?? -r.Credit ?? 0,
                        Description = r.Description,
                        TransactionDate = r.TransactionDate,
                        PaymentMethod = PaymentMethod
                    }).ToList();

        }

    }
}