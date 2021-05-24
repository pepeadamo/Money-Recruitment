using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VacationRental.Api.Models;
using Xunit;

namespace VacationRental.Api.Tests
{
    [Collection("Integration")]
    public class PutRentalTests
    {
        private readonly HttpClient _client;
        
        public PutRentalTests(IntegrationFixture fixture)
        {
            _client = fixture.Client;
        }
        
        [Fact]
        public async Task GivenCompleteRequest_WhenGetRental_ThenAGetReturnsTheUpdatedRental()
        {
            var request = new RentalBindingModel
            {
                Units = 25
            };

            ResourceIdViewModel postResult;
            using (HttpResponseMessage postResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", request))
            {
                Assert.True(postResponse.IsSuccessStatusCode);
                Assert.True(postResponse.StatusCode == HttpStatusCode.Created);
                postResult = await postResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            request.PreparationTimeInDays = 100;
            request.Units = 100;
            ResourceIdViewModel putResult;
            using (HttpResponseMessage postResponse = await _client.PutAsJsonAsync($"/api/v1/rentals/{postResult.Id}", request))
            {
                Assert.True(postResponse.IsSuccessStatusCode);
                Assert.True(postResponse.StatusCode == HttpStatusCode.NoContent);
            }

            using (HttpResponseMessage getResponse = await _client.GetAsync($"/api/v1/rentals/{postResult.Id}"))
            {
                Assert.True(getResponse.IsSuccessStatusCode);
                Assert.True(getResponse.StatusCode == HttpStatusCode.OK);
                
                RentalViewModel getResult = await getResponse.Content.ReadAsAsync<RentalViewModel>();
                Assert.Equal(request.Units, getResult.Units);
                Assert.Equal(request.PreparationTimeInDays, getResult.PreparationTimeInDays);
            }
        }
        
        [Fact]
        public async Task GivenCompleteRequestAndBookingsSetUp_WhenIncrementThePreparationTimeInDaysAndUpdateRental_ThenAPutReturnsAConflictResponse()
        {
            int yearInTheFuture = DateTime.Now.AddYears(10).Year;
            
            // POST Rental
            var postRentalRequest = new RentalBindingModel
            {
                Units = 2,
                PreparationTimeInDays = 3
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            // POST Booking 1
            var postBooking1Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 5,
                Start = new DateTime(yearInTheFuture, 06, 10)
            };

            using (var postBooking1Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking1Request))
            {
                Assert.True(postBooking1Response.IsSuccessStatusCode);
            }

            // POST Booking 2
            var postBooking2Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 5,
                Start = new DateTime(yearInTheFuture, 06, 11)
            };
            
            using (var postBooking2Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking2Request))
            {
                Assert.True(postBooking2Response.IsSuccessStatusCode);
            }
            
            // POST Booking 3
            var postBooking3Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 5,
                Start = new DateTime(yearInTheFuture, 06, 18)
            };
            
            using (var postBooking3Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking3Request))
            {
                Assert.True(postBooking3Response.IsSuccessStatusCode);
            }
            
            // PUT Rental 
            var putRental = new RentalBindingModel
            {
                Units = 2,
                PreparationTimeInDays = 4
            };

            ResourceIdViewModel putResult;
            using (HttpResponseMessage putResponse = await _client.PutAsJsonAsync($"/api/v1/rentals/{postRentalResult.Id}", putRental))
            {
                Assert.False(putResponse.IsSuccessStatusCode);
                Assert.True(putResponse.StatusCode == HttpStatusCode.Conflict);
                
                string result = await putResponse.Content.ReadAsStringAsync();
                Assert.Contains("The preparation time value can not be modified due to conflicts with existing bookings", result);
            }
        }
        
        [Fact]
        public async Task GivenCompleteRequestAndBookingsSetUp_WhenReduceTheNumberOfUnitsAndUpdateRental_ThenAPutReturnsAConflictResponse()
        {
            int yearInTheFuture = DateTime.Now.AddYears(10).Year;
            
            // POST Rental
            var postRentalRequest = new RentalBindingModel
            {
                Units = 2,
                PreparationTimeInDays = 3
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            // POST Booking 1
            var postBooking1Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 5,
                Start = new DateTime(yearInTheFuture, 06, 10)
            };

            using (var postBooking1Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking1Request))
            {
                Assert.True(postBooking1Response.IsSuccessStatusCode);
            }

            // POST Booking 2
            var postBooking2Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 5,
                Start = new DateTime(yearInTheFuture, 06, 11)
            };
            
            using (var postBooking2Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking2Request))
            {
                Assert.True(postBooking2Response.IsSuccessStatusCode);
            }

            // PUT Rental 
            var putRental = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 3
            };

            ResourceIdViewModel putResult;
            using (HttpResponseMessage putResponse = await _client.PutAsJsonAsync($"/api/v1/rentals/{postRentalResult.Id}", putRental))
            {
                Assert.False(putResponse.IsSuccessStatusCode);
                Assert.True(putResponse.StatusCode == HttpStatusCode.Conflict);
                
                string result = await putResponse.Content.ReadAsStringAsync();
                Assert.Contains("The number of units can not be modified due to conflicts with existing bookings", result);
            }
        }
        
        [Fact]
        public async Task GivenCompleteRequestAndBookingsSetUp_WhenIncrementingTheNumberOfUnitsAndUpdateRental_ThenAGetReturnsTheUpdatedRental()
        {
            int yearInTheFuture = DateTime.Now.AddYears(10).Year;
            
            // POST Rental
            var postRentalRequest = new RentalBindingModel
            {
                Units = 2,
                PreparationTimeInDays = 3
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            // POST Booking 1
            var postBooking1Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 5,
                Start = new DateTime(yearInTheFuture, 06, 10)
            };

            using (var postBooking1Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking1Request))
            {
                Assert.True(postBooking1Response.IsSuccessStatusCode);
            }

            // POST Booking 2
            var postBooking2Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 5,
                Start = new DateTime(yearInTheFuture, 06, 11)
            };
            
            using (var postBooking2Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking2Request))
            {
                Assert.True(postBooking2Response.IsSuccessStatusCode);
            }

            // PUT Rental 
            var putRental = new RentalBindingModel
            {
                Units = 100,
                PreparationTimeInDays = 3
            };
            
            using (HttpResponseMessage putResponse = await _client.PutAsJsonAsync($"/api/v1/rentals/{postRentalResult.Id}", putRental))
            {
                Assert.True(putResponse.IsSuccessStatusCode);
                Assert.True(putResponse.StatusCode == HttpStatusCode.NoContent);
            }
            
            // GET Rental 
            using (HttpResponseMessage putResponse = await _client.GetAsync($"/api/v1/rentals/{postRentalResult.Id}"))
            {
                Assert.True(putResponse.IsSuccessStatusCode);
                RentalViewModel rentalUpdated = await putResponse.Content.ReadAsAsync<RentalViewModel>();
                
                Assert.True(rentalUpdated.Units == putRental.Units);
                Assert.True(rentalUpdated.PreparationTimeInDays == putRental.PreparationTimeInDays);
            }
        }
    }
}