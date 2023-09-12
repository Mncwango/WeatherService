using Microsoft.VisualStudio.TestTools.UnitTesting;
using Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf.Contracts;
using Domain;
using Microsoft.Extensions.Configuration;
using Moq;
using DinkToPdf;
using RestSharp;
using RestSharp.Serializers;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using Infrastructure.Exceptions;

namespace Application.Tests
{
    [TestClass]
    public class WeatherServiceTests
    {
        private WeatherService weatherService;
        private Mock<IConverter> converterMock;
        private Mock<IConfiguration> configurationMock;
        private Mock<IHttpClientFactory> httpClientFactoryMock;
        private readonly string baseUrl = "http://api.weatherstack.com";
        private readonly string apiKey = "3d5e6dfb91846b825c71ed06de33a69c";

        [TestInitialize]
        public void Initialize()
        {
            converterMock = new Mock<IConverter>();
            configurationMock = new Mock<IConfiguration>();
            httpClientFactoryMock = new Mock<IHttpClientFactory>();

            weatherService = new WeatherService(converterMock.Object, configurationMock.Object);
        }

        [TestMethod]
        public void GetWeatherData_ReturnsValidApiResponse()
        {
            // Arrange
            string cityName = "nqutu";
            var expectedApiResponse = new WeatherDataApiResponse
            {
                Location = new Location()
                {
                    Name=cityName,
                }
            };

            var httpClient = new HttpClient(new MockHttpMessageHandler(JsonConvert.SerializeObject(expectedApiResponse), HttpStatusCode.OK));

            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            configurationMock.Setup(cfg => cfg["weatherService:tokenKey"]).Returns(apiKey);
            configurationMock.Setup(cfg => cfg["weatherService:baseUrl"]).Returns(baseUrl);

            // Act
            var result = weatherService.GetWeatherData(cityName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedApiResponse.Location.Name.ToLower(), result.Location.Name.ToLower());
        }

        [TestMethod]
        [ExpectedException(typeof(WeatherServiceException), "Weather response data is null")]
        public void GetWeatherData_HandlesErrorResponse()
        {
            string cityName = "nqutu";
            var httpClient = new HttpClient(new MockHttpMessageHandler("Error message", HttpStatusCode.BadRequest));
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            configurationMock.Setup(cfg => cfg["weatherService:tokenKey"]).Returns("");
            configurationMock.Setup(cfg => cfg["weatherService:baseUrl"]).Returns(baseUrl);
            var result = weatherService.GetWeatherData(cityName);
        }

        [TestMethod]
        public void GeneratePdfReport_CreatesPdfFile()
        {
            var weatherData = new WeatherDataApiResponse
            {
                Request = new Request
                {
                    Type = "City",
                    Query = "New York, United States of America",
                    Language = "en",
                    Unit = "m"
                },
                Location = new Location
                {
                    Name = "New York",
                    Country = "United States of America",
                    Region = "New York",
                    Lat = "40.714",
                    Lon = "-74.006",
                    Timezone_id = "America/New_York",
                    Localtime = "2023-09-11 14:43",
                    Localtime_epoch = 1694443380,
                    Utc_offset = "-4.0"
                },
                Current = new Current
                {
                    Observation_time = "06:43 PM",
                    Temperature = 27,
                    Weather_code = 113,
                    Weather_descriptions = new string[]{"Sunny"},
                    Wind_speed = 4,
                    Wind_degree = 170,
                    Wind_dir = "S",
                    Pressure = 1016,
                    Precip = 0,
                    Humidity = 65,
                    Cloudcover = 0,
                    Feelslike = 30,
                    Uv_index = 6,
                    Visibility = 16,
                    Is_day = "yes"
                }
            };

            var pdfBytes = new byte[] { 1, 2, 3 };
            converterMock.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Returns(pdfBytes);
            weatherService.GeneratePdfReport(weatherData);
            string expectedFilePath = $"{weatherData.Location.Name}_Weather_Report.pdf";
            Assert.IsTrue(File.Exists(expectedFilePath));
            File.Delete(expectedFilePath);
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly string content;
            private readonly HttpStatusCode statusCode;

            public MockHttpMessageHandler(string content, HttpStatusCode statusCode)
            {
                this.content = content;
                this.statusCode = statusCode;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(statusCode);
                response.Content = new StringContent(content);
                return await Task.FromResult(response);
            }
        }
    }
}