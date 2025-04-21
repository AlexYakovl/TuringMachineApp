using System.Globalization;
using TuringMachineApp.Models;
using TuringMachineApp.ViewModels;

namespace TuringMachineApp.Converters;
public class HeadConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TapeCell cell)
        {
            var currentPage = Shell.Current?.CurrentPage;

            if (currentPage?.BindingContext is MainViewModel vm)
            {
                return vm.HeadPosition == cell.Index
                    ? Colors.Red
                    : Colors.Transparent;
            }
        }

        return Colors.Transparent;
    }



    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
