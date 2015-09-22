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
    public class AmExTravelParser : BaseParser
    {
        private class MyDataRow : List<DataRowItem>, IDataRow
        { }

        private class AmExItem
        {
            [CsvColumn(FieldIndex = 1)]
            public DateTime TransactionDate { get; set; }
            [CsvColumn(FieldIndex = 2)]
            public string Reference { get; set; }
            [CsvColumn(FieldIndex = 3)]
            public decimal? Amount { get; set; }
            [CsvColumn(FieldIndex = 4)]
            public string Description { get; set; }
            [CsvColumn(FieldIndex = 5)]
            public string Cat { get; set; }
        }

        public override string PaymentMethod
        {
            get { return "AmEx"; }
        }

        public override bool CanParse(string content)
        {
            return GetFirstLine(content).Contains(",\"Reference: ");
        }

        public override List<Expense> Parse(string content)
        {
            CsvContext cc = new CsvContext();
            var rows = cc.Read<AmExItem>(content.ToReader(), new CsvFileDescription() { FirstLineHasColumnNames = false, EnforceCsvColumnAttribute = true });
            return (from r in rows
                    where !r.Description.Contains("ONLINE PAYMENT - THANK YOU")
                    select new Expense()
                    {
                        Amount = -r.Amount ?? 0,
                        Description = ShrinkSpace(r.Description),
                        TransactionDate = r.TransactionDate,
                        PaymentMethod = PaymentMethod
                    }).ToList();
        }

        private string ShrinkSpace(string value)
        {
            value = value.Replace("\t", " ");
            while (value.Contains("  "))
                value = value.Replace("  ", " ");

            return value;
        }
    }
}
