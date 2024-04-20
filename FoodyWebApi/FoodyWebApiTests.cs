using FoodyWebApi.Models;
using NuGet.Frameworks;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace FoodyWebApi
{
    public class FoodyWebApiTests
    {
        private RestClient client;

        private string url = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";
        private string userName = "tester123";
        private string password = "test123";

        private static string foodId;

        [OneTimeSetUp]
        public void Setup()
        {
            var jwtToken = GetToken(userName, password);

            var options = new RestClientOptions(url)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);           
        }

        public string GetToken(string userName, string password)
        {
            var client = new RestClient(url);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            var credentials = new
            {
                userName = userName,
                password = password
            };

            request.AddJsonBody(credentials);

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = JsonSerializer.Deserialize<AuthResponse>(response.Content);
                var token = responseJson.AccessToken;
                

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Access Token is null or empty");
                }

                return token;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected response type {response.StatusCode} with data {response.Content}");
            }

        }

        [Test, Order(1)]
        public void Post_CreateFood_ShouldCreateFood_WhenGivenValidData()
        {
            // Arrange
            var food = new Food
            {
                Name = "Some name",
                Description = "Some Description"
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            // Act
            var response = client.Execute(request); 
            var responseJson = JsonSerializer.Deserialize<ApiResponse>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(responseJson, Is.Not.Null);
            Assert.That(responseJson.FoodId, Is.Not.Null);

            foodId = responseJson.FoodId;
        }

        [Test, Order(2)]
        public void Patch_EditExistingFood_ShouldEditFood_WhenGivenValidData()
        {
            // Arrange
            var request = new RestRequest($"/api/Food/Edit/{foodId}", Method.Patch);

            var body = new[]
            {
                new
                {
                path = "/name",
                op = "replace",
                value = "Name Updated"
                }
            };

            request.AddJsonBody(body);
            string successMessage = "Successfully edited";

            // Act
            var response = client.Execute(request);
            var responseJson = JsonSerializer.Deserialize<ApiResponse>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseJson.Message, Is.EqualTo(successMessage));
        }

        [Test, Order(3)]
        public void Get_GetAllFoods_ShouldReturnAllAvailableFoods()
        {
            // Arrange
            var request = new RestRequest("/api/Food/All", Method.Get);

            // Act
            var response = client.Execute(request);
            var responseJson = JsonSerializer.Deserialize<List<Food>>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseJson.Count(), Is.AtLeast(1));
        }

        [Test, Order(4)]
        public void Delete_DeleteEditedFood_ShouldDeleteEditedFood()
        {
            // Arrange
            string deletionMessage = "Deleted successfully!";

            var request = new RestRequest($"/api/Food/Delete/{foodId}", Method.Delete);

            // Act
            var response = client.Execute(request);
            var responseJson = JsonSerializer.Deserialize<ApiResponse>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseJson.Message, Is.EqualTo(deletionMessage));

        }
        [Test, Order(5)]
        public void Post_CreateFood_ShouldReturn_BadRequest_WhenGivenInvalidData()
        {
            // Arrange
            var food = new Food
            {
                Name = "Some name"
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            // Act
            var response = client.Execute(request);
            var responseJson = JsonSerializer.Deserialize<ApiResponse>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void Patch_EditFood_ShouldReturn_NotFound_WhenGivenInvalidFoodId()
        {
            // Arrange

            var body = new[]
            {
                new
                {
                path = "/name",
                op = "replace",
                value = "Name Updated"
                }
            };
            var request = new RestRequest($"/api/Food/Edit/invalid", Method.Patch);           
            
            request.AddJsonBody(body);

            string errorMessage = "No food revues...";

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain(errorMessage));
        }

        [Test, Order(7)]
        public void Delete_DeleteFood_ShouldReturnStatusCode_BadRequest_WhenGivenInvalidFoodId()
        {
            // Arrange
            string deletionMessage = "Unable to delete this food revue!";

            var request = new RestRequest($"/api/Food/Delete/invalidId", Method.Delete);

            // Act
            var response = client.Execute(request);
            var responseJson = JsonSerializer.Deserialize<ApiResponse>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responseJson.Message, Is.EqualTo(deletionMessage));
        }

        [Test, Order(8)]
        public void Patch2_EditFood_ShouldReturn_NotFound_WhenGivenInvalidFoodId()
        {
            // Arrange

            var body = new[]
            {
                new
                {
                path = "/name",
                op = "replace",
                value = "Name Updated"
                }
            };
            var request = new RestRequest($"/api/Food/Edit/invalid", Method.Patch);

            request.AddJsonBody(body);

            string errorMessage = "No food revues...";

            // Act
            var response = client.Execute(request);
            var responseJson = JsonSerializer.Deserialize<ApiResponse>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(responseJson.Message, Is.EqualTo(errorMessage));
        }

    }
}