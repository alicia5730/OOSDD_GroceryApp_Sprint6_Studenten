﻿using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
namespace Grocery.App.ViewModels
{
    public partial class ProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        public ObservableCollection<Product> Products { get; set; }

        public ProductViewModel(IProductService productService)
        {
            _productService = productService;
            Products = [];
            foreach (Product p in _productService.GetAll()) Products.Add(p);
        }
        [RelayCommand]
        private async Task AddNewProduct()
        {
            await Shell.Current.GoToAsync(nameof(NewProductView));
        }
        public void LoadProducts()
        {
            Products.Clear();
            foreach (Product p in _productService.GetAll())
            {
                Products.Add(p);
            }
        }

    }
}
