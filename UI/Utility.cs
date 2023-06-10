using PaperSoccer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace UI;

public static class Utility
{
    public static bool PointNearbyOtherPoint(Point point, int maxDistance, Point second)
        => Math.Pow(point.x - second.x, 2) + Math.Pow(point.y - second.y, 2) <= Math.Pow(maxDistance, 2);


}

public class StrategyProvider
{
    public Array GetStrategies()
    {
        return Enum.GetValues<Strategy>();
    }
}
public static class EnumHelper
{
    /// <summary>
    /// Gets an attribute on an enum field value
    /// </summary>
    /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
    /// <param name="enumVal">The enum value</param>
    /// <returns>The attribute of type T that exists on the enum value</returns>
    public static T? GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
        return (attributes.Length > 0) ? (T)attributes[0] : null;
    }
}

public class StrategyConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value == null)
            return string.Empty;



        return ((Strategy[])value).Select(e => e.GetAttributeOfType<DisplayAttribute>()?.Name ?? string.Empty);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}