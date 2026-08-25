using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace GRYLibrary.Core.APIServer.CommonRoutes
{
    /// <summary>
    /// Adds the <see cref="CommonRoutesConvention"/> to the options of the controllers.
    /// </summary>
    /// <remarks>
    /// It is done through the options-pattern because the convention needs the configured
    /// <see cref="ICommonRoutesInformation"/>, which the application registers and which is therefore only resolvable
    /// once the container exists.
    /// </remarks>
    public sealed class ConfigureCommonRoutes : IConfigureOptions<MvcOptions>
    {
        private readonly IServiceProvider _ServiceProvider;

        public ConfigureCommonRoutes(IServiceProvider serviceProvider)
        {
            this._ServiceProvider = serviceProvider;
        }

        public void Configure(MvcOptions options)
        {
            ICommonRoutesInformation? commonRoutesInformation = this._ServiceProvider.GetService<ICommonRoutesInformation>();
            if (commonRoutesInformation != null)
            {
                options.Conventions.Add(new CommonRoutesConvention(commonRoutesInformation));
            }
        }
    }
}
