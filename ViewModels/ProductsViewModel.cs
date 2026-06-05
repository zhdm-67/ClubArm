// ViewModels/ProductsViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using ClubArm.Models;
using ClubArm.Services;
using ClubArm.Views;

namespace ClubArm.ViewModels
{
    public class ProductDisplayItem : INotifyPropertyChanged
    {
        public Product Product { get; set; }
        public int Id => Product.Id;
        public string Name => Product.Name;
        public decimal Price => Product.Price;
        public int Stock => Product.Stock;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ProductsViewModel : INotifyPropertyChanged
    {
        private readonly ClubService _service;
        private ObservableCollection<ProductDisplayItem> _products;
        private ProductDisplayItem _selectedProduct;
        private string _searchText = "";
        private ICollectionView _productsView;

        public ObservableCollection<ProductDisplayItem> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
                UpdateCollectionView();
            }
        }

        public ProductDisplayItem SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                _productsView?.Refresh();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public ProductsViewModel(ClubService service)
        {
            _service = service;
            AddCommand = new RelayCommand(_ => OpenProductEditor(null));
            EditCommand = new RelayCommand(_ => OpenProductEditor(SelectedProduct?.Product), _ => SelectedProduct != null);
            DeleteCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null);
            RefreshCommand = new RelayCommand(_ => LoadProducts());
            ClearSearchCommand = new RelayCommand(_ => SearchText = "");
            LoadProducts();
        }

        private void LoadProducts()
        {
            var productsRaw = _service.GetAllProducts();
            Products = new ObservableCollection<ProductDisplayItem>(
                productsRaw.Select(p => new ProductDisplayItem { Product = p })
            );
        }

        private void UpdateCollectionView()
        {
            _productsView = CollectionViewSource.GetDefaultView(Products);
            _productsView.Filter = item =>
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return true;
                var prod = item as ProductDisplayItem;
                return prod != null && prod.Name.ToLower().Contains(SearchText.ToLower());
            };
        }

        private void OpenProductEditor(Product product)
        {
            var dialog = new ProductEditorWindow(product, _service);
            if (dialog.ShowDialog() == true)
                LoadProducts();
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;
            if (System.Windows.MessageBox.Show("Удалить товар? Продажи с этим товаром будут потеряны.", "Подтверждение",
                System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                _service.DeleteProduct(SelectedProduct.Id);
                LoadProducts();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}