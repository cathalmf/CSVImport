using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace CSVImporter.MSSQLDatabaseConnection
{
    public class DatabaseConnection
    {
        private readonly static string ConnectionString;

        static DatabaseConnection()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
        }

        /// <summary>
        /// Bulk inserts data from a csv file into database table
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static int SqlBulkInsert(string databaseTable, string path)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                string SQL = string.Format(@"BULK INSERT {0}
                        FROM {1}{2}{3}
                        WITH
                        (
                            FIELDTERMINATOR = ',',
                            ROWTERMINATOR = '\n',
                            MAXERRORS = 0,
                            DATAFILETYPE = 'widechar',
                            KEEPIDENTITY
                        )", databaseTable, "\"",path,"\"");

                SqlCommand command = new SqlCommand(SQL, connection);
                command.CommandTimeout = 10 * 60;   // Give the insert plenty of time. 
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected;
            }
        }
    }
}
