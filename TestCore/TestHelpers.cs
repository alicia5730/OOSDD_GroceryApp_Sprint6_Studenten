using Grocery.Core.Models;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.Json;
using Grocery.Core.Data.Repositories;
using Grocery.Core.Helpers;
using Microsoft.Data.Sqlite;

namespace TestCore
{
    public class TestHelpers
    {
        private string testDbFile;
        private ProductRepository repo;
        [SetUp]
        public void Setup()
        {
            testDbFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"testdb_{Guid.NewGuid():N}.sqlite");
            repo = new ProductRepository(testDbFile); 
        }

        [TearDown]
        public void Cleanup()
        {
            repo?.Dispose();
        }

        //Happy flow
        [Test]
        public void TestPasswordHelperReturnsTrue()
        {
            string password = "user3";
            string passwordHash = "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA=";
            Assert.IsTrue(PasswordHelper.VerifyPassword(password, passwordHash));
        }

        [TestCase("user1", "IunRhDKa+fWo8+4/Qfj7Pg==.kDxZnUQHCZun6gLIE6d9oeULLRIuRmxmH2QKJv2IM08=")]
        [TestCase("user3", "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA=")]
        public void TestPasswordHelperReturnsTrue(string password, string passwordHash)
        {
            Assert.IsTrue(PasswordHelper.VerifyPassword(password, passwordHash));
        }


        //Unhappy flow
        [Test]
        public void TestPasswordHelperReturnsFalse()
        {
            string password = "user3";
            string passwordHash = "sxnIcZdYt8wC8MYWcQVQjQ";
            Assert.IsFalse(PasswordHelper.VerifyPassword(password, passwordHash));
        }

        [TestCase("user1", "IunRhDKa+fWo8+4/Qfj7Pg")]
        [TestCase("user3", "sxnIcZdYt8wC8MYWcQVQjQ")]
        public void TestPasswordHelperReturnsFalse(string password, string passwordHash)
        {
            Assert.IsFalse(PasswordHelper.VerifyPassword(password, passwordHash));
        }
        
        // 🟢 TC18-01 - Happy path: product succesvol toegevoegd aan boodschappenlijst
        [Test]
        public void TC18_01_ProductToevoegenAanBoodschappenlijst_MoetWordenOpgeslagen()
        {
            var repo = new GroceryListItemsRepository();
            var nieuwItem = new GroceryListItem(0, groceryListId: 3, productId: 9, amount: 2);

            // Act
            var toegevoegd = repo.Add(nieuwItem);
            var alleItems = repo.GetAllOnGroceryListId(3);

            // Assert
            Assert.That(toegevoegd.Id, Is.GreaterThan(0), "Toegevoegd item zou een geldig ID moeten hebben.");
            Assert.IsTrue(alleItems.Any(i => i.ProductId == 9 && i.Amount == 2),
                "Product met ID 9 zou moeten zijn toegevoegd aan boodschappenlijst 3.");
        }


// 🔴 TC18-02 - Unhappy path: hetzelfde product mag niet nogmaals toegevoegd worden
        [Test]
        public void TC18_02_ProductDubbelToevoegenAanBoodschappenlijst_MoetWordenGeweigerd()
        {
            var repo = new GroceryListItemsRepository();
            var item = new GroceryListItem(0, groceryListId: 3, productId: 10, amount: 1);

            // Eerste keer lukt
            repo.Add(item);

            // Tweede keer moet mislukken
            var ex = Assert.Throws<InvalidOperationException>(() => repo.Add(item));
            Assert.That(ex.Message, Is.EqualTo("Product bestaat al in de boodschappenlijst."));
        }
        //Happy Path - TC19-01
        [Test]
        public void TC19_01_ProductToevoegen_MoetWordenOpgeslagenInDatabase()
        {
            var product = new Product(0, "Melk", 300, new DateOnly(2025, 9, 25), 0.95m);
            repo.Add(product);

            var all = repo.GetAll();
            Assert.IsTrue(all.Any(p => p.Name == "Melk" && p.Price == 0.95m));
        }

        //Unhappy Path - TC19-02 (ongeldige prijs)
        [Test]
        public void TC19_02_ProductToevoegen_OngeldigePrijs_MoetFoutmeldingGeven()
        {
            var invalid = new Product(0, "Melk", 100, new DateOnly(2025, 9, 25), -2m);
            var ex = Assert.Throws<ArgumentException>(() => repo.Add(invalid));
            Assert.That(ex.Message, Is.EqualTo("Prijs moet groter zijn dan 0."));

        }

    }
}