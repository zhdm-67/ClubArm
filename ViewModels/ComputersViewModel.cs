// ViewModels/ComputersViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClubArm.Models;
using ClubArm.Services;
using ClubArm.Views;

namespace ClubArm.ViewModels
{
    public class ComputerDisplayItem : INotifyPropertyChanged
    {
        public Computer Computer { get; set; }
        public Session? ActiveSession { get; set; }

        public string Name => Computer?.Name ?? "?";
        public string Status => Computer?.Status ?? "Unknown";
        public string ClientName => ActiveSession?.Client?.FullName ?? "—";
        public string TariffName => ActiveSession?.Tariff?.Name ?? "—";
        public string StartTime => ActiveSession?.StartTime.ToString("HH:mm:ss") ?? "—";
        public string Duration
        {
            get
            {
                if (ActiveSession == null) return "—";
                var duration = DateTime.Now - ActiveSession.StartTime;
                return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            }
        }
        public string ClientBalance => ActiveSession?.Client?.Balance.ToString("F2") ?? "—";
        public decimal? TotalCost => ActiveSession?.TotalCost;

        // Обновление отображения (вызывается при изменении свойств)
        public void Refresh()
        {
            OnPropertyChanged(nameof(ClientName));
            OnPropertyChanged(nameof(TariffName));
            OnPropertyChanged(nameof(StartTime));
            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(ClientBalance));
            OnPropertyChanged(nameof(TotalCost));
            OnPropertyChanged(nameof(Status));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ComputersViewModel : INotifyPropertyChanged
    {
        private readonly ClubService _service;
        private ObservableCollection<ComputerDisplayItem> _computers;
        private ComputerDisplayItem _selectedComputer;
        private Timer _refreshTimer;

        public ObservableCollection<ComputerDisplayItem> Computers
        {
            get => _computers;
            set { _computers = value; OnPropertyChanged(); }
        }

        public ComputerDisplayItem SelectedComputer
        {
            get => _selectedComputer;
            set { _selectedComputer = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand StartSessionCommand { get; }
        public ICommand StopSessionCommand { get; }

        public ComputersViewModel(ClubService service)
        {
            _service = service;
            AddCommand = new RelayCommand(_ => OpenComputerEditor(null));
            EditCommand = new RelayCommand(_ => OpenComputerEditor(SelectedComputer?.Computer), _ => SelectedComputer != null);
            DeleteCommand = new RelayCommand(_ => DeleteComputer(), _ => SelectedComputer != null);
            RefreshCommand = new RelayCommand(_ => LoadComputers());
            StartSessionCommand = new RelayCommand(_ => StartSession(), _ => SelectedComputer?.Status == "Free");
            StopSessionCommand = new RelayCommand(_ => StopSession(), _ => SelectedComputer?.Status == "Occupied");

            LoadComputers();

            _refreshTimer = new Timer(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var item in Computers)
                    {
                        item.Refresh();
                    }
                });
            }, null, 0, 5000);
        }

        private void LoadComputers()
        {
            var data = _service.GetComputersWithActiveSessions();
            var list = new ObservableCollection<ComputerDisplayItem>();
            foreach (var item in data)
            {
                list.Add(new ComputerDisplayItem
                {
                    Computer = item.Computer,
                    ActiveSession = item.ActiveSession
                });
            }
            Computers = list;
        }

        private void OpenComputerEditor(Computer computer)
        {
            var dialog = new ComputerEditorWindow(computer, _service);
            if (dialog.ShowDialog() == true)
                LoadComputers();
        }

        private void DeleteComputer()
        {
            if (MessageBox.Show("Удалить компьютер?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _service.DeleteComputer(SelectedComputer.Computer.Id);
                LoadComputers();
            }
        }

        private void StartSession()
        {
            // Открываем окно выбора клиента и тарифа, предвыбирая текущий компьютер
            var dialog = new StartSessionWindow(_service, computer: SelectedComputer.Computer);
            if (dialog.ShowDialog() == true)
                LoadComputers();
        }

        private void StopSession()
        {
            if (MessageBox.Show("Завершить сессию на этом компьютере?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _service.StopSessionByComputer(SelectedComputer.Computer.Id);
                LoadComputers();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}