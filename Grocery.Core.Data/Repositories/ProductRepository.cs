using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;

namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository :DatabaseConnection, IProductRepository
    {
        private readonly List<Product> products;
        public ProductRepository()
        {
            // Tabel aanmaken als die nog niet bestaat
            CreateTable(@"CREATE TABLE IF NOT EXISTS Product (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Stock INTEGER NOT NULL,
                            ShelfLife TEXT NOT NULL,
                            Price REAL NOT NULL
                          )");

            OpenConnection();
            using (var checkCommand = Connection.CreateCommand())
            {
                checkCommand.CommandText = "SELECT COUNT(*) FROM Product";
                var count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count == 0)
                {
                    Console.WriteLine("[SEED] Product table is empty, inserting demo data...");

                    // Voeg testproducten toe
                    var seedProducts = new List<Product>
                    {
                        new Product(1, "Melk", 300, new DateOnly(2025, 9, 25), 0.95m),
                        new Product(2, "Kaas", 100, new DateOnly(2025, 9, 30), 7.98m),
                        new Product(3, "Brood", 400, new DateOnly(2025, 9, 12), 2.19m),
                        new Product(4, "Cornflakes", 0, new DateOnly(2025, 12, 31), 1.48m)
                    };

                    foreach (var p in seedProducts)
                    {
                        using var insertCmd = Connection.CreateCommand();
                        insertCmd.CommandText = "INSERT INTO Product (Name, Stock, ShelfLife, Price) VALUES (@n, @s, @e, @p)";
                        insertCmd.Parameters.AddWithValue("@n", p.Name);
                        insertCmd.Parameters.AddWithValue("@s", p.Stock);
                        insertCmd.Parameters.AddWithValue("@e", p.ShelfLife.ToString("yyyy-MM-dd"));
                        insertCmd.Parameters.AddWithValue("@p", p.Price);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
            CloseConnection();
        }
        public List<Product> GetAll()
        {
            var products = new List<Product>();
            OpenConnection();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Name, Stock, ShelfLife, Price FROM Product";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetInt32(2),
                            DateOnly.Parse(reader.GetString(3)),
                            (decimal)reader.GetDouble(4)
                        ));
                    }
                }
            }

            CloseConnection();
            return products;
            
        }

        public Product? Get(int id)
        {
            OpenConnection();
            using var command = Connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Stock, ShelfLife, Price FROM Product WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var product = new Product(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    DateOnly.Parse(reader.GetString(3)),
                    (decimal)reader.GetDouble(4)
                );
                CloseConnection();
                return product;
            }

            CloseConnection();
            return null;
            
        }

        public Product Add(Product item)
        {
            OpenConnection();
            using var command = Connection.CreateCommand();
            command.CommandText = "INSERT INTO Product (Name, Stock, ShelfLife, Price) VALUES (@n, @s, @e, @p)";
            command.Parameters.AddWithValue("@n", item.Name);
            command.Parameters.AddWithValue("@s", item.Stock);
            command.Parameters.AddWithValue("@e", item.ShelfLife.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@p", item.Price);
            command.ExecuteNonQuery();
            CloseConnection();
            return item;        }

        public Product? Delete(Product item)
        {
            OpenConnection();
            using var command = Connection.CreateCommand();
            command.CommandText = "DELETE FROM Product WHERE Id = @id";
            command.Parameters.AddWithValue("@id", item.Id);
            var result = command.ExecuteNonQuery();
            CloseConnection();
            return result > 0 ? item : null;
            
        }

        public Product? Update(Product item)
        {
            OpenConnection();
            using var command = Connection.CreateCommand();
            command.CommandText = @"UPDATE Product 
                                    SET Name = @n, Stock = @s, ShelfLife = @e, Price = @p
                                    WHERE Id = @id";
            command.Parameters.AddWithValue("@n", item.Name);
            command.Parameters.AddWithValue("@s", item.Stock);
            command.Parameters.AddWithValue("@e", item.ShelfLife.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@p", item.Price);
            command.Parameters.AddWithValue("@id", item.Id);

            var rows = command.ExecuteNonQuery();
            CloseConnection();
            return rows > 0 ? item : null;
        }
    }
}
