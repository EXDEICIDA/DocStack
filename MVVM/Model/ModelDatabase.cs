using System;
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

        //A method for adding into the favorites table 
        public async Task AddToFavoritesAsync(Paper paper)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                string insertQuery = @"INSERT OR REPLACE INTO FavoritePapers 
                               (Authors, Title, Journal, Year, ColorCode, FullTextLink, DOI) 
                               VALUES (@Authors, @Title, @Journal, @Year, @ColorCode, @FullTextLink, @DOI)";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Authors", paper.Authors);
                    command.Parameters.AddWithValue("@Title", paper.Title);
                    command.Parameters.AddWithValue("@Journal", paper.Journal);
                    command.Parameters.AddWithValue("@Year", paper.Year);
                    command.Parameters.AddWithValue("@ColorCode", "Grey");  // Default value or you can pass paper.ColorCode if it's set
                    command.Parameters.AddWithValue("@FullTextLink", paper.FullTextLink);
                    command.Parameters.AddWithValue("@DOI", paper.DOI);

                    await command.ExecuteNonQueryAsync();
                }
            }
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

        public async Task<List<Paper>> GetAllFavoritePapersAsync()
        {
            List<Paper> favoritePapers = new List<Paper>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT * FROM FavoritePapers";

                using (var command = new SQLiteCommand(selectQuery, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        favoritePapers.Add(new Paper
                        {
                            PaperID = reader.GetInt32(reader.GetOrdinal("ID")),
                            Authors = reader.GetString(reader.GetOrdinal("Authors")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Journal = reader.GetString(reader.GetOrdinal("Journal")),
                            Year = reader.GetInt32(reader.GetOrdinal("Year")),
                            DOI = reader.IsDBNull(reader.GetOrdinal("DOI")) ? null : reader.GetString(reader.GetOrdinal("DOI")),
                            FullTextLink = reader.IsDBNull(reader.GetOrdinal("FullTextLink")) ? null : reader.GetString(reader.GetOrdinal("FullTextLink"))
                        });
                    }
                }
            }

            return favoritePapers;
        }

        //Papers view and table mmethods 
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
                            PaperID = reader.GetInt32(reader.GetOrdinal("PaperID")),
                            Authors = reader.GetString(reader.GetOrdinal("Authors")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Journal = reader.GetString(reader.GetOrdinal("Journal")),
                            Year = reader.GetInt32(reader.GetOrdinal("Year")),
                            DOI = reader.IsDBNull(reader.GetOrdinal("DOI")) ? null : reader.GetString(reader.GetOrdinal("DOI")),
                            FullTextLink = reader.IsDBNull(reader.GetOrdinal("FullTextLink")) ? null : reader.GetString(reader.GetOrdinal("FullTextLink"))
                        });
                    }
                }
            }

            return papers;
        }
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



                //Papers Table - for holding the research papers datas
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

                //Documents table - for holding the documents
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

                //Favorite papers - a table for holding favorites

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        connection.Open();
                        Console.WriteLine("Database connection opened successfully.");

                        string createFavoritePapersTableQuery = @"
                    CREATE TABLE IF NOT EXISTS FavoritePapers (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Authors TEXT NOT NULL,
                        Title TEXT NOT NULL,
                        Journal TEXT,
                        Year INTEGER,
                        ColorCode TEXT,
                        FullTextLink TEXT,
                        DOI TEXT UNIQUE
                    )";

                        using (var command = new SQLiteCommand(createFavoritePapersTableQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        Console.WriteLine("FavoritePapers table created successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error initializing database: {ex.Message}");
                        throw;
                    }
                }
            }
           
        }
    }
}