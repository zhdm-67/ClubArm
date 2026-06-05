// ViewModels/TariffsViewModel.cs

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
    public class TariffDisplayItem : INotifyPropertyChanged
    {
        public Tariff Tariff { get; set; }

        public int Id => Tariff.Id;
        public string Name => Tariff.Name;
        public string TypeDisplay => Tariff.Type switch
        {
            "Minute" => "Поминутный",
            "Night" => "Ночной",
            "Package" => "Пакетный",
            _ => Tariff.Type
        };
        public decimal PricePerMinute => Tariff.PricePerMinute;
        public int? PackageMinutes => Tariff.PackageMinutes;
        public decimal? PackagePrice => Tariff.PackagePrice;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class TariffsViewModel : INotifyPropertyChanged
    {
        private readonly ClubService _service;
        private ObservableCollection<TariffDisplayItem> _tariffs;
        private TariffDisplayItem _selectedTariff;
        private string _searchText = "";
        private ICollectionView _tariffsView;

        public ObservableCollection<TariffDisplayItem> Tariffs
        {
            get => _tariffs;
            set
            {
                _tariffs = value;
                OnPropertyChanged();
                UpdateCollectionView();
            }
        }

        public TariffDisplayItem SelectedTariff
        {
            get => _selectedTariff;
            set { _selectedTariff = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                _tariffsView?.Refresh();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public TariffsViewModel(ClubService service)
        {
            _service = service;
            AddCommand = new RelayCommand(_ => OpenTariffEditor(null));
            EditCommand = new RelayCommand(_ => OpenTariffEditor(SelectedTariff?.Tariff), _ => SelectedTariff != null);
            DeleteCommand = new RelayCommand(_ => DeleteTariff(), _ => SelectedTariff != null);
            RefreshCommand = new RelayCommand(_ => LoadTariffs());
            ClearSearchCommand = new RelayCommand(_ => SearchText = "");
            LoadTariffs();
        }

        private void LoadTariffs()
        {
            var tariffsRaw = _service.GetAllTariffs();
            Tariffs = new ObservableCollection<TariffDisplayItem>(
                tariffsRaw.Select(t => new TariffDisplayItem { Tariff = t })
            );
        }

        private void UpdateCollectionView()
        {
            _tariffsView = CollectionViewSource.GetDefaultView(Tariffs);
            _tariffsView.Filter = item =>
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return true;
                var tariff = item as TariffDisplayItem;
                return tariff != null && tariff.Name.ToLower().Contains(SearchText.ToLower());
            };
        }

        private void OpenTariffEditor(Tariff tariff)
        {
            var dialog = new TariffEditorWindow(tariff, _service);
            if (dialog.ShowDialog() == true)
                LoadTariffs();
        }

        private void DeleteTariff()
        {
            if (SelectedTariff == null) return;
            if (System.Windows.MessageBox.Show("Удалить тариф? Сессии, использующие этот тариф, будут потеряны.", "Подтверждение",
                System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                _service.DeleteTariff(SelectedTariff.Id);
                LoadTariffs();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}