using GRYLibrary.Core.APIServer.CommonRoutes;
using GRYLibrary.Core.APIServer.MaintenanceRoutes;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Regulations.GDPR;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;

namespace GRYLibrary.Core.APIServer.Utilities
{
    internal class CustomControllerFeatureProvider<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> : ControllerFeatureProvider
        where PersistedApplicationSpecificConfiguration : new()
    {
        private readonly APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> _Configuration;
        private readonly IGRYLog _Log;
        public CustomControllerFeatureProvider(APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> apiServerConfiguration, IGRYLog log)
        {
            this._Configuration = apiServerConfiguration;
            this._Log = log;
        }
        protected override bool IsController(TypeInfo typeInfo)
        {
            bool result = this.IsControllerInternal(typeInfo);
            if (result)
            {
                this._Log.Log($"Treat {typeInfo.FullName} as controller.", LogLevel.Debug);
            }
            return result;
        }

        protected virtual bool IsControllerInternal(TypeInfo typeInfo)
        {
            Type type = typeInfo.AsType();
            if (type.IsAssignableTo(typeof(CommonRoutesController)))
            {
                if (this._Configuration.InitializationInformation.ApplicationConstants.CommonRoutesHostInformation is HostCommonRoutes hostCommonRoutes)
                {
                    bool result = type.Equals(hostCommonRoutes.ControllerType);
                    return result;
                }
                else
                {
                    return false;
                }
            }
            if (type.IsAssignableTo(typeof(MaintenanceRoutesController)))
            {
                if (this._Configuration.InitializationInformation.ApplicationConstants.HostMaintenanceInformation is HostMaintenanceRoutes hostMaintenanceRoutes)
                {
                    bool result = type.Equals(hostMaintenanceRoutes.ControllerType);
                    return result;
                }
                else
                {
                    return false;
                }
            }
            if (type.IsAssignableTo(typeof(GDPRController)))
            {
                Regulation? regulation = this._Configuration.InitializationInformation.ApplicationConstants.Regulations.FirstOrDefault(t => t is GDPRegulation);
                if (regulation == null)
                {
                    return false;
                }
                else
                {
                    if (regulation is GDPRegulation gdpregulation)
                    {
                        bool result = gdpregulation.ServiceProcessesPersonalData && gdpregulation.ServiceIsSubjectOfGDPR;
                        if (result)
                        {
                            //TODO check if IGDPRService is injectable
                        }
                        return result;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return base.IsController(typeInfo);
        }
    }
}
