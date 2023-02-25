using GarageManagement.Data.Entity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace GarageManagement.Data.Context
{
    public class DataContext
    {
        private ConnectionStringOptions connectionStringOptions;

        public DataContext(IOptionsMonitor<ConnectionStringOptions> optionsMonitor)
        {
            connectionStringOptions = optionsMonitor.CurrentValue;
        }
        public IDbConnection CreateConnection() => new SqlConnection(connectionStringOptions.SqlConnection);
    }
}
