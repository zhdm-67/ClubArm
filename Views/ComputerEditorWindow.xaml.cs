using ClubArm.Models;
using ClubArm.Services;
using System.Windows;
using System.Windows.Controls;

namespace ClubArm.Views
{
    public partial class ComputerEditorWindow : Window
    {
        private readonly ClubService _service;
        private readonly Computer _computer;

        public ComputerEditorWindow(Computer computer, ClubService service)
        {
            InitializeComponent();
            _service = service;
            _computer = computer ?? new Computer();
            if (computer != null)
            {
                txtName.Text = computer.Name;
                cmbStatus.Text = computer.Status;
                txtConfig.Text = computer.Configuration;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _computer.Name = txtName.Text;
            _computer.Status = ((ComboBoxItem)cmbStatus.SelectedItem)?.Content.ToString() ?? "Free";
            _computer.Configuration = txtConfig.Text;
            if (_computer.Id == 0)
                _service.AddComputer(_computer);
            else
                _service.UpdateComputer(_computer);
            DialogResult = true;
            Close();
        }
    }
}