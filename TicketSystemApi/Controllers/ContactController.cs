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
    [RoutePrefix("api/ticket")]
    public class ContactController : ApiController
    {
        private readonly ICrmService _crmService;

        public ContactController()
        {
            _crmService = new CrmService();
        }

        [HttpGet]
        [Route("customer")]
        public IHttpActionResult GetContactUrlByPhone([FromUri] string number)
        {
            try
            {
                var service = _crmService.GetService();

                var query = new QueryExpression("phone")
                {
                    ColumnSet = new ColumnSet("contactid"),
                    Criteria =
            {
                Conditions =
                {
                    new ConditionExpression("mobilephone", ConditionOperator.Equal, number)
                }
            }
                };

                var contacts = service.RetrieveMultiple(query);
                var contact = contacts.Entities.FirstOrDefault();

                string baseUrl = ConfigurationManager.AppSettings["CrmBaseUrl"];
                string crmUrl;

                if (contact != null)
                {
                    Guid contactId = contact.Id;
                    crmUrl = $"{baseUrl}/main.aspx?pagetype=entityrecord&etn=contact&id={contactId}";
                }
                else
                {
                    crmUrl = $"{baseUrl}/main.aspx?pagetype=entityrecord&etn=contact";
                }

                return Ok(ApiResponse<object>.Success(new { url = crmUrl }));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error($"CRM error: {ex.Message}"));
            }
        }
    }
}