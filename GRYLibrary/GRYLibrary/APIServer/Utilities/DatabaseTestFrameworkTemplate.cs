using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.ExecutePrograms.WaitingStates;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.APIServer.Utilities
{
    public abstract class DatabaseTestFrameworkTemplate : IDisposable
    {
        public DbConnection Connection { get; private set; }
        public string ConnectionString { get; private set; }
        public bool IsConnected { get; private set; }
        private bool _Disposed = false;
        private readonly string _TestDatabaseFolder;
        private readonly IGenericDatabaseInteractor _GenericDatabaseInteractor;
        public abstract string GetDatabaseTypeName();
        private readonly string _TaskNameStart;
        private readonly string _TaskNameStop;
        private readonly IGRYLog _Log;
        private readonly string _ResetDatabaseScript;
        public DatabaseTestFrameworkTemplate(IDatabasePersistenceConfiguration configuration, string testDatabaseFolder, string repositoryFolder, string taskNameStart, string taskNameStop, string resetDatabaseScript, IGRYLog log, TimeSpan connectionTimeout, ISet<string> containerNamesToWaitToBeHealthy)
        {
            this._TaskNameStart = taskNameStart;
            this._TaskNameStop = taskNameStop;
            this._ResetDatabaseScript = resetDatabaseScript;
            this._Log = log;
            this._TestDatabaseFolder = testDatabaseFolder;
            this.IsConnected = false;
            bool containerStarted = false;
            try
            {
                string argument = this._TaskNameStart;
                string volumesFolder = Path.Combine(this._TestDatabaseFolder, "Volumes", "Data");
                using (ExternalProgramExecutor externalProgramExecutor = new ExternalProgramExecutor("task", argument, repositoryFolder))
                {
                    externalProgramExecutor.Run();
                    GUtilities.AssertCondition(externalProgramExecutor.ExitCode == 0, $"Error while starting test-database using command \"{externalProgramExecutor.CMD}\" due to exitcode {externalProgramExecutor.ExitCode}. StdOut: {string.Join(Environment.NewLine, externalProgramExecutor.AllStdOutLines)}; StdErr: {string.Join(Environment.NewLine, externalProgramExecutor.AllStdErrLines)}");
                    containerStarted = true;
                    // The health-state has to get polled until the timeout is reached: an exception which is thrown
                    // inside of the action of RunWithTimeout gets swallowed (no error-handler is passed), so a
                    // single assertion inside of the action would let an unhealthy container pass as healthy.
                    //
                    // Docker only reports a health-state for containers whose image or container-definition declares a
                    // health-check. For a container without one there is nothing to observe, so polling it can never
                    // succeed and would inevitably run into the timeout. Such a container therefore gets skipped here
                    // (with a warning) instead of blocking: the readiness of the database is guaranteed anyway by the
                    // connection-loop below, which is the authoritative check.
                    ISet<string> containersWithHealthCheck = new HashSet<string>();
                    foreach (string containerName in containerNamesToWaitToBeHealthy)
                    {
                        if (GUtilities.GetContainerHealthState(containerName) == ContainerHealthState.NoHealthCheckDefined)
                        {
                            log.Log($"The container \"{containerName}\" does not declare a health-check, so its health-state can not get awaited. Add a health-check to the definition of this container (for example in its docker-compose-file) to enable this check.", LogLevel.Warning);
                        }
                        else
                        {
                            containersWithHealthCheck.Add(containerName);
                        }
                    }
                    GUtilities.AssertCondition(GUtilities.RunWithTimeout(() =>
                    {
                        foreach (string containerNameToWaitToBeHealthy in containersWithHealthCheck)
                        {
                            while (!GUtilities.ContainerIsHealthy(containerNameToWaitToBeHealthy))
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(1));
                            }
                        }
                    }, connectionTimeout), $"Not all of the containers {{{string.Join(", ", containersWithHealthCheck)}}} did become healthy within the expected time.");
                }
                this._GenericDatabaseInteractor = DBUtilities.ToGenericDatabaseInteractor(configuration, log);
                Exception? lastException = null;
                if (!GUtilities.RunWithTimeout(() =>
                {
                    bool connected = false;
                    while (!connected)
                    {
                        (bool, Exception?) isAvailable = this._GenericDatabaseInteractor.IsAvailable();
                        connected = isAvailable.Item1;
                        lastException = isAvailable.Item2;
                        if (!connected)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                        }
                    }
                }, connectionTimeout))
                {
                    string message = $"{this.GetType().Name} was not able to connect to the test-database.";
                    if (lastException == null)
                    {
                        throw new NotReadyException(message);
                    }
                    else
                    {
                        throw new NotReadyException(message, lastException);
                    }
                }
            }
            catch
            {
                // Constructor failed mid-init. Make a best-effort attempt to roll back so the next
                // test does not inherit a half-started container / leftover network state.
                if (containerStarted)
                {
                    this.TryStopTestContainersBestEffort();
                }
                try { this._GenericDatabaseInteractor?.Dispose(); } catch { /* best-effort cleanup */ }
                this._Disposed = true;
                throw;
            }
        }

        private void TryStopTestContainersBestEffort()
        {
            try
            {
                using ExternalProgramExecutor stopExecutor = new ExternalProgramExecutor("task", this._TaskNameStop, this._TestDatabaseFolder);
                stopExecutor.Configuration.WaitingState = new RunSynchronously();
                stopExecutor.Run();
            }
            catch
            {
                // Best-effort: the database might never have actually started, the task may not exist, etc.
                // The next constructor attempt will surface a real error if state is still broken.
            }
        }


        public IGenericDatabaseInteractor GenericDatabaseInteractor()
        {
            return this._GenericDatabaseInteractor;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (this._Disposed)
            {
                return;
            }
            try
            {
                if (disposing)
                {
                    if (this.Connection != null)
                    {
                        if (this.IsConnected)
                        {
                            try { this.Connection.Close(); } catch { /* best-effort */ }
                        }
                        try { this.Connection.Dispose(); } catch { /* best-effort */ }
                    }
                    try
                    {
                        using ExternalProgramExecutor externalProgramExecutor = new ExternalProgramExecutor("task", this._TaskNameStop, this._TestDatabaseFolder);
                        externalProgramExecutor.Configuration.WaitingState = new RunSynchronously();
                        externalProgramExecutor.Run();
                        GUtilities.AssertCondition(externalProgramExecutor.ExitCode == 0, $"Error while stopping test-database using command \"{externalProgramExecutor.CMD}\" due to exitcode {externalProgramExecutor.ExitCode}. StdOut: {string.Join(Environment.NewLine, externalProgramExecutor.AllStdOutLines)}; StdErr: {string.Join(Environment.NewLine, externalProgramExecutor.AllStdErrLines)}");
                    }
                    catch
                    {
                        // Stopping the container failed (e.g. docker hang, task missing). Don't propagate
                        // out of Dispose — it would crash the test host and mask any real test failure.
                        // The next test's constructor will surface a leftover-container error if the state
                        // is actually broken.
                    }
                }
                try { this._GenericDatabaseInteractor?.Dispose(); } catch { /* best-effort */ }
            }
            finally
            {
                this._Disposed = true;
            }
        }

        public void ResetDatabase()
        {
            if (0 < this.GenericDatabaseInteractor().GetAllTableNames().ToList().Count)
            {
                DbConnection connection = this._GenericDatabaseInteractor.GetConnection();
                try
                {
                    using DbCommand cmd = connection.CreateCommand();
                    cmd.CommandText = this._ResetDatabaseScript;
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    throw;
                }
                // The reset script may drop the schema/database the connection is currently attached to
                // (MariaDB's reset-script does "DROP DATABASE ...; CREATE DATABASE ..."). The connection
                // itself stays open in that case - just without a selected database - so every further
                // query on it fails with "No database selected", and the connection's self-healing
                // background thread (GenericDatabaseInteractor.StartTryToConnectScheduler) never notices,
                // because the connection's ConnectionState is still Open. Closing it here makes that
                // background thread reconnect, which re-applies "Database=..." from the connection string.
                connection.Close();
                this._GenericDatabaseInteractor.WaitUntilAvailable(TimeSpan.FromSeconds(30));
            }
        }
        public void Dispose() // Implement IDisposable
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DatabaseTestFrameworkTemplate() // the finalizer
        {
            this.Dispose(false);
        }
    }
}
