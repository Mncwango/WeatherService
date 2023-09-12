using DinkToPdf;
using DinkToPdf.Contracts;
using Domain;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public class WeatherService : IWeatherService
    {
        private string weatherstackApiKey;
        private readonly IConverter converter;
        private readonly IConfiguration configuration;

        public WeatherService(IConverter converter, IConfiguration configuration)
        {
            this.converter = converter;
            this.configuration = configuration;
        }

        public WeatherDataApiResponse GetWeatherData(string cityName)
        {
            try
            {
                weatherstackApiKey = configuration["weatherService:tokenKey"];
                using (var client = new HttpClient())
                {
                    var url = configuration["weatherService:baseUrl"] + $"/current?access_key={weatherstackApiKey}&query={cityName}";
                    client.Timeout = TimeSpan.FromMilliseconds(15000);
                    client.DefaultRequestHeaders.ConnectionClose = true;
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("access_key", weatherstackApiKey);
                    var response = client.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {

                        string responseText = response.Content.ReadAsStringAsync().Result;
                        var weatherApiResponse = JsonConvert.DeserializeObject<WeatherDataApiResponse>(responseText);

                        if (weatherApiResponse.Location == null)
                        {
                          throw new WeatherServiceException($"Weather response data is null");
                        }


                        return weatherApiResponse;
                    }
                    else
                    {
                        throw new WeatherServiceException($"Failed to fetch weather data for {cityName}: {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new WeatherServiceException($"Failed to retrieve weather data for {cityName}, {ex.Message}");
            }

        }

        public void GeneratePdfReport(WeatherDataApiResponse weatherData)
        {
            try
            {
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4
            },
                    Objects = {
                new ObjectSettings
                {
                    PagesCount = true,

                    HtmlContent = @"
                                <head>
                                <style>
                                table {
                                  font-family: arial, sans-serif;
                                  border-collapse: collapse;
                                  width: 100%;
                                }

                                td, th {
                                  border: 1px solid #dddddd;
                                  text-align: left;
                                  padding: 8px;
                                }

                                tr:nth-child(even) 
                                                    {
                                  background-color: #dddddd;
                                }
                                </style>
                                </head>
                                <body>"+
                                $@"
                                  <h1>Weather Report for {weatherData.Location.Name}</h1>
                                  <table>
                                      <tr>
                                        <th>Temperature</th>
                                        <th>Description</th>
                                        <th>Wind Speed</th>
                                        <th>Precipitation</th>
                                        <th>Humidity</th>
                                        <th>Cloud Cover</th>
                                        <th>Feels like</th>
                                      </tr>
                                      <tr>
                                            <td>{weatherData.Current.Temperature}°C</td>
                                            <td> {string.Join(",",weatherData.Current.Weather_descriptions)}</p>
                                            <td>{weatherData.Current.Wind_speed} km/hr</td>
                                            <td>{weatherData.Current.Precip}%</td>
                                            <td>{weatherData.Current.Humidity}%</td>
                                            <td>{weatherData.Current.Cloudcover}%</td>
                                            <td>{weatherData.Current.Feelslike}°C</td>
                                      </tr>
                                        </table>
                    </body>",
                    WebSettings = { DefaultEncoding = "utf-8" },
                    FooterSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                }
            }
                };

                var pdfBytes = converter.Convert(doc);
                File.WriteAllBytes($"{weatherData.Location.Name}_Weather_Report.pdf", pdfBytes);
            }
            catch (Exception ex)
            {

                throw new GeneratePdfReportException($"Could not generate the pdf report for {weatherData.Location.Name} because of \n\n {ex.Message} ");
            }
        }
    }
}
