using System;
using System.Text;

namespace GRYLibrary.Core.Misc.Healthcheck
{
    public sealed class Healthcheck : IDisposable
    {
        public string File { get; }
        public bool AddTimestamp { get; set; } = true;
        public Encoding Encoding { get; set; } = new UTF8Encoding(false);
        public Healthcheck(string file)
        {
            this.File = file;
        }
        public void SetState(HealthcheckValue value, string message = "")
        {
            Utilities.EnsureFileExists(this.File);
            string text;
            if (this.AddTimestamp)
            {
                text = Utilities.DateTimeToISO8601String(Utilities.GetNow()) + ": ";
            }
            else
            {
                text = string.Empty;
            }
            text = text + Enum.GetName(value);
            if (!string.IsNullOrWhiteSpace(message))
            {
                text = $"{text} ({message})";
            }
            Utilities.AppendLineToFile(this.File, text, this.Encoding);
        }
        public void Dispose()
        {
            this.SetState(HealthcheckValue.NotRunning, "Disposed");
        }
    }
}