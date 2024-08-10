using Dapper;
using System.Data;
using System.Data.SQLite;

namespace Kuti.Windows.Settings
{
    public interface IDatabase
    {
        IDbConnection GetOpenConnection();
    }

    public class Database : IDatabase
    {
        private readonly string _connectionString;
        private readonly IEnumerable<string> _setupScripts;

        private static object _lock = new object();
        private bool _wasInitialized = false;

        public IDbConnection GetOpenConnection()
        {
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            if (!_wasInitialized)
            {
                lock (_lock)
                {
                    if (!_wasInitialized)
                    {
                        Initialize(connection);
                    }
                }
            }

            return connection;
        }

        private void Initialize(IDbConnection connection)
        {
            using var txn = connection.BeginTransaction(); 
            try
            {
                foreach (var script in _setupScripts)
                {
                    connection.Execute(script, txn);
                }
                txn.Commit();
                _wasInitialized = true;
            }
            catch (Exception)
            {
                txn.Rollback();
                txn.Dispose();
                connection.Close();
                connection.Dispose();
                throw;
            }
        }

        public Database(string databasePath, IEnumerable<string> setupScripts)
        {
            _connectionString = $"Data Source={databasePath}";
            _setupScripts = setupScripts;
        }
    }

    public static class DatabaseExtensions
    {
        public static void SetParameter<T>(this IDbCommand command, string name, T value)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }
    }
}
