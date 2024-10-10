﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Xml.Linq;
using DocStack.MVVM.ViewModel;

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
            string appFolder = Path.Combine(appDataPath, "DocStack");
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

                        // Papers table with updated columns
                        string createPapersTableQuery = @"
                                     CREATE TABLE IF NOT EXISTS Papers (
                                     PaperID INTEGER PRIMARY KEY AUTOINCREMENT,
                                     Authors TEXT NOT NULL,
                                     Title TEXT NOT NULL,
                                     Journal TEXT NOT NULL,
                                     Year INTEGER NOT NULL,
                                     DOI TEXT,
                                     FullTextLink TEXT
            )";

                        using (var command = new SQLiteCommand(createPapersTableQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        Console.WriteLine("Papers table created successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error initializing database: {ex.Message}");
                        throw;
                    }
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
            else
            {
                // Add is_starred column if it doesn't exist
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string addColumnQuery = @"
                        PRAGMA table_info(Documents);
                    ";
                    using (var command = new SQLiteCommand(addColumnQuery, connection))
                    {
                        var reader = command.ExecuteReader();
                        bool columnExists = false;
                        while (reader.Read())
                        {
                            if (reader["name"].ToString() == "is_starred")
                            {
                                columnExists = true;
                                break;
                            }
                        }
                        if (!columnExists)
                        {
                            string alterTableQuery = @"
                                ALTER TABLE Documents ADD COLUMN is_starred INTEGER DEFAULT 0;
                            ";
                            using (var alterCommand = new SQLiteCommand(alterTableQuery, connection))
                            {
                                alterCommand.ExecuteNonQuery();
                            }
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



        //Papers view and table mmethods 
        public async Task AddPaperAsync(Paper paper)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                string insertQuery = @"INSERT INTO Papers (Authors, Title, Journal, Year, DOI, FullTextLink) 
                                       VALUES (@authors, @title, @journal, @year, @doi, @fullTextLink)";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@authors", paper.Authors);
                    command.Parameters.AddWithValue("@title", paper.Title);
                    command.Parameters.AddWithValue("@journal", paper.Journal);
                    command.Parameters.AddWithValue("@year", paper.Year);
                    command.Parameters.AddWithValue("@doi", paper.DOI ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@fullTextLink", paper.FullTextLink ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Paper>> GetAllPapersAsync()
        {
            List<Paper> papers = new List<Paper>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT * FROM Papers";

                using (var command = new SQLiteCommand(selectQuery, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        papers.Add(new Paper
                        {
                            PaperID = reader.GetInt32(0),
                            Authors = reader.GetString(1),
                            Title = reader.GetString(2),
                            Journal = reader.GetString(3),
                            Year = reader.GetInt32(4),
                            DOI = reader.IsDBNull(5) ? null : reader.GetString(5),
                            FullTextLink = reader.IsDBNull(6) ? null : reader.GetString(6)
                        });
                    }
                }
            }

            return papers;
        }




        public async Task<List<DocumentsModel>> GetAllDocumentsAsync()
        {
            List<DocumentsModel> documents = new List<DocumentsModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT * FROM Documents";

                using (var command = new SQLiteCommand(selectQuery, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        documents.Add(new DocumentsModel
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