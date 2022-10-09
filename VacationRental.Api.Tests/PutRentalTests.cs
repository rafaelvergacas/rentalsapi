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
        public async Task GivenCompleteRequest_WhenPutCalendar_ThenAGetReturnsTheUpdatedRental()
        {
            var request = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 1
            };

            ResourceIdViewModel postResult;
            using (var postResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", request))
            {
                Assert.True(postResponse.IsSuccessStatusCode);
                postResult = await postResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            using (var getResponse = await _client.GetAsync($"/api/v1/rentals/{postResult.Id}"))
            {
                Assert.True(getResponse.IsSuccessStatusCode);

                var getResult = await getResponse.Content.ReadAsAsync<RentalViewModel>();
                Assert.Equal(request.Units, getResult.Units);
            }

            var putRentalsRequest = new RentalBindingModel
            {
                 Units = 2,
                 PreparationTimeInDays = 2
            };

            using (var putResponse = await _client.PutAsJsonAsync($"/api/v1/rentals/{1}", putRentalsRequest))
            {
                Assert.True(putResponse.IsSuccessStatusCode);
                var getPutResult = await putResponse.Content.ReadAsAsync<RentalViewModel>();

                Assert.Equal(postResult.Id, getPutResult.Id);
                Assert.Equal(putRentalsRequest.Units, getPutResult.Units);
                Assert.Equal(putRentalsRequest.PreparationTimeInDays, getPutResult.PreparationTimeInDays);
            }
        }
    }

}