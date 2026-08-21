using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GRYLibrary.Core.APIServer.CommonRoutes
{
    /// <summary>
    /// Keeps the routes of the <see cref="CommonRoutesController"/> whose link is not configured from being hosted.
    /// </summary>
    /// <remarks>
    /// A link which is null means that the application has no such information. The route is therefore removed instead
    /// of being hosted and answering with a redirect to nothing: an application which does not state a contact should
    /// answer "this route does not exist" and not "this route exists but leads nowhere".
    /// </remarks>
    public sealed class CommonRoutesConvention : IActionModelConvention
    {
        private readonly ICommonRoutesInformation _CommonRoutesInformation;

        public CommonRoutesConvention(ICommonRoutesInformation commonRoutesInformation)
        {
            this._CommonRoutesInformation = commonRoutesInformation;
        }

        public void Apply(ActionModel action)
        {
            if (!typeof(CommonRoutesController).IsAssignableFrom(action.Controller.ControllerType))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(this.GetLinkOf(action.ActionName)))
            {
                // Removing the selectors removes the routes of the action, which is what makes it unreachable. The
                // method itself stays; there is no other way to remove an action of a controller.
                action.Selectors.Clear();
            }
        }

        /// <summary>
        /// The link which belongs to the given action, or null if the action is not one of the three which redirect to
        /// a link.
        /// </summary>
        private string? GetLinkOf(string actionName)
        {
            if (actionName == nameof(CommonRoutesController.TermsOfService))
            {
                return this._CommonRoutesInformation.TermsOfServiceLink;
            }
            if (actionName == nameof(CommonRoutesController.Contact))
            {
                return this._CommonRoutesInformation.ContactLink;
            }
            if (actionName == nameof(CommonRoutesController.License))
            {
                return this._CommonRoutesInformation.LicenseLink;
            }
            // Every other action of the controller stays as it is, so this convention can not remove something which
            // it does not know.
            return "not a link-action";
        }
    }
}
