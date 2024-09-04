using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml;
using Flowingly.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Flowingly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private const decimal TaxRate = 10; //10%

        [HttpPost("processdata")]
        public IActionResult ProcessData([FromBody] string value)
        {
            var objExpense = new Expense();

            var xmlPattern = @"<expense>(.*?)</expense>";
            var match = Regex.Match(value, xmlPattern, RegexOptions.Singleline);
            if (!match.Success){
                return Ok(new { Status = "Invalid data!" });
            }

            var expenseXml = match.Value;
            try
            {
                var xmlDoc = XDocument.Parse(expenseXml);
                objExpense.CostCentre = xmlDoc.Descendants("cost_centre").FirstOrDefault()?.Value ?? "UNKNOWN";
                if (xmlDoc.Descendants("total").FirstOrDefault() == null)
                {
                    return Ok(new { Status = "Missing <total> field" });
                }
                objExpense.Total = decimal.Parse(xmlDoc.Descendants("total").FirstOrDefault()?.Value ?? "0");
                objExpense.PaymentMethod = xmlDoc.Descendants("payment_method").FirstOrDefault()?.Value;

                objExpense.SalesTax = (objExpense.Total * 10) / 100;
                objExpense.TotalExcludingTax = objExpense.Total - objExpense.SalesTax;

                return Ok(new { Status = "Success" , Expense = objExpense });
            }
            catch (XmlException ex)
            {
                return Ok(new { Status = "Invalid data!" });
            }

        }

    }
}
