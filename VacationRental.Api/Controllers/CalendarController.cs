using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VacationRental.Api.Models;
using VacationRental.Api.Services;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/calendar")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;
        private readonly ILogger<CalendarController> _logger;

        public CalendarController(ILogger<CalendarController> logger, ICalendarService calendarService)
        {
            _logger = logger;
            _calendarService = calendarService;
        }

        [HttpGet]
        public IActionResult Get(int rentalId, DateTime start, int nights)
        {
            try
            {
                ServiceResponseViewModel serviceResponse = _calendarService.GetCalendar(rentalId, start, nights);
                
                switch (serviceResponse.HttpCodeResponse)
                {
                    case HttpStatusCode.OK:
                        _logger.LogInformation($"{nameof(CalendarController)}_{nameof(Get)}: successfully executed.");
                        return Ok(serviceResponse.Response);
                    case HttpStatusCode.BadRequest:
                        _logger.LogInformation($"{nameof(CalendarController)}_{nameof(Get)}: has returned a Bad Request. Message: {serviceResponse.Response}");
                        return BadRequest(serviceResponse.Response);
                    case HttpStatusCode.NotFound:
                        _logger.LogInformation($"{nameof(CalendarController)}_{nameof(Get)}: has returned a Not Found. Message: {serviceResponse.Response}");
                        return NotFound(serviceResponse.Response);
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(CalendarController)}_{nameof(Get)}: has thrown an exception: {e.Message.ToString()}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
