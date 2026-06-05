using System.Windows;
using System.Windows.Controls;
using ClubArm.Models;
using ClubArm.Services;

namespace ClubArm.Views
{
    public partial class TariffEditorWindow : Window
    {
        private readonly ClubService _service;
        private readonly Tariff _tariff;

        public TariffEditorWindow(Tariff tariff, ClubService service)
        {
            InitializeComponent();
            _service = service;
            _tariff = tariff ?? new Tariff();
            if (tariff != null)
            {
                txtName.Text = tariff.Name;
                cmbType.Text = tariff.Type switch
                {
                    "Minute" => "Поминутный",
                    "Night" => "Ночной",
                    "Package" => "Пакетный",
                    _ => "Поминутный"
                };
                txtPricePerMinute.Text = tariff.PricePerMinute.ToString();
                txtPackageMinutes.Text = tariff.PackageMinutes?.ToString() ?? "";
                txtPackagePrice.Text = tariff.PackagePrice?.ToString() ?? "";
            }
            UpdatePackagePanelVisibility();
        }

        private void CmbType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdatePackagePanelVisibility();
        }

        private void UpdatePackagePanelVisibility()
        {
            bool isPackage = (cmbType.SelectedItem as ComboBoxItem)?.Content.ToString() == "Пакетный";
            pnlPackage.Visibility = isPackage ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool ValidatePricePerMinute(out decimal price)
        {
            price = 0;
            string input = txtPricePerMinute.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Цена за минуту не может быть пустой.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPricePerMinute.Focus();
                return false;
            }

            if (!decimal.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price))
            {
                MessageBox.Show("Введите корректное число (разделитель - точка).", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPricePerMinute.Focus();
                return false;
            }

            if (price <= 0)
            {
                MessageBox.Show("Цена за минуту должна быть больше 0.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPricePerMinute.Focus();
                return false;
            }

            if (price > 1000)
            {
                var result = MessageBox.Show("Цена за минуту очень высокая (> 1000 BYN). Продолжить?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            }

            return true;
        }
        private bool ValidatePackageFields(out int packageMinutes, out decimal packagePrice)
        {
            packageMinutes = 0;
            packagePrice = 0;

            if (pnlPackage.Visibility != Visibility.Visible)
                return true;

            string minutesInput = txtPackageMinutes.Text.Trim();
            if (string.IsNullOrWhiteSpace(minutesInput))
            {
                MessageBox.Show("Укажите количество минут в пакете.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPackageMinutes.Focus();
                return false;
            }

            if (!int.TryParse(minutesInput, out packageMinutes) || packageMinutes <= 0)
            {
                MessageBox.Show("Количество минут в пакете должно быть целым положительным числом.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPackageMinutes.Focus();
                return false;
            }

            string priceInput = txtPackagePrice.Text.Trim();
            if (string.IsNullOrWhiteSpace(priceInput))
            {
                MessageBox.Show("Укажите цену пакета.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPackagePrice.Focus();
                return false;
            }

            if (!decimal.TryParse(priceInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out packagePrice) || packagePrice <= 0)
            {
                MessageBox.Show("Цена пакета должна быть положительным числом.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPackagePrice.Focus();
                return false;
            }

            return true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название тарифа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (!ValidatePricePerMinute(out decimal pricePerMinute))
                return;

            if (!ValidatePackageFields(out int packageMinutes, out decimal packagePrice))
                return;

            if (cmbType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип тарифа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbType.Focus();
                return;
            }

            _tariff.Name = txtName.Text.Trim();
            string selectedType = ((ComboBoxItem)cmbType.SelectedItem).Content.ToString();
            _tariff.Type = selectedType switch
            {
                "Поминутный" => "Minute",
                "Ночной" => "Night",
                "Пакетный" => "Package",
                _ => "Minute"
            };
            _tariff.PricePerMinute = pricePerMinute;

            if (_tariff.Type == "Package")
            {
                _tariff.PackageMinutes = packageMinutes;
                _tariff.PackagePrice = packagePrice;
            }
            else
            {
                _tariff.PackageMinutes = null;
                _tariff.PackagePrice = null;
            }

            try
            {
                if (_tariff.Id == 0)
                    _service.AddTariff(_tariff);
                else
                    _service.UpdateTariff(_tariff);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}