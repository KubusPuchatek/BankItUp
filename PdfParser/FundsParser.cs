using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace PdfParser
{
    public class FundsParser
    {
  
        private static readonly string filePath = @"Fundusze.xlsx";

        //Used to speedup loading grants file
        private static ExcelWorksheet _worksheet;
        public static ExcelWorksheet Worksheet
        {
            get
            {
                if (_worksheet == null)
                {
                    ExcelPackage xlPackage = new ExcelPackage(new FileInfo(filePath));
                    _worksheet = xlPackage.Workbook.Worksheets.First();
                }
                return _worksheet;
            }
        }



        //To get updated funds file please visit: http://www.funduszeeuropejskie.gov.pl/lista-projektow
        public List<Grant> Parse(string companyName)
        {
            var grants = new List<Grant>();

            var companyGrants = Worksheet.Cells["D:D"].Where(x => x.Value.ToString().ToLower().Contains(companyName));

            grants.AddRange(companyGrants.Select(item => item.Address.Replace("D", ""))
                .Select(rowNumber => new Grant
                {
                    Title = Worksheet.Cells["A" + rowNumber + ":A" + rowNumber].Value.ToString(),
                    Priority = Worksheet.Cells["G" + rowNumber + ":G" + rowNumber].Value.ToString(),
                    Measure = Worksheet.Cells["H" + rowNumber + ":H" + rowNumber].Value.ToString(),
                    Submeasure = Worksheet.Cells["I" + rowNumber + ":I" + rowNumber].Value.ToString(),
                    Amount = Worksheet.Cells["L" + rowNumber + ":L" + rowNumber].Value.ToString(),
                    Form = Worksheet.Cells["N" + rowNumber + ":N" + rowNumber].Value.ToString()
                }));

            return grants;

        }
    }
}
