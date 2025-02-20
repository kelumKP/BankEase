using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankEase.Infrastructure.Repositories
{
    using Microsoft.Data.Sqlite;
    using System;
    using System.Collections.Generic;
    using System.IO;

    namespace BankEase.Infrastructure.Repositories
    {
        public abstract class BaseRepository
        {
            private string ConnectionString => "Data Source=" + Path.Combine(
                Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "BankEase.Infrastructure", "Data", "BankEaseDB.db");

            protected void ExecuteNonQuery(string commandText, Action<SqliteCommand> parameterAction = null)
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = commandText;
                    parameterAction?.Invoke(command);
                    command.ExecuteNonQuery();
                }
            }

            protected T ExecuteScalar<T>(string commandText, Action<SqliteCommand> parameterAction = null)
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = commandText;
                    parameterAction?.Invoke(command);
                    return (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T));
                }
            }

            protected List<T> ExecuteReader<T>(string commandText, Func<SqliteDataReader, T> mapFunction, Action<SqliteCommand> parameterAction = null)
            {
                var results = new List<T>();
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = commandText;
                    parameterAction?.Invoke(command);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(mapFunction(reader));
                        }
                    }
                }
                return results;
            }
        }
    }
}
