using System;
using System.Text.RegularExpressions;
using System.Windows;
using ClubArm.Models;
using ClubArm.Services;
using Microsoft.EntityFrameworkCore;

namespace ClubArm.Views
{
    public partial class ClientEditorWindow : Window
    {
        private readonly ClubService _service;
        private readonly Client _client;

        public ClientEditorWindow(Client client, ClubService service)
        {
            InitializeComponent();
            _service = service;
            _client = client ?? new Client { RegistrationDate = DateTime.Now };
            if (client != null)
            {
                txtFullName.Text = client.FullName;
                txtPhone.Text = client.Phone;
                txtDiscount.Text = client.Discount.ToString();
            }
        }

        private bool ValidateForm(out string fullName, out string phone, out decimal discount)
        {
            fullName = txtFullName.Text?.Trim();
            phone = txtPhone.Text?.Trim();
            discount = 0;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Введите ФИО клиента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return false;
            }

            if (fullName.Length < 2 || fullName.Length > 100)
            {
                MessageBox.Show("ФИО должно быть от 2 до 100 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Введите номер телефона.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return false;
            }

            string phonePattern = @"^(\+375|8|375)?[0-9]{9,12}$";
            if (!Regex.IsMatch(phone.Replace(" ", "").Replace("-", ""), phonePattern))
            {
                MessageBox.Show("Введите корректный номер телефона (например, +375291234567 или 80291234567).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return false;
            }

            string discountText = txtDiscount.Text?.Trim();
            if (string.IsNullOrWhiteSpace(discountText))
            {
                discount = 0;
            }
            else if (!decimal.TryParse(discountText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out discount))
            {
                MessageBox.Show("Скидка должна быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDiscount.Focus();
                return false;
            }

            if (discount < 0)
            {
                MessageBox.Show("Скидка не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDiscount.Focus();
                return false;
            }

            if (discount > 100)
            {
                MessageBox.Show("Скидка не может превышать 100%.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDiscount.Focus();
                return false;
            }

            return true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm(out string fullName, out string phone, out decimal discount))
                return;

            _client.FullName = fullName;
            _client.Phone = phone;
            _client.Discount = discount;

            try
            {
                if (_client.Id == 0)
                    _service.AddClient(_client);
                else
                    _service.UpdateClient(_client);
                DialogResult = true;
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                string message = dbEx.InnerException?.Message ?? dbEx.Message;
                if (message.Contains("Arithmetic overflow") || message.Contains("out of range"))
                    MessageBox.Show("Слишком большое значение. Проверьте баланс или скидку.", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show($"Ошибка при сохранении: {message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}