using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Flurl;
using Flurl.Http;
using System.Text;
using System.Linq;
namespace HelloWorldThanos.Function
{
    public static class Salute
    {
        [FunctionName("Salute")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //validate google recaptcha
            if (!await IsValidRecaptcha(req.Form["recaptcha_response"]))
                return new BadRequestResult();

            //get form fields
            var firstName = req.Form["firstName"][0];
            var lastName = req.Form["lastName"][0];

            //check snap status
            var snapped = DidThanosSnappedThisPerson(firstName, lastName);

            //return value
            return new OkObjectResult(new { snapped });
        }

        private static bool DidThanosSnappedThisPerson(string firstName, string lastName)
        {
            var bytes = Encoding.ASCII.GetBytes((firstName + lastName).ToLower().Trim());
            return bytes.Select(b => (int)b).Sum() % 2 == 1;
        }

        private static async Task<bool> IsValidRecaptcha(string token)
        {
            var url = "https://www.google.com/recaptcha/api/siteverify";
            var key = Environment.GetEnvironmentVariable("RECAPTCHA_SECRET_KEY");
            var result = await url.SetQueryParam("secret", key)
            .SetQueryParam("response", token).PostAsync();
            dynamic response = await result.GetJsonAsync<dynamic>();
            return (bool)(response.success);
        }
    }
}
