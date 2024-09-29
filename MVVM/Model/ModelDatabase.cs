using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DocStack.MVVM.Model
{
    public class ModelDatabase
    {
        private readonly string _connectionString;

        /*
        This is an database module which will handle our databse
        operations as well as configurations
        */

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

        //other methods concenring the database and other stuff
        public async Task AddDocumentAsync(string documentName, string filePath, long fileSize, string dateAdded)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                string insertQuery = @"INSERT INTO Documents (document_name, file_path, file_size, date_added) 
                                       VALUES (@name, @path, @size, @date)";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@name", documentName);
                    command.Parameters.AddWithValue("@path", filePath);
                    command.Parameters.AddWithValue("@size", fileSize);
                    command.Parameters.AddWithValue("@date", dateAdded);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<DocumentsModel>> GetAllDocumentsAsync()
        {
            List<DocumentsModel> documents = new List<DocumentsModel    >();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT * FROM Documents";

                using (var command = new SQLiteCommand(selectQuery, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        documents.Add(new   DocumentsModel
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            FilePath = reader.GetString(2),
                            Size = reader.GetInt64(3),
                            DateAdded = reader.GetString(4)
                        });
                    }
                }
            }

            return documents;
        }


    }
}