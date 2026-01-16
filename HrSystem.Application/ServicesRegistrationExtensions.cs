using FluentValidation;
using HrSystem.Application.Common.Behaviors;
using HrSystem.Application.Common.Mappings;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using HrSystem.Infrustructure;

namespace HrSystem.Application;

public static class ServicesRegistrationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        //services.AddValidatorsFromAssembly(typeof(Result<>).Assembly);
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        MappingConfig.Register(TypeAdapterConfig.GlobalSettings);
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        services.AddScoped<IMapper, ServiceMapper>();


        return services;
    }
}