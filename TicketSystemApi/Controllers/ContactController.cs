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
    [RoutePrefix("ticket")]
    public class ContactController : ApiController
    {
        private readonly ICrmService _crmService;

        public ContactController()
        {
            _crmService = new CrmService();
        }

        [HttpGet]
        [Route("customer")]
        public IHttpActionResult GetContactUrlByPhone([FromUri] string phone)
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
                    new ConditionExpression("mobilephone", ConditionOperator.Equal, phone)
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



        [HttpPost]
        [Route("createnote")]
        public IHttpActionResult CreateNote([FromBody] InteractionNoteModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.PhoneNumber))
                return Ok(ApiResponse<object>.Error("Invalid request"));

            try
            {
                var service = _crmService.GetService();

                // Find the Contact by phone number
                var contactQuery = new QueryExpression("contact")
                {
                    ColumnSet = new ColumnSet("contactid"),
                    Criteria =
            {
                Conditions =
                {
                    new ConditionExpression("mobilephone", ConditionOperator.Equal, model.PhoneNumber)
                }
            }
                };

                var contactResult = service.RetrieveMultiple(contactQuery);
                var contact = contactResult.Entities.FirstOrDefault();

                if (contact == null)
                    return Ok(ApiResponse<object>.Error("Contact not found for provided phone number"));

                var note = new Entity("annotation");
                note["subject"] = "Genesys Call Summary";
                note["notetext"] = $"Interaction ID: {model.InteractionId}\nDisposition: {model.DispositionCode}\nRecording URL: {model.RecordingUrl}";
                note["objectid"] = new EntityReference("contact", contact.Id);

                service.Create(note);

                return Ok(ApiResponse<object>.Success("Note created successfully"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error($"CRM error: {ex.Message}"));
            }
        }
    }
}
