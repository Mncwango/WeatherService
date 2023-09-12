using Application;
using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Infrastructure;

namespace Assesment
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var serviceProvider = ConfigureServices();
                Console.WriteLine("Enter a city name:");
                string cityName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(cityName))
                {
                    Console.WriteLine("City name cannot be empty.");
                    return;
                }

                var weatherService = serviceProvider.GetService<IWeatherService>();
                var weatherData = weatherService.GetWeatherData(cityName);
                if (weatherData != null)
                {
                    weatherService.GeneratePdfReport(weatherData);
                    Console.WriteLine($"PDF report generated for {cityName}");
                }
                else
                {
                    Console.WriteLine($"Failed to fetch weather data for {cityName}");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
            }
        }


        private static IServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfiguration config = builder.Build();
            services.AddSingleton<IConfiguration>(config);
            services.AddInfrastructure();
            services.AddApplication();
            return services.BuildServiceProvider();
        }
    }
}