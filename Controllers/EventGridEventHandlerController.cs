using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace EventGrid.Webhook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventGridEventHandlerController(ILogger<EventGridEventHandlerController> logger) : ControllerBase
    {
        private readonly ILogger<EventGridEventHandlerController> _logger = logger;

        /// <summary>
        /// Post method to process events from Event Grid Topic.
        /// </summary>
        /// <returns></returns>
        [HttpPost("processevents")]
        [Authorize(AuthenticationSchemes = "eventgrid")]
        public async Task<IActionResult> ProcessEventGridEvents()
        {
            try
            {
                Request.Headers.TryGetValue("Authorization", out StringValues authorizationHeaders);

                BinaryData events = await BinaryData.FromStreamAsync(Request.Body);
                _logger.LogInformation("{@Mesage} Received events", events);

                EventGridEvent[] eventGridEvents = EventGridEvent.ParseMany(events);

                foreach (EventGridEvent eventGridEvent in eventGridEvents)
                {
                    if (eventGridEvent.TryGetSystemEventData(out object eventData))
                    {
                        // Handle the subscription validation event
                        if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
                        {
                            _logger.LogInformation("{@ValidationCode} {@TopicName} Got SubscriptionValidation event data",
                                subscriptionValidationEventData.ValidationCode,
                                eventGridEvent.Topic);

                            // Do any additional validation (as required) and then return back the below response
                            var responseData = new
                            {
                                ValidationResponse = subscriptionValidationEventData.ValidationCode
                            };
                            return new OkObjectResult(responseData);
                        }
                        else
                        {
                            _logger.LogInformation("{@Message} Received event data", eventGridEvent.EventType);
                        }
                    }
                    else
                    {
                        // Handle the event
                        _logger.LogInformation("{@Message} Received event data", eventGridEvent.EventType);

                        var eventModel = eventGridEvent.Data.ToObjectFromJson<string>();
                        _logger.LogInformation("{@Message} User data received:", eventModel);
                    }
                }
                return Ok("Success!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "exception in ProcessEventGridEvents ");
                return BadRequest(ex);
            }
        }
    }
}
