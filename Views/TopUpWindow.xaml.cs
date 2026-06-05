using System;
using System.Windows;
using ClubArm.Models;
using ClubArm.Services;
using Microsoft.EntityFrameworkCore;

namespace ClubArm.Views
{
    public partial class TopUpWindow : Window
    {
        private readonly Client _client;
        private readonly ClubService _service;

        public TopUpWindow(Client client, ClubService service)
        {
            InitializeComponent();
            _client = client;
            _service = service;
        }

        private bool ValidateAmount(out decimal amount)
        {
            amount = 0;
            string input = txtAmount.Text?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Введите сумму пополнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
                return false;
            }

            if (!decimal.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out amount))
            {
                MessageBox.Show("Введите корректное число (разделитель - точка).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
                return false;
            }

            if (amount <= 0)
            {
                MessageBox.Show("Сумма пополнения должна быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
                return false;
            }

            if (amount > 10000)
            {
                var result = MessageBox.Show($"Сумма {amount} BYN очень велика. Продолжить?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                    return false;
            }

            decimal newBalance = (_client.Balance) + amount;
            if (newBalance > 1000000)
            {
                MessageBox.Show($"После пополнения баланс превысит 1 000 000 BYN. Уменьшите сумму.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
                return false;
            }

            return true;
        }

        private void TopUp_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAmount(out decimal amount))
                return;

            try
            {
                _service.TopUpBalance(_client.Id, amount);
                DialogResult = true;
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                string message = dbEx.InnerException?.Message ?? dbEx.Message;
                if (message.Contains("Arithmetic overflow") || message.Contains("out of range"))
                    MessageBox.Show("Сумма слишком велика для базы данных. Попробуйте уменьшить сумму.", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show($"Ошибка при пополнении: {message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}