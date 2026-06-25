using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SGC.AntonioAnte.Application.Common.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 1. Registra MediatR y todos los Handlers automáticamente buscando en este ensamblado
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                // 2. Registramos nuestro Pipeline de Validación
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // 3. Registra todos los validadores de FluentValidation automáticamente
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
