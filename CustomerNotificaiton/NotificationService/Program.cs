﻿using EmailService;
using GreenPipes;
using MassTransit;
using MassTransit.Hosting;
using Messaging.InterfacesConstants.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Consumers;
using NotificationService.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace NotificationService
{
    public class Program
    {
        public static async Task  Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile($"appsettings.json", optional: false);
                    configHost.AddEnvironmentVariables();
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json");
                })
                 .ConfigureServices((hostContext, services) =>
                 {

                     var emailConfig = hostContext.Configuration
                     .GetSection("EmailConfiguration")
                     .Get<EmailConfig>();
                     services.AddSingleton(emailConfig);
                     services.AddScoped<IEmailSender,EmailSender>();
                     services.AddMassTransit(
                         c =>
                         {
                             c.AddConsumer<OrderProcessedEventConsumer>();
                         }
                         );
                     services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                     {
                         cfg.Host("localhost", "/", h => { });
                         cfg.ReceiveEndpoint(RabbitMqMassTransitConstants.NotificationServiceQueue, e =>
                         {
                             e.PrefetchCount = 16;
                             e.UseMessageRetry(x => x.Interval(2, TimeSpan.FromSeconds(10)));
                             e.Consumer<OrderProcessedEventConsumer>(provider);
                         }
                         );
                         cfg.ConfigureEndpoints(provider);
                     }));
                     services.AddSingleton<IHostedService, BusService>();
                 });
            return builder;
        }
    }
}
