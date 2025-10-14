using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Data.Repositories;
using Grocery.Core.Models;
using System.Globalization;

namespace Grocery.App.ViewModels
{
    public partial class NewProductViewModel : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int stock;

        [ObservableProperty]
        private DateTime shelfLife = DateTime.Now.AddMonths(6);

        [ObservableProperty]
        private string price; // 🧠 nu string i.p.v. decimal!

        private readonly ProductRepository repo = new();

        [RelayCommand]
        private async Task SaveProduct()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Price))
            {
                await App.Current.MainPage.DisplayAlert("Fout", "Vul een productnaam en prijs in.", "OK");
                return;
            }

            // ✅ werkt met komma of punt
            if (!decimal.TryParse(Price.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedPrice))
            {
                await App.Current.MainPage.DisplayAlert("Fout", "Vul een geldige prijs in, bijv. 2,50 of 2.50.", "OK");
                return;
            }

            if (parsedPrice <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Fout", "Prijs moet groter zijn dan 0.", "OK");
                return;
            }

            parsedPrice = Math.Round(parsedPrice, 2);

            var newProduct = new Product(
                0,
                Name,
                Stock > 0 ? Stock : 0,
                DateOnly.FromDateTime(ShelfLife),
                parsedPrice
            );

            repo.Add(newProduct);

            await App.Current.MainPage.DisplayAlert("Succes", $"Product '{Name}' toegevoegd met prijs €{parsedPrice:F2}", "OK");
            Name = string.Empty;
            Price = string.Empty;
            Stock = 0;
            ShelfLife = DateTime.Now.AddMonths(6);
        }

    }
}
