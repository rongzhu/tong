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
    public class AmExParser : BaseParser
    {
        private class AmExCostcoItem
        {
            [CsvColumn(FieldIndex = 1)]
            public string TransactionDate { get; set; }
            [CsvColumn(FieldIndex = 2)]
            public string Description { get; set; }
			[CsvColumn(FieldIndex = 3)]
			public string AccountName { get; set; }
			[CsvColumn(FieldIndex = 4)]
			public string AccountNumber { get; set; }
            [CsvColumn(FieldIndex = 5)]
            public decimal? Amount { get; set; }
            [CsvColumn(FieldIndex = 6)]
            public string Padding1 { get; set; }
        }

        public override string PaymentMethod
        {
            get { return "AmEx"; }
        }

        public override bool CanParse(string content)
        {
            return GetFirstLine(content).EndsWith(",,,,,,,,");
        }

        public override List<Expense> Parse(string content)
        {
			//eg
			//08/08/2014  Fri,,"AMAZON.COM AMZN.COM/BILL WA","Rong Zhu","XXXX-XXXXXX-93000",,,23.39,,,,,,,,
            //reduce " ,,,,,,, " to " , "
            while (content.Contains(",,")) content = content.Replace(",,", ",");
			//replace the double space after date to comma

            CsvContext cc = new CsvContext();
            var rows = cc.Read<AmExCostcoItem>(content.ToReader(), new CsvFileDescription() { FirstLineHasColumnNames = false, EnforceCsvColumnAttribute = true });
            return (from r in rows
                    where !r.Description.Contains("ONLINE PAYMENT - THANK YOU")
                    select new Expense()
                    {
                        Amount = r.Amount ?? 0,
                        Description = r.Description,
                        TransactionDate = DateTime.Parse(r.TransactionDate.Split()[0]),
                        PaymentMethod = PaymentMethod
                    }).ToList();
        }
    }
}
