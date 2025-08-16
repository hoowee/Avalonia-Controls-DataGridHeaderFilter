using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using System.Collections.Generic;
using System.Reflection;
using Avalonia.Styling;

namespace Huwei96.AvaloniaControls.DataGridHeaderFilter.ColumnsFilter;

public class DataGridFilterColumnControl : TemplatedControl
{
    private Button? PART_ColumnIdBtn;
    private Popup? PART_PopupFilterControl;
    private PopupColumnFilter? _popupRef;

    public static readonly StyledProperty<IBrush?> FillColorProperty =
        AvaloniaProperty.Register<DataGridFilterColumnControl, IBrush?>(nameof(FillColor), new SolidColorBrush(Color.Parse("#606060")));
    public IBrush? FillColor
    {
        get => GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    public static readonly StyledProperty<bool?> IsAllCheckedProperty =
        AvaloniaProperty.Register<DataGridFilterColumnControl, bool?>(nameof(IsAllChecked), false);
    public bool? IsAllChecked
    {
        get => GetValue(IsAllCheckedProperty);
        set => SetValue(IsAllCheckedProperty, value);
    }

    public static readonly StyledProperty<string?> PropertyPathProperty =
        AvaloniaProperty.Register<DataGridFilterColumnControl, string?>(nameof(PropertyPath));
    public string? PropertyPath
    {
        get => GetValue(PropertyPathProperty);
        set => SetValue(PropertyPathProperty, value);
    }

    // 是否存在激活的筛选（非全选且非清空）
    public static readonly StyledProperty<bool> HasActiveFilterProperty =
        AvaloniaProperty.Register<DataGridFilterColumnControl, bool>(nameof(HasActiveFilter), false);
    public bool HasActiveFilter
    {
        get => GetValue(HasActiveFilterProperty);
        set => SetValue(HasActiveFilterProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PART_ColumnIdBtn = e.NameScope.Find<Button>("PART_ColumnIdBtn");
        PART_PopupFilterControl = e.NameScope.Find<Popup>("PART_PopupFilterControl");
        // 解除旧模板事件订阅
        if (_popupRef != null)
        {
            try { _popupRef.FilterApplied -= OnFilterApplied; } catch { }
        }
        _popupRef = e.NameScope.Find<PopupColumnFilter>("PART_PopupFilterControl");
        if (_popupRef != null)
        {
            try { _popupRef.FilterApplied += OnFilterApplied; } catch { }
        }
        if (PART_ColumnIdBtn != null)
        {
            PART_ColumnIdBtn.Click += (_, _) =>
            {
                if (PART_PopupFilterControl != null)
                {
                    if (PART_PopupFilterControl.PlacementTarget is null)
                        PART_PopupFilterControl.PlacementTarget = PART_ColumnIdBtn;
                    try { PART_PopupFilterControl.Placement = PlacementMode.Bottom; } catch { }

                    var popup = PART_PopupFilterControl as PopupColumnFilter;
                    var grid = this.FindAncestorOfType<DataGrid>();
                    if (popup != null && grid != null && !string.IsNullOrEmpty(PropertyPath))
                    {
                        var items = Ext.BuildColumnItems(grid, PropertyPath!);
                        popup.LoadFor(grid, PropertyPath!, items);
                    }
                    PART_PopupFilterControl.IsOpen = !PART_PopupFilterControl.IsOpen;
                }
            };
        }
    }

    private void OnFilterApplied(object? sender, bool hasActive)
    {
        HasActiveFilter = hasActive;
        // 主题高亮色优先，其次回退固定颜色
        if (hasActive)
        {
            if (TryGetAccentBrush(out var accent))
                FillColor = accent;
            else
                FillColor = new SolidColorBrush(Color.Parse("#2D7FF9"));
        }
        else
        {
            FillColor = new SolidColorBrush(Color.Parse("#606060"));
        }
    }

    private static bool TryGetAccentBrush(out IBrush brush)
    {
        brush = new SolidColorBrush(Color.Parse("#2D7FF9"));
        try
        {
            var app = Application.Current;
            if (app != null && app.TryFindResource("ThemeAccentBrush", app.ActualThemeVariant, out var res) && res is IBrush b)
            {
                brush = b; return true;
            }
        }
        catch { }
        return false;
    }
}

public static class Ext
{
    private static object? GetPropertyValue(object obj, string path)
    {
        object? current = obj;
        foreach (var name in path.Split('.'))
        {
            if (current == null) return null;
            var prop = current.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) return null;
            current = prop.GetValue(current);
        }
        return current;
    }

    internal static List<Avalonia.Controls.DataGridHeaderFilter.Models.ColumnFilter> BuildColumnItems(DataGrid grid, string propertyPath)
    {
        var result = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        if (grid.ItemsSource is System.Collections.IEnumerable src)
        {
            foreach (var item in src)
            {
                var val = GetPropertyValue(item!, propertyPath)?.ToString() ?? string.Empty;
                if (result.ContainsKey(val)) result[val]++;
                else result[val] = 1;
            }
        }
    var list = new List<Avalonia.Controls.DataGridHeaderFilter.Models.ColumnFilter>();
        foreach (var kv in result)
            list.Add(new Avalonia.Controls.DataGridHeaderFilter.Models.ColumnFilter(false, kv.Key, kv.Value));
        list.Sort((a,b) => string.Compare(a.Title, b.Title, System.StringComparison.OrdinalIgnoreCase));
        return list;
    }
}
