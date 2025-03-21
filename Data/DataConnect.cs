using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Assignment.Data
{
    public class DatabaseConnection
    {
        private string connectionString;

        public DatabaseConnection(SqlConnectionStringBuilder builder)
        {
            // Build the connection string.  Be very careful with storing passwords.
            // Consider using Windows Authentication or Azure AD authentication if possible.
            //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            //builder.DataSource = server;
            //builder.InitialCatalog = database;
            //builder.UserID = userId;
            //builder.Password = password;
            //builder.Encrypt = true;
            //builder.TrustServerCertificate = true;
            //Optional settings.
            //builder.ConnectTimeout = 30; // in seconds
            //builder.IntegratedSecurity = false; // Explicitly set to false when using user/pass.
            //builder.MultipleActiveResultSets = true; // Allows multiple open readers.
            //builder.Encrypt = true; // Use encryption.
            //builder.TrustServerCertificate = false; // Validate server certificate.

            connectionString = builder.ConnectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public void TestConnection()
        {
            using (SqlConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Connection successful!");
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Error connecting to database: " + ex.Message);
                    // Optionally, log the full exception details:
                    // Console.WriteLine(ex.ToString());
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }

        // Example usage (in a console application or other context):
        public static List<Dictionary<string, object>> GetListData(SqlConnectionStringBuilder builder)
        {
            string server = "DESKTOP-6QJG7RL\\SQLEXPRESS"; // e.g., "localhost\\SQLEXPRESS" or "yourserver.database.windows.net"
            string database = "TreasureHuntDb";
            string userId = "sa";
            string password = "123";

            DatabaseConnection dbConnection = new DatabaseConnection(builder);
            dbConnection.TestConnection();
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

            // Example of using the connection for a query:
            using (SqlConnection connection = dbConnection.GetConnection())
            {
                try
                {
                    connection.Open();
     
                    string sql = "SELECT * FROM TreasureMap;";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader[i];
                                }
                                results.Add(row);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Exception during query: " + ex.Message);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
                return results;
            }
        }
        public static void InsertData(SqlConnectionStringBuilder builder, string tableName, string columns, string values, SqlParameter[] parameters)
        {
            DatabaseConnection dbConnection = new DatabaseConnection(builder);
            dbConnection.TestConnection();
            using (SqlConnection connection = dbConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    string sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values});";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        command.ExecuteNonQuery();
                        Console.WriteLine("Data inserted successfully.");
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error: {ex.Message}");
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }
    }
}
