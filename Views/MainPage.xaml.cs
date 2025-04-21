using TuringMachineApp.ViewModels;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace TuringMachineApp.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel viewModel;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = viewModel = new MainViewModel();
        ModePicker.SelectedIndex = 0; // Устанавливаем режим по умолчанию
    }

    private void OnModeChanged(object sender, EventArgs e)
    {
        if (viewModel == null || !(sender is Picker picker)) return;

        viewModel.CurrentMode = picker.SelectedItem?.ToString();
        viewModel.ResetMachine();
    }
}