using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Threading.Tasks;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.APIServer.Binder
{
    public class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            return GUtilities.GenericModelBinder(value => decimal.Parse(value, CultureInfo.InvariantCulture), typeof(decimal).Name, true)(bindingContext);
        }
    }
}
