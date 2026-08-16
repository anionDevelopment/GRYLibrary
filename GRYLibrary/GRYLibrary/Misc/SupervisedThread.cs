using GRYLibrary.Core.Logging.GRYLogger;
using System;

namespace GRYLibrary.Core.Misc
{
    public class SupervisedThread
    {
        public GRYLog LogObject { get; set; }
        public static SupervisedThread CreateByLogFile(Action action, string logFile, string name = "", string informationAboutInvoker = "")
        {
            return CreateByGRYLog(action, GRYLog.Create(logFile), name, informationAboutInvoker);
        }

        public static SupervisedThread CreateByGRYLog(Action action, GRYLog log = null, string name = "", string informationAboutInvoker = "")
        {
            return new SupervisedThread(action, log, name, informationAboutInvoker);
        }

        public static SupervisedThread Create(Action action, string name = "", string informationAboutInvoker = "")
        {
            return CreateByLogFile(action, null, name, informationAboutInvoker);
        }

        private SupervisedThread(Action action, GRYLog log, string name, string informationAboutInvoker)
        {
            this.InformationAboutInvoker = informationAboutInvoker;
            this.Action = action;
            this.Id = Guid.NewGuid();
            this.Name = $"{nameof(SupervisedThread)} {this.Id} " + (string.IsNullOrEmpty(name) ? string.Empty : $"({name})");
            this.LogObject = log;
        }
        public bool LogOverhead { get; set; } = false;
        public string Name { get; set; }
        public Guid Id { get; }
        public string InformationAboutInvoker { get; set; }
        public Action Action { get; }
        private bool _Running = false;
        private System.Threading.Thread _Thread = null;
        private void Execute()
        {
            this._Running = true;
            if (this.LogOverhead)
            {
                this.LogObject?.Log(string.Format("Start action with id {0} and name \"{1}\"", this.Id.ToString(), this.Name.ToString()));
            }
            try
            {
                this.Action();
            }
            catch (Exception exception)
            {
                this.LogObject?.Log(string.Format("Error occurred while executing action with id {0} and name \"{1}\"", this.Id.ToString(), this.Name.ToString()), exception);
            }
            finally
            {
                this._Running = false;
            }
            if (this.LogOverhead)
            {
                this.LogObject?.Log(string.Format("Finished action with id {0} and name \"{1}\" started", this.Id.ToString(), this.Name.ToString()));
            }
        }

        public void Start()
        {
            if (!this._Running)
            {
                this._Thread = new System.Threading.Thread(this.Execute)
                {
                    Name = this.Name
                };
                this._Thread.Start();
            }
        }
        public void Abort()
        {
            if (this._Running)
            {
                this._Thread.Interrupt();
            }
        }
    }
}