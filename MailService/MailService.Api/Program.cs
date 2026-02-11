using MailService.Application.Abstractions.Mail;
using MailService.Application.Settings.Email;
using MailService.Infrastructure.MessageEvents;
using MailService.Infrastructure.MessageEvents.Consumers;
using MailService.Infrastructure.MessageEvents.Settings;
using MailService.Infrastructure.Services.Email;
using MassTransit;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(nameof(RabbitMqSettings)));
builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();
    busConfigurator.AddConsumer<MailSendingEventConsumer>();
    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        var brokerSettings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
        configurator.Host(new Uri(brokerSettings.Host), h =>
        {
            h.Username(brokerSettings.Username);
            h.Password(brokerSettings.Password);
        });
        configurator.ConfigureEndpoints(context);
    });    
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));
builder.Services.AddScoped<IMailService, EmailService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();