// ViewModels/ClientsViewModel.cs

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ClubArm.Models;
using ClubArm.Services;
using ClubArm.Views;

namespace ClubArm.ViewModels
{
    public class ClientDisplayItem : INotifyPropertyChanged
    {
        public Client Client { get; set; }
        public int TotalSessions { get; set; }
        public decimal TotalSpent { get; set; }

        public string FullName => Client?.FullName ?? "";
        public string Phone => Client?.Phone ?? "";
        public decimal Balance => Client?.Balance ?? 0;
        public decimal Discount => Client?.Discount ?? 0;
        public DateTime RegistrationDate => Client?.RegistrationDate ?? DateTime.Now;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ClientsViewModel : INotifyPropertyChanged
    {
        private readonly ClubService _service;
        private ObservableCollection<ClientDisplayItem> _clients;
        private ClientDisplayItem _selectedClient;
        private string _searchText = "";
        private ICollectionView _clientsView;

        public ObservableCollection<ClientDisplayItem> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged();
                UpdateCollectionView();
            }
        }

        public ClientDisplayItem SelectedClient
        {
            get => _selectedClient;
            set { _selectedClient = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                _clientsView?.Refresh();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand TopUpCommand { get; }
        public ICommand StartSessionCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public ClientsViewModel(ClubService service)
        {
            _service = service;
            AddCommand = new RelayCommand(_ => OpenClientEditor(null));
            EditCommand = new RelayCommand(_ => OpenClientEditor(SelectedClient?.Client), _ => SelectedClient != null);
            DeleteCommand = new RelayCommand(_ => DeleteClient(), _ => SelectedClient != null);
            RefreshCommand = new RelayCommand(_ => LoadClients());
            TopUpCommand = new RelayCommand(_ => TopUpBalance(), _ => SelectedClient != null);
            StartSessionCommand = new RelayCommand(_ => StartSession(), _ => SelectedClient != null);
            ClearSearchCommand = new RelayCommand(_ => SearchText = "");
            LoadClients();
        }

        private void LoadClients()
        {
            var clientsRaw = _service.GetAllClients();
            var list = new ObservableCollection<ClientDisplayItem>();
            foreach (var client in clientsRaw)
            {
                // Получаем количество сессий и сумму трат (транзакции с отрицательной суммой)
                var sessionsCount = _service.GetSessionsCount(client.Id);
                var totalSpent = _service.GetTotalSpent(client.Id);
                list.Add(new ClientDisplayItem
                {
                    Client = client,
                    TotalSessions = sessionsCount,
                    TotalSpent = totalSpent
                });
            }
            Clients = list;
        }

        private void UpdateCollectionView()
        {
            _clientsView = CollectionViewSource.GetDefaultView(Clients);
            _clientsView.Filter = item =>
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return true;
                var client = item as ClientDisplayItem;
                if (client == null) return false;
                return client.FullName.ToLower().Contains(SearchText.ToLower()) ||
                       client.Phone.ToLower().Contains(SearchText.ToLower());
            };
        }

        private void OpenClientEditor(Client client)
        {
            var dialog = new ClientEditorWindow(client, _service);
            if (dialog.ShowDialog() == true)
                LoadClients();
        }

        private void DeleteClient()
        {
            if (SelectedClient == null) return;
            if (MessageBox.Show("Удалить клиента? Все его сессии и транзакции будут удалены.", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _service.DeleteClient(SelectedClient.Client.Id);
                LoadClients();
            }
        }

        private void TopUpBalance()
        {
            var dialog = new TopUpWindow(SelectedClient.Client, _service);
            if (dialog.ShowDialog() == true)
                LoadClients();
        }

        private void StartSession()
        {
            var dialog = new StartSessionWindow(_service, preselectedClient: SelectedClient.Client);
            if (dialog.ShowDialog() == true)
                LoadClients();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}