using System.Collections;
using Avalonia;
using Avalonia.Controls;

namespace Huwei96.AvaloniaControls.DataGridHeaderFilter.ColumnsFilter;

public class DataGridFilter : AvaloniaObject
{
    public static readonly AttachedProperty<IEnumerable?> CurrentDataGridItemSourceProperty =
        AvaloniaProperty.RegisterAttached<DataGridFilter, Control, IEnumerable?>("CurrentDataGridItemSource");

    public static IEnumerable? GetCurrentDataGridItemSource(Control obj)
        => obj.GetValue(CurrentDataGridItemSourceProperty);
    public static void SetCurrentDataGridItemSource(Control obj, IEnumerable? value)
        => obj.SetValue(CurrentDataGridItemSourceProperty, value);

    public static readonly AttachedProperty<DataGridFilterHost?> FilterProperty =
        AvaloniaProperty.RegisterAttached<DataGridFilter, Control, DataGridFilterHost?>("Filter");

    public static DataGridFilterHost? GetFilter(Control obj)
        => obj.GetValue(FilterProperty);
    public static void SetFilter(Control obj, DataGridFilterHost? value)
        => obj.SetValue(FilterProperty, value);
}
