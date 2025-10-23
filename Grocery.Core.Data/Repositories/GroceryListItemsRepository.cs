using Grocery.Core.Data;
using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        private readonly List<GroceryListItem> groceryListItems = [];

        public GroceryListItemsRepository()
        {
            // Maak de tabel aan (als die nog niet bestaat)
            CreateTable(@"
                CREATE TABLE IF NOT EXISTS GroceryListItem (
                    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    [GroceryListId] INTEGER NOT NULL,
                    [ProductId] INTEGER NOT NULL,
                    [Amount] INTEGER NOT NULL
                );");

            // Voeg voorbeelddata toe (alleen als ze nog niet bestaan)
            List<string> insertQueries =
            [
                @"INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(1, 1, 1, 3)",
                @"INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(2, 1, 2, 1)",
                @"INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(3, 1, 3, 4)",
                @"INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(4, 2, 1, 2)",
                @"INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(5, 2, 2, 5)"
            ];
            InsertMultipleWithTransaction(insertQueries);
            GetAll(); // preload cache
        }

        public List<GroceryListItem> GetAll()
        {
            groceryListItems.Clear();
            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem;";
            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    groceryListItems.Add(new GroceryListItem(id, groceryListId, productId, amount));
                }
            }

            CloseConnection();
            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int groceryListId)
        {
            return GetAll().Where(g => g.GroceryListId == groceryListId).ToList();
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            OpenConnection();
            using (var checkCmd = Connection.CreateCommand())
            {
                checkCmd.CommandText = "SELECT COUNT(*) FROM GroceryListItem WHERE GroceryListId=@list AND ProductId=@prod";
                checkCmd.Parameters.AddWithValue("@list", item.GroceryListId);
                checkCmd.Parameters.AddWithValue("@prod", item.ProductId);
                var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    CloseConnection();
                    throw new InvalidOperationException("Product bestaat al in de boodschappenlijst.");
                }
            }

            string insertQuery = @"
        INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount)
        VALUES(@GroceryListId, @ProductId, @Amount);
        SELECT last_insert_rowid();";

            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                command.Parameters.AddWithValue("@Amount", item.Amount);
                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }

            CloseConnection();
            groceryListItems.Add(item);
            return item;
        }

        public GroceryListItem? Get(int id)
        {
            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE Id = @Id;";
            GroceryListItem? item = null;
            OpenConnection();

            using (SqliteCommand command = new(selectQuery, Connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                SqliteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    item = new GroceryListItem(
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.GetInt32(2),
                        reader.GetInt32(3)
                    );
                }
            }

            CloseConnection();
            return item;
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            string updateQuery = @"
                UPDATE GroceryListItem 
                SET GroceryListId = @GroceryListId, ProductId = @ProductId, Amount = @Amount 
                WHERE Id = @Id;";

            OpenConnection();
            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("@Id", item.Id);
                command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                command.Parameters.AddWithValue("@Amount", item.Amount);
                command.ExecuteNonQuery();
            }
            CloseConnection();

            return item;
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            string deleteQuery = "DELETE FROM GroceryListItem WHERE Id = @Id;";
            OpenConnection();
            using (SqliteCommand command = new(deleteQuery, Connection))
            {
                command.Parameters.AddWithValue("@Id", item.Id);
                command.ExecuteNonQuery();
            }
            CloseConnection();
            groceryListItems.RemoveAll(g => g.Id == item.Id);
            return item;
        }
    }
}
