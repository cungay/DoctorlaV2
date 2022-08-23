using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Doctorla.Identity.Core
{
    public interface IDatabaseConnectionFactory
    {
        Task<SqlConnection> CreateConnectionAsync();
        string DbSchema { get; set; }
    }
}
