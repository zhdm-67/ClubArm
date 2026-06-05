// ViewModels/MainViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClubArm.Models;
using ClubArm.Services;

namespace ClubArm.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ClubService _service;
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }
        public ICommand ShowComputersCommand { get; }
        public ICommand ShowClientsCommand { get; }
        public ICommand ShowTariffsCommand { get; }
        public ICommand ShowProductsCommand { get; }
        public ICommand ShowReportsCommand { get; }

        public MainViewModel()
        {
            _service = new ClubService();
            ShowComputersCommand = new RelayCommand(_ => CurrentView = new ComputersViewModel(_service));
            ShowClientsCommand = new RelayCommand(_ => CurrentView = new ClientsViewModel(_service));
            ShowTariffsCommand = new RelayCommand(_ => CurrentView = new TariffsViewModel(_service));
            ShowProductsCommand = new RelayCommand(_ => CurrentView = new ProductsViewModel(_service));
            ShowReportsCommand = new RelayCommand(_ => CurrentView = new ReportsViewModel(_service));
            CurrentView = new ComputersViewModel(_service);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}