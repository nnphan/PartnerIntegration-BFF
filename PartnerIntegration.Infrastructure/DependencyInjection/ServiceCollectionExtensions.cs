using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PartnerIntegration.Application.Interfaces;
using PartnerIntegration.Application.Services;
using PartnerIntegration.Application.Validators;
using PartnerIntegration.Infrastructure.ExternalServices;
using PartnerIntegration.Infrastructure.Messaging;
using PartnerIntegration.Infrastructure.Resilience;


namespace PartnerIntegration.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering PartnerIntegration services, clients, and messaging components into the dependency injection container.
    /// This includes application services, partner verification client, and RabbitMQ message publisher.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPartnerIntegrationServices(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            //commnent 
            services.AddApplicationServices();
            services.AddPartnerVerificationClient(configuration);
            services.AddRabbitMqPublisher(configuration);


            return services;
        }

        private static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            //comment 
            services.AddScoped<ITransactionService, TransactionService>(); // why AddScoped
            services.AddValidatorsFromAssemblyContaining<TransactionRequestValidator>();

            return services;
        }

        private static IServiceCollection AddPartnerVerificationClient(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            //comment
            services.Configure<PartnerVerificationOptions>(
            configuration.GetSection(PartnerVerificationOptions.SectionName)); // get from appsetting

            services.AddScoped<IPartnerVerificationClient, PartnerVerificationClient>();

            services.AddHttpClient(PartnerVerificationClient.HttpClientName, (provider, client) =>
            {
                var options = configuration
                    .GetSection(PartnerVerificationOptions.SectionName)
                    .Get<PartnerVerificationOptions>() ?? new PartnerVerificationOptions();

                if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    client.BaseAddress = new Uri(options.BaseUrl);
                }
            })
            .AddPolicyHandler((provider, _) =>

                PartnerVerificationPolicies.GetRetryPolicy(
                    provider.GetRequiredService<ILoggerFactory>().CreateLogger("PartnerVerification.Retry")))
            .AddPolicyHandler((provider, _) =>
                PartnerVerificationPolicies.GetTimeoutPolicy());

            return services;
        }

        private static IServiceCollection AddRabbitMqPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

            services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
            services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

            return services;
        }
    }
}
