using System.Linq;
using System.Web.Http;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TicketSystemApi.Services;
using TicketSystemApi.Models;
using System.Configuration;
using System;


namespace TicketSystemApi.Controllers
{
    [RoutePrefix("api/contact")]
    public class ContactController : ApiController
    {
        private readonly ICrmService _crmService;

        public ContactController()
        {
            _crmService = new CrmService();
        }

        [HttpGet]
        [Route("contact-url/{phoneNumber}")]
        public IHttpActionResult GetContactUrlByPhone(string phoneNumber)
        {
            try
            {
                var service = _crmService.GetService();

                var query = new QueryExpression("contact")
                {
                    ColumnSet = new ColumnSet("contactid"),
                    Criteria =
            {
                Conditions =
                {
                    new ConditionExpression("mobilephone", ConditionOperator.Equal, phoneNumber)
                }
            }
                };

                var contacts = service.RetrieveMultiple(query);
                var contact = contacts.Entities.FirstOrDefault();

                if (contact == null)
                {
                    return Ok(ApiResponse<object>.Error("No contact found with this phone number"));
                }

                Guid contactId = contact.Id;

                string baseUrl = ConfigurationManager.AppSettings["CrmBaseUrl"];
                string crmUrl = $"{baseUrl}/main.aspx?pagetype=entityrecord&etn=contact&id={contactId}";

                return Ok(ApiResponse<object>.Success(new { url = crmUrl }));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error($"CRM error: {ex.Message}"));
            }
        }
    }
}