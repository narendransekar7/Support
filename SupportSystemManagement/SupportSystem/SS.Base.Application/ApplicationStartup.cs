using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.Base.Application.Events;
namespace SS.Base.Application
{
    public static class ApplicationStartup
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(typeof(SS.Base.Application.AssemblyReference).Assembly);
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
           // services.AddTransient<AzureServiceBusQueueSender, AzureServiceBusQueueSender>();
            
            services.AddTransient<AzureServiceBusQueueSender>(provider =>
            {
                
                string connectionString = configuration["AzureServiceBus:ConnectionString"];
                string queueName = configuration["AzureServiceBus:QueueName"];
                return new AzureServiceBusQueueSender(connectionString, queueName);
            });
            return services;
        }

    }
}
