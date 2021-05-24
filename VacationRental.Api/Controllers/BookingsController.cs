using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using VacationRental.Api.Data;
using VacationRental.Api.Models;
using VacationRental.Api.Services;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingsService _bookingsService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(ILogger<BookingsController> logger, IBookingsService bookings)
        {
            _logger = logger;
            _bookingsService = bookings;
        }

        [HttpGet]
        [Route("{bookingId:int}")]
        public IActionResult Get(int bookingId)
        {
            try
            {
                ServiceResponseViewModel bookingResponse = _bookingsService.GetBooking(bookingId);
                
                switch (bookingResponse.HttpCodeResponse)
                {
                    case HttpStatusCode.OK:
                        _logger.LogInformation($"{nameof(BookingsController)}_{nameof(Get)}: successfully executed.");
                        return Ok(bookingResponse.Response);
                    case HttpStatusCode.BadRequest:
                        _logger.LogInformation($"{nameof(BookingsController)}_{nameof(Get)}: has returned a Bad Request. Message: {bookingResponse.Response}");
                        return BadRequest(bookingResponse.Response);
                    case HttpStatusCode.NotFound:
                        _logger.LogInformation($"{nameof(BookingsController)}_{nameof(Get)}: has returned a Not Found. Message: {bookingResponse.Response}");
                        return NotFound(bookingResponse.Response);
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(BookingsController)}_{nameof(Get)}: has thrown an exception: {e.Message.ToString()}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public IActionResult Post(BookingBindingModel model)
        {
            try
            {
                ServiceResponseViewModel bookingResponse = _bookingsService.CreateBooking(model);

                switch (bookingResponse.HttpCodeResponse)
                {
                    case HttpStatusCode.Created:
                        _logger.LogInformation($"{nameof(BookingsController)}_{nameof(Post)}: successfully executed.");
                        return Created(nameof(Post), bookingResponse.Response);
                    case HttpStatusCode.BadRequest:
                        _logger.LogInformation($"{nameof(BookingsController)}_{nameof(Post)}: has returned a Bad Request. Message: {bookingResponse.Response}");
                        return BadRequest(bookingResponse.Response);
                    case HttpStatusCode.Conflict:
                        _logger.LogWarning($"{nameof(BookingsController)}_{nameof(Post)}: has returned a Conflict. Message: {bookingResponse.Response}");
                        return Conflict(bookingResponse.Response);
                    case HttpStatusCode.NotFound:
                        _logger.LogInformation($"{nameof(BookingsController)}_{nameof(Post)}: has returned a Not Found. Message: {bookingResponse.Response}");
                        return NotFound(bookingResponse.Response);
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(BookingsController)}_{nameof(Post)}: has thrown an exception: {e.Message.ToString()}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            
        }
    }
}
