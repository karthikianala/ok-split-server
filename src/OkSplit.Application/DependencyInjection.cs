using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OkSplit.Application.Interfaces;
using OkSplit.Application.Services;

namespace OkSplit.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
