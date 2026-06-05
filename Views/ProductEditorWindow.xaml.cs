using System;
using System.Windows;
using ClubArm.Models;
using ClubArm.Services;
using Microsoft.EntityFrameworkCore;

namespace ClubArm.Views
{
    public partial class ProductEditorWindow : Window
    {
        private readonly ClubService _service;
        private readonly Product _product;

        public ProductEditorWindow(Product product, ClubService service)
        {
            InitializeComponent();
            _service = service;
            _product = product ?? new Product();
            if (product != null)
            {
                txtName.Text = product.Name;
                txtPrice.Text = product.Price.ToString();
                txtStock.Text = product.Stock.ToString();
            }
        }

        private bool ValidateForm(out string name, out decimal price, out int stock)
        {
            name = txtName.Text?.Trim();
            price = 0;
            stock = 0;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название товара.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return false;
            }

            string priceText = txtPrice.Text?.Trim();
            if (string.IsNullOrWhiteSpace(priceText))
            {
                MessageBox.Show("Введите цену товара.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return false;
            }

            if (!decimal.TryParse(priceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price))
            {
                MessageBox.Show("Цена должна быть числом (разделитель - точка).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return false;
            }

            if (price <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return false;
            }

            if (price > 100000)
            {
                if (MessageBox.Show("Цена очень высокая (> 100 000 BYN). Продолжить?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return false;
            }

            string stockText = txtStock.Text?.Trim();
            if (string.IsNullOrWhiteSpace(stockText))
            {
                MessageBox.Show("Введите остаток товара.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStock.Focus();
                return false;
            }

            if (!int.TryParse(stockText, out stock))
            {
                MessageBox.Show("Остаток должен быть целым числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStock.Focus();
                return false;
            }

            if (stock < 0)
            {
                MessageBox.Show("Остаток не может быть отрицательным.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStock.Focus();
                return false;
            }

            return true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm(out string name, out decimal price, out int stock))
                return;

            _product.Name = name;
            _product.Price = price;
            _product.Stock = stock;

            try
            {
                if (_product.Id == 0)
                    _service.AddProduct(_product);
                else
                    _service.UpdateProduct(_product);
                DialogResult = true;
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                string message = dbEx.InnerException?.Message ?? dbEx.Message;
                if (message.Contains("Arithmetic overflow") || message.Contains("out of range"))
                    MessageBox.Show("Слишком большое число. Уменьшите цену или остаток.", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
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