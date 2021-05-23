using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VacationRental.Api.Models;
using VacationRental.Api.Services;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/rentals")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalsService _rentalsService;
        private readonly ILogger<RentalsController> _logger;

        public RentalsController(ILogger<RentalsController> logger, IRentalsService rentalsService)
        {
            _logger = logger;
            _rentalsService = rentalsService;
        }

        [HttpGet]
        [Route("{rentalId:int}")]
        public IActionResult Get(int rentalId)
        {
            try
            {
                ServiceResponseViewModel rentalResponse = _rentalsService.GetRental(rentalId);
                
                switch (rentalResponse.HttpCodeResponse)
                {
                    case HttpStatusCode.OK:
                        _logger.LogInformation($"{nameof(RentalsController)}_{nameof(Get)}: successfully executed.");
                        return Ok(rentalResponse.Response);
                    case HttpStatusCode.BadRequest:
                        _logger.LogInformation($"{nameof(RentalsController)}_{nameof(Get)}: has returned a Bad Request. Message: {rentalResponse.Response}");
                        return BadRequest(rentalResponse.Response);
                    case HttpStatusCode.NotFound:
                        _logger.LogInformation($"{nameof(RentalsController)}_{nameof(Get)}: has returned a Not Found. Message: {rentalResponse.Response}");
                        return NotFound(rentalResponse.Response);
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(RentalsController)}_{nameof(Get)}: has thrown an exception: {e.Message.ToString()}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
        
        [HttpPost]
        public IActionResult Post(RentalBindingModel model)
        {
            try
            {
                ServiceResponseViewModel rentalResponse = _rentalsService.CreateRental(model);

                switch (rentalResponse.HttpCodeResponse)
                {
                    case HttpStatusCode.Created:
                        _logger.LogInformation($"{nameof(RentalsController)}_{nameof(Post)}: successfully executed.");
                        return Created(nameof(Post), rentalResponse.Response);
                    case HttpStatusCode.BadRequest:
                        _logger.LogInformation($"{nameof(RentalsController)}_{nameof(Post)}: has returned a Bad Request. Message: {rentalResponse.Response}");
                        return BadRequest(rentalResponse.Response);
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(RentalsController)}_{nameof(Post)}: has thrown an exception: {e.Message.ToString()}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
        
        [HttpPut]
        [Route("{rentalId:int}")]
        public IActionResult Put(RentalBindingModel model, int rentalId)
        {
            try
            {
                ServiceResponseViewModel rentalResponse = _rentalsService.ModifyRental(rentalId, model);

                switch (rentalResponse.HttpCodeResponse)
                {
                    case HttpStatusCode.NoContent:
                        _logger.LogInformation($"{nameof(RentalsController)}_{nameof(Put)}: successfully executed.");
                        return Ok(rentalResponse.Response);
                    case HttpStatusCode.BadRequest:
                        _logger.LogInformation($"{nameof(RentalsController)}_{nameof(Put)}: has returned a Bad Request. Message: {rentalResponse.Response}");
                        return BadRequest(rentalResponse.Response);
                    case HttpStatusCode.Conflict:
                        _logger.LogWarning($"{nameof(RentalsController)}_{nameof(Put)}: has returned a Conflict. Message: {rentalResponse.Response}");
                        return Conflict(rentalResponse.Response);
                    case HttpStatusCode.NotFound:
                        _logger.LogInformation($"{nameof(RentalsController)}_{nameof(Put)}: has returned a Not Found. Message: {rentalResponse.Response}");
                        return NotFound(rentalResponse.Response);
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(RentalsController)}_{nameof(Put)}: has thrown an exception: {e.Message.ToString()}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
