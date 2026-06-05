using System;
using System.Linq;
using System.Windows;
using ClubArm.Models;
using ClubArm.Services;

namespace ClubArm.Views
{
    public partial class StartSessionWindow : Window
    {
        private readonly ClubService _service;
        private readonly Client _preselectedClient;
        private readonly Computer _preselectedComputer;

        public StartSessionWindow(ClubService service, Client preselectedClient = null, Computer computer = null)
        {
            InitializeComponent();
            _service = service;
            _preselectedClient = preselectedClient;
            _preselectedComputer = computer;
            LoadData();
        }

        private void LoadData()
        {
            // Клиенты
            var allClients = _service.GetAllClients();
            cmbClient.ItemsSource = allClients;
            if (_preselectedClient != null)
            {
                cmbClient.SelectedItem = allClients.FirstOrDefault(c => c.Id == _preselectedClient.Id);
                cmbClient.IsEnabled = false;
            }

            // Компьютеры (только свободные)
            var freeComputers = _service.GetAllComputers().Where(c => c.Status == "Free").ToList();
            cmbComputer.ItemsSource = freeComputers;
            if (_preselectedComputer != null && freeComputers.Any(c => c.Id == _preselectedComputer.Id))
            {
                cmbComputer.SelectedItem = freeComputers.First(c => c.Id == _preselectedComputer.Id);
                cmbComputer.IsEnabled = false;
            }

            // Тарифы
            cmbTariff.ItemsSource = _service.GetAllTariffs();
        }

        // Метод обработки нажатия кнопки "Запустить"
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            var client = cmbClient.SelectedItem as Client;
            var computer = cmbComputer.SelectedItem as Computer;
            var tariff = cmbTariff.SelectedItem as Tariff;

            if (client == null || computer == null || tariff == null)
            {
                MessageBox.Show("Выберите клиента, компьютер и тариф", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _service.StartSession(client.Id, computer.Id, tariff.Id);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}