using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VacationRental.Api.Models;
using Xunit;

namespace VacationRental.Api.Tests
{
    [Collection("Integration")]
    public class PostBookingTests
    {
        private readonly HttpClient _client;

        public PostBookingTests(IntegrationFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task GivenCompleteRequest_WhenPostBooking_ThenAGetReturnsTheCreatedBooking()
        {
            int yearInTheFuture = DateTime.Now.AddYears(10).Year;
            
            var postRentalRequest = new RentalBindingModel
            {
                Units = 4
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                Assert.True(postRentalResponse.StatusCode == HttpStatusCode.Created);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            var postBookingRequest = new BookingBindingModel
            {
                 RentalId = postRentalResult.Id,
                 Nights = 3,
                 Start = new DateTime(yearInTheFuture, 01, 01)
            };

            ResourceIdViewModel postBookingResult;
            using (var postBookingResponse = await _client.PostAsJsonAsync($"/api/v1/bookings", postBookingRequest))
            {
                Assert.True(postBookingResponse.IsSuccessStatusCode);
                Assert.True(postBookingResponse.StatusCode == HttpStatusCode.Created);
                postBookingResult = await postBookingResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            using (var getBookingResponse = await _client.GetAsync($"/api/v1/bookings/{postBookingResult.Id}"))
            {
                Assert.True(getBookingResponse.IsSuccessStatusCode);
                Assert.True(getBookingResponse.StatusCode == HttpStatusCode.OK);
                
                var getBookingResult = await getBookingResponse.Content.ReadAsAsync<BookingViewModel>();
                Assert.Equal(postBookingRequest.RentalId, getBookingResult.RentalId);
                Assert.Equal(postBookingRequest.Nights, getBookingResult.Nights);
                Assert.Equal(postBookingRequest.Start, getBookingResult.Start);
            }
        }

        [Fact]
        public async Task GivenCompleteRequest_WhenPostBooking_ThenAPostReturnsAConflictWhenThereIsOverbooking()
        {
            int yearInTheFuture = DateTime.Now.AddYears(10).Year;
            
            var postRentalRequest = new RentalBindingModel
            {
                Units = 1
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                Assert.True(postRentalResponse.StatusCode == HttpStatusCode.Created);
                
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            var postBooking1Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 3,
                Start = new DateTime(yearInTheFuture, 01, 01)
            };

            using (var postBooking1Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking1Request))
            {
                Assert.True(postBooking1Response.IsSuccessStatusCode);
                Assert.True(postBooking1Response.StatusCode == HttpStatusCode.Created);
            }

            var postBooking2Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 1,
                Start = new DateTime(yearInTheFuture, 01, 02)
            };

            // The behaviour of this endpoint has changed to a handled response with an HttpStatusCode.Conflict response code
            // await Assert.ThrowsAsync<ApplicationException>(async () =>
            // {
            //     using (var postBooking2Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking2Request))
            //     {
            //     }
            // });

            using (var postBooking2Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking2Request))
            {
                Assert.False(postBooking2Response.IsSuccessStatusCode);
                Assert.True(HttpStatusCode.Conflict == postBooking2Response.StatusCode);
                
                string result = await postBooking2Response.Content.ReadAsStringAsync();
                Assert.Contains("The rental is fully booked for at least one day of the given booking period", result);
            }
        }
    }
}
