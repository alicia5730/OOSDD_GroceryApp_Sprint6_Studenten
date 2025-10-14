using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Data.Repositories;
using Grocery.Core.Models;

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
        private decimal price;

        private readonly ProductRepository repo = new();

        [RelayCommand]
        private async Task SaveProduct()
        {
            if (string.IsNullOrWhiteSpace(Name) || Price <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Fout", "Vul een geldige naam en prijs in.", "OK");
                return;
            }

            var newProduct = new Product(
                0,                      
                Name,
                Stock > 0 ? Stock : 10,  
                DateOnly.FromDateTime(ShelfLife),
                Price
            );

            repo.Add(newProduct);

            await App.Current.MainPage.DisplayAlert("Succes", $"Product '{Name}' is toegevoegd!", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}