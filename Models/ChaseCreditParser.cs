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
            [CsvColumn(FieldIndex = 1)]
            public string Type { get; set; }
            [CsvColumn(FieldIndex = 2, Name = "Trans Date")]
            public DateTime TransactionDate { get; set; }
            [CsvColumn(FieldIndex = 3, Name = "Post Date")]
            public DateTime PostDate { get; set; }
            [CsvColumn(FieldIndex = 4)]
            public string Description { get; set; }
            [CsvColumn(FieldIndex = 5)]
            public decimal Amount { get; set; }
        }

        public override string PaymentMethod
        {
            get { return "Chase Credit"; }
        }

        public override bool CanParse(string content)
        {
            return GetFirstLine(content) == "Type,Trans Date,Post Date,Description,Amount";
        }

        public override List<Expense> Parse(string content)
        {
            CsvContext cc = new CsvContext();
            var rows = cc.Read<ChaseCreditItem>(content.ToReader());
            return (from r in rows
                    where !r.Description.Contains("Payment Thank You")
                    select new Expense()
                    {
                        Amount = -r.Amount,
                        Description = r.Description,
                        TransactionDate = r.TransactionDate,
                        PaymentMethod = PaymentMethod
                    }).ToList();

        }

    }
}
