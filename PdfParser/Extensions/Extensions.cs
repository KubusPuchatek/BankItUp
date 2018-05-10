using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfParser.Extensions
{
    public static class Extensions
    {
        public static bool TryParseLinkedInEmploymentDuration(this string input, out DateTime[] result)
        {
            try
            {
                var indexOfEnd = input.LastIndexOf("(", StringComparison.Ordinal);

                if (indexOfEnd != -1)
                {
                    input = input.Substring(0, indexOfEnd);
                }
           
                var splitedInput = input.Split('-');

                result = new DateTime[2];

                //Start date
                DateTime.TryParse(splitedInput[0].Trim(), out result[0]);

                //End Date
                if (splitedInput[1].Trim().Equals("Present"))
                {
                    result[1] = DateTime.Now;
                }
                else
                {
                    DateTime.TryParse(splitedInput[1].Trim(), out result[1]);
                }

                return result[0]!=DateTime.MinValue && result[1]!=DateTime.MinValue;
            }
            catch (Exception e)
            {
                result = new DateTime[2];
                return false;
            }
        }
    }
}
