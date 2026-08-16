using GRYLibrary.Core.Misc.Migration;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.Database
{
    public interface IProjectSpecificDatabaseInteractor
    {
        public IGenericDatabaseInteractor GetGenericDatabaseInteractor();
        public IList<MigrationInstance> GetAllMigrations();
        public void SetLogConnectionAttemptErrors(bool enabled);
    }
}
