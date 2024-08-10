using Kuti.Windows.Settings.Pages.PinnedApps;
using System.Globalization;
using System.Windows.Data;

namespace Kuti.Windows.Settings.Utils;

public class TupleConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is PinnableProcess && values[1] is PinnableDesktop)
        {
            return Tuple.Create((PinnableProcess)values[0], (PinnableDesktop)values[1]);
        }
        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
