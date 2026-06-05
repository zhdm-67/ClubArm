// ViewModels/ReportsViewModel

using ClubArm.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ClubArm.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly ClubService _service;
        private DateTime _startDate = DateTime.Today;
        private DateTime _endDate = DateTime.Today;
        private decimal _totalRevenue;
        private string _computerLoadReport;
        private string _tariffPopularity;

        public DateTime StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }
        public DateTime EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }
        public decimal TotalRevenue { get => _totalRevenue; set { _totalRevenue = value; OnPropertyChanged(); } }
        public string ComputerLoadReport { get => _computerLoadReport; set { _computerLoadReport = value; OnPropertyChanged(); } }
        public string TariffPopularity { get => _tariffPopularity; set { _tariffPopularity = value; OnPropertyChanged(); } }

        public ICommand GenerateReportCommand { get; }

        public ReportsViewModel(ClubService service)
        {
            _service = service;
            GenerateReportCommand = new RelayCommand(_ => GenerateReports());
            GenerateReports();
        }

        private void GenerateReports()
        {
            TotalRevenue = _service.GetTotalRevenue(StartDate, EndDate);
            var load = _service.GetComputerLoad(StartDate, EndDate);
            ComputerLoadReport = string.Join("\n", load.Select(kv => $"{kv.Key}: {kv.Value} мин."));
            var popular = _service.GetPopularTariffs(StartDate, EndDate);
            TariffPopularity = string.Join("\n", popular.Select(kv => $"{kv.Key}: {kv.Value} сессий"));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}