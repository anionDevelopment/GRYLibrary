using GRYLibrary.Core.Misc.CustomDisposables;

namespace GRYLibrary.Core.Logging.GRYLogger
{
    public class GRYLogSubNamespaceProvider : CustomDisposable
    {
        private readonly GRYLog _LogObject;
        private readonly string _SubNamespace;
        private readonly string _OriginalNamespace;

        public GRYLogSubNamespaceProvider(GRYLog logObject, string subnamespace)
        {
            this._LogObject = logObject;
            subnamespace = subnamespace.Trim();
            this._SubNamespace = subnamespace;
            this._OriginalNamespace = this._LogObject.Configuration.Name;
            if (!string.IsNullOrEmpty(subnamespace))
            {
                string prefix;
                if (string.IsNullOrEmpty(this._LogObject.Configuration.Name))
                {
                    prefix = string.Empty;
                }
                else
                {
                    prefix = $"{this._LogObject.Configuration.Name}.";
                }
                this._LogObject.Configuration.Name = $"{prefix}{this._SubNamespace}";
            }
            this.DisposeAction = () => this._LogObject.Configuration.Name = this._OriginalNamespace;
        }
    }
}