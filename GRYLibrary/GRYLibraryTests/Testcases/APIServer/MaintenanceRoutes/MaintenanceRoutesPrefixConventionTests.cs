using GRYLibrary.Core.APIServer.MaintenanceRoutes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace GRYLibrary.Tests.Testcases.APIServer.MaintenanceRoutes
{
    [TestClass]
    public class MaintenanceRoutesPrefixConventionTests
    {
        private const string MaintenanceRoute = "/API/Other/Maintenance";

        private static ControllerModel CreateControllerModel(Type controllerType)
        {
            ControllerModel controller = new ControllerModel(controllerType.GetTypeInfo(), Array.Empty<object>());
            controller.Selectors.Add(new SelectorModel() { AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(MaintenanceRoute)) });
            return controller;
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ApplyPrependsPrefixToMaintenanceRoutes()
        {
            //arrange
            ControllerModel controller = CreateControllerModel(typeof(MaintenanceRoutesController));
            MaintenanceRoutesPrefixConvention convention = new MaintenanceRoutesPrefixConvention("/MyApp");

            //act
            convention.Apply(controller);

            //assert
            Assert.AreEqual("/MyApp/API/Other/Maintenance", controller.Selectors[0].AttributeRouteModel!.Template);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ApplyNormalizesSlashesOfPrefix()
        {
            //arrange
            ControllerModel controller = CreateControllerModel(typeof(MaintenanceRoutesController));
            MaintenanceRoutesPrefixConvention convention = new MaintenanceRoutesPrefixConvention("MyApp/");

            //act
            convention.Apply(controller);

            //assert
            Assert.AreEqual("/MyApp/API/Other/Maintenance", controller.Selectors[0].AttributeRouteModel!.Template);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ApplyKeepsMaintenanceRoutesUnchangedForEmptyPrefix()
        {
            //arrange
            ControllerModel controller = CreateControllerModel(typeof(MaintenanceRoutesController));
            MaintenanceRoutesPrefixConvention convention = new MaintenanceRoutesPrefixConvention(string.Empty);

            //act
            convention.Apply(controller);

            //assert
            Assert.AreEqual(MaintenanceRoute, controller.Selectors[0].AttributeRouteModel!.Template);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ApplyKeepsRoutesOfOtherControllersUnchanged()
        {
            //arrange
            ControllerModel controller = CreateControllerModel(typeof(ControllerBase));
            MaintenanceRoutesPrefixConvention convention = new MaintenanceRoutesPrefixConvention("/MyApp");

            //act
            convention.Apply(controller);

            //assert
            Assert.AreEqual(MaintenanceRoute, controller.Selectors[0].AttributeRouteModel!.Template);
        }
    }
}
