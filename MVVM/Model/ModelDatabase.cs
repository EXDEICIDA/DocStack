using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace DocStack.MVVM.Model
{
    public class ModelDatabase
    {
        private readonly string _connectionString;

        //This is an database module which will handle our databse
        //operations as well as configurations

        public ModelDatabase(string databaseFileName = "DocStackApp.db")
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "Cerberus");
            string databasePath = Path.Combine(appFolder, databaseFileName);

            Directory.CreateDirectory(appFolder);

            _connectionString = $"Data Source={databasePath};Version=3;";

            //database initialization
            InitializeDatabase(databasePath);
        }

        //Databse init method.

        private void InitializeDatabase(string databasePath)
        {
            if (!File.Exists(databasePath))
            {
                try
                {
                    SQLiteConnection.CreateFile(databasePath);
                    Console.WriteLine($"Database file created at: {databasePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating database file: {ex.Message}");
                    throw;
                }

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        connection.Open();
                        Console.WriteLine("Database connection opened successfully.");

                        // Documents table
                        string createDocumentsTableQuery = @"CREATE TABLE IF NOT EXISTS Documents (
                                                           Id INTEGER PRIMARY KEY,
                                                           document_name TEXT NOT NULL,
                                                           file_path TEXT NOT NULL,
                                                           file_size INTEGER NOT NULL,
                                                           date_added TEXT NOT NULL
                                                                                      )";
                        using (var command = new SQLiteCommand(createDocumentsTableQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        {
                            Console.WriteLine($"Error initializing database: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
        }

        // From now on other methods concenring the database and other stuff



    }
}