
using Grocery.Core.Data.Helpers;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data
{
    public abstract class DatabaseConnection : IDisposable
    {
        protected SqliteConnection Connection { get; }
        string databaseName;

        public DatabaseConnection(string? overrideConnection = null)
        {
            // 1️⃣ Gebruik override of fallback naar appsettings
            databaseName = !string.IsNullOrEmpty(overrideConnection)
                ? overrideConnection
                : ConnectionHelper.ConnectionStringValue("GroceryAppDb");

            // 2️⃣ Bouw pad correct op
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(projectDirectory, databaseName);

            // 3️⃣ Zorg dat map bestaat
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            // 4️⃣ Maak SQLite-verbinding
            string dbpath = $"Data Source={fullPath}";
            Connection = new SqliteConnection(dbpath);
            Console.WriteLine($"🧩 Using SQLite database at: {fullPath}");

            try
            {
                // 👇 forceer dat SQLite het bestand echt aanmaakt
                Connection.Open();
                Console.WriteLine($"✅ SQLite DB initialized at: {Path.Combine(projectDirectory, databaseName)}");
                Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to create SQLite DB: {ex.Message}");
            }
        }

        protected void OpenConnection()
        {
            if (Connection.State != System.Data.ConnectionState.Open) Connection.Open();
        }

        protected void CloseConnection()
        {
            if (Connection.State != System.Data.ConnectionState.Closed) Connection.Close();
        }

        public void CreateTable(string commandText)
        {
            OpenConnection();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }

        public void InsertMultipleWithTransaction(List<string> linesToInsert)
        {
            OpenConnection();
            var transaction = Connection.BeginTransaction();

            try
            {
                linesToInsert.ForEach(l => Connection.ExecuteNonQuery(l));
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                transaction.Rollback();
            }
            finally
            {
                transaction.Dispose();
            }
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}
