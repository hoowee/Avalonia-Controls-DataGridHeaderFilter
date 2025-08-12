using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Collections;
using Huwei96.AvaloniaControls.DataGridHeaderFilter.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Huwei96.AvaloniaControls.DataGridHeaderFilter.ColumnsFilter;

public partial class PopupColumnFilter : Popup, INotifyPropertyChanged
{
    protected DataGridFilterHost? FilterHost { get; private set; }
    protected DataGrid? DataGrid { get; private set; }
    public string? PropertyPath { get; private set; }

    public ObservableCollection<ColumnFilter> ColumnFilters { get; private set; } = new();
    private ObservableCollection<ColumnFilter> AllItems { get; set; } = new();

    private Border? _popupBorderEl;
    private Grid? _popupContentGridEl;
    private Grid? _popupGridBtnEl;
    private TextBox? _searchTextBox;
    private CheckBox? _allCheckBox;
    private ListBoxItem? _contentAllViewEl;
    private ListBox? _checkedListView;
    private Button? _btnClear;
    private Button? _btnSelectAll;
    private Button? _btnInverse;
    private Button? _btnApply;

    private List<object>? _originalItems;
    private DataGridCollectionView? _collectionView;

    private string _allControlContent = "0";
    public string AllControlContent
    {
        get => _allControlContent;
        private set { if (_allControlContent != value) { _allControlContent = value; OnPropertyChanged(); } }
    }

    public PopupColumnFilter()
    {
        InitializeComponent();
        DataContext = this;
        WireUiOnce();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void WireUiOnce()
    {
        _popupBorderEl = this.FindControl<Border>("_popupBorder");
        _popupContentGridEl = this.FindControl<Grid>("_popupContentGrid");
        _popupGridBtnEl = this.FindControl<Grid>("_popupGridBtn");
        _searchTextBox = this.FindControl<TextBox>("searchTextBox");
        _allCheckBox = this.FindControl<CheckBox>("_AllCheckBox");
        _contentAllViewEl = this.FindControl<ListBoxItem>("_contentAllView");
        _checkedListView = this.FindControl<ListBox>("checkedListView");
        _btnClear = this.FindControl<Button>("btnClear");
        _btnSelectAll = this.FindControl<Button>("btnSelectAll");
        _btnInverse = this.FindControl<Button>("btnInverse");
        _btnApply = this.FindControl<Button>("btnApply");

        if (_searchTextBox != null)
            _searchTextBox.PropertyChanged += SearchBox_PropertyChanged;
        if (_allCheckBox != null)
            _allCheckBox.IsCheckedChanged += AllCheckBox_IsCheckedChanged;
        if (_contentAllViewEl != null)
            _contentAllViewEl.AddHandler(Control.PointerPressedEvent, OnAllItemPointer, RoutingStrategies.Tunnel);
        if (_checkedListView != null)
            _checkedListView.AddHandler(Control.PointerPressedEvent, OnListPointerPressed, RoutingStrategies.Tunnel);
        if (_btnClear != null) _btnClear.Click += (_, __) => ClearSelection();
        if (_btnSelectAll != null) _btnSelectAll.Click += (_, __) => SetAll(true);
        if (_btnInverse != null) _btnInverse.Click += (_, __) => InverseSelection();
        if (_btnApply != null) _btnApply.Click += (_, __) => CloseWithApply();
    }

    public void LoadFor(DataGrid grid, string propertyPath, IList<ColumnFilter> items)
    {
        DataGrid = grid;
        PropertyPath = propertyPath;
        if (_originalItems == null && grid.ItemsSource is IEnumerable src0)
            _originalItems = src0.Cast<object>().ToList();

        // 确保使用集合视图，这样列头排序始终可用
        try
        {
            if (grid.ItemsSource is DataGridCollectionView cv)
            {
                _collectionView = cv;
            }
            else if (grid.ItemsSource is IEnumerable srcItems)
            {
                _collectionView = new DataGridCollectionView(srcItems);
                grid.ItemsSource = _collectionView;
            }
        }
        catch { }
        AllItems.Clear();
        ColumnFilters.Clear();
        foreach (var it in items)
        {
            AllItems.Add(it);
            ColumnFilters.Add(it);
        }
        foreach (var it in AllItems) it.IsSelected = true;
        if (_allCheckBox != null) _allCheckBox.IsChecked = true;
        if (_contentAllViewEl != null) _contentAllViewEl.IsSelected = true;
        UpdateAllCheckBoxState();
        UpdateAllCount();
    }

    private void SearchBox_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == TextBox.TextProperty)
            OnSearchChanged(_searchTextBox?.Text);
    }

    private void OnSearchChanged(string? text)
    {
        var query = (text ?? string.Empty).Trim();
        var filtered = string.IsNullOrEmpty(query)
            ? AllItems
            : new ObservableCollection<ColumnFilter>(AllItems.Where(i => (i.Title ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase)));
        ColumnFilters.Clear();
        foreach (var it in filtered) ColumnFilters.Add(it);
        UpdateAllCheckBoxState();
        UpdateAllCount();
    }

    private void OnAllItemPointer(object? sender, PointerPressedEventArgs e)
    {
        AllViewToggle();
        e.Handled = true;
    }

    private void OnListPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        UpdateAllCheckBoxState();
        UpdateAllCount();
    }

    private void AllCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        var state = _allCheckBox?.IsChecked;
        if (state == true) SetAll(true);
        else if (state == false) SetAll(false);
        UpdateAllCheckBoxState();
        UpdateAllCount();
        if (_contentAllViewEl != null) _contentAllViewEl.IsSelected = (state == true);
    }

    private void AllViewToggle()
    {
        bool next = !(_contentAllViewEl?.IsSelected == true);
        SetAll(next);
        if (_allCheckBox != null) _allCheckBox.IsChecked = next;
        if (_contentAllViewEl != null) _contentAllViewEl.IsSelected = next;
        UpdateAllCheckBoxState();
        UpdateAllCount();
    }

    private void SetAll(bool value)
    {
        foreach (var it in AllItems) it.IsSelected = value;
        if (_searchTextBox != null && !string.IsNullOrWhiteSpace(_searchTextBox.Text))
        {
            _searchTextBox.Text = string.Empty;
            ColumnFilters.Clear();
            foreach (var item in AllItems) ColumnFilters.Add(item);
        }
    }

    private void ClearSelection()
    {
        // 数据重置：重建可选项并全部选中，清除过滤器，使弹出面板回到初始状态
        if (_searchTextBox != null)
            _searchTextBox.Text = string.Empty;

        IEnumerable<object>? source = null;
        if (_originalItems != null)
            source = _originalItems;
        else if (DataGrid?.ItemsSource is IEnumerable src)
            source = src.Cast<object>().ToList();

        if (source != null && !string.IsNullOrEmpty(PropertyPath))
        {
            RebuildFromSource(source);
        }

        // 全选
        foreach (var it in AllItems) it.IsSelected = true;
        ColumnFilters.Clear();
        foreach (var it in AllItems) ColumnFilters.Add(it);

        if (_allCheckBox != null) _allCheckBox.IsChecked = true;
        if (_contentAllViewEl != null) _contentAllViewEl.IsSelected = true;

        UpdateAllCheckBoxState();
        UpdateAllCount();

        // 清除集合视图筛选，保留排序
        if (_collectionView != null)
            _collectionView.Filter = null;
        else if (source != null)
            SetGridItems(source);

        // 通知宿主：当前无激活筛选
        try { FilterApplied?.Invoke(this, false); } catch { }
    }

    private void RestoreGridToOriginal()
    {
        try
        {
            if (DataGrid == null) return;
            // 优先通过集合视图清除筛选，保持排序可用
            if (_collectionView != null)
            {
                _collectionView.Filter = null;
                return;
            }
            // 兜底：如果没有集合视图，再回退到直接设置 ItemsSource
            if (_originalItems != null) { SetGridItems(_originalItems); return; }
            if (DataGrid.ItemsSource is IEnumerable src) { SetGridItems(src.Cast<object>().ToList()); }
        }
        catch { }
    }

    private void RebuildFromSource(IEnumerable<object> source)
    {
        if (string.IsNullOrEmpty(PropertyPath))
        {
            ColumnFilters.Clear();
            foreach (var o in source) { }
            return;
        }
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in source)
        {
            var val = GetPropertyValue(item, PropertyPath!)?.ToString() ?? string.Empty;
            if (map.ContainsKey(val)) map[val]++;
            else map[val] = 1;
        }
        AllItems.Clear();
        ColumnFilters.Clear();
        foreach (var kv in map.OrderBy(k => k.Key))
        {
            var cf = new ColumnFilter(false, kv.Key, kv.Value);
            AllItems.Add(cf);
            ColumnFilters.Add(cf);
        }
    }

    private void InverseSelection()
    {
        foreach (var it in AllItems) it.IsSelected = !it.IsSelected;
        if (_searchTextBox != null && !string.IsNullOrWhiteSpace(_searchTextBox.Text))
        {
            _searchTextBox.Text = string.Empty;
            ColumnFilters.Clear();
            foreach (var item in AllItems) ColumnFilters.Add(item);
        }
        UpdateAllCheckBoxState();
        UpdateAllCount();
    }

    private void CloseWithApply()
    {
        try
        {
            if (DataGrid == null)
            {
                IsOpen = false; return;
            }

            if (_originalItems == null)
            {
                if (DataGrid.ItemsSource is IEnumerable src)
                    _originalItems = src.Cast<object>().ToList();
            }

            var selected = GetSelectedItems();
            if (selected == null || selected.Count == 0)
            {
                if (_collectionView != null)
                {
                    _collectionView.Filter = null;
                }
                else if (_originalItems != null)
                {
                    SetGridItems(_originalItems);
                }
                // 无筛选
                try { FilterApplied?.Invoke(this, false); } catch { }
            }
            else
            {
                var set = selected.Select(x => x.Title).ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (_collectionView != null)
                {
                    _collectionView.Filter = (obj) =>
                    {
                        var val = GetPropertyValue(obj!, PropertyPath ?? string.Empty)?.ToString() ?? string.Empty;
                        return set.Contains(val);
                    };
                }
                else
                {
                    // 兜底：没有集合视图时，仍采用替换 ItemsSource
                    var filtered = (_originalItems ?? new List<object>()).Where(it =>
                    {
                        var val = GetPropertyValue(it, PropertyPath ?? string.Empty)?.ToString() ?? string.Empty;
                        return set.Contains(val);
                    }).ToList();
                    SetGridItems(filtered);
                }

                // 判断是否为“等同全选”的情况，如果等同全选则视为无筛选
                bool isAllSelected = selected.Count == (AllItems?.Count ?? selected.Count);
                bool hasActive = !isAllSelected;
                try { FilterApplied?.Invoke(this, hasActive); } catch { }
            }
        }
        finally
        {
            IsOpen = false;
        }
    }

    private void SetGridItems(IEnumerable<object> items)
    {
        if (DataGrid == null) return;
        try
        {
            // 使用 DataGridCollectionView 包装集合，确保开启列头排序能力
            var view = new DataGridCollectionView(items);
            _collectionView = view;
            DataGrid.ItemsSource = view;
        }
        catch
        {
            // 兜底：若类型不可用，仍直接赋值
            DataGrid.ItemsSource = items;
        }
    }

    private static object? GetPropertyValue(object obj, string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        object? current = obj;
        foreach (var name in path.Split('.'))
        {
            if (current == null) return null;
            var prop = current.GetType().GetProperty(name);
            if (prop == null) return null;
            current = prop.GetValue(current);
        }
        return current;
    }

    private void UpdateAllCheckBoxState()
    {
        if (_allCheckBox == null) return;
        if (AllItems.Count == 0) { _allCheckBox.IsChecked = false; return; }
        var all = AllItems.All(c => c.IsSelected);
        var any = AllItems.Any(c => c.IsSelected);
        _allCheckBox.IsChecked = all ? true : any ? (bool?)null : false;
    }

    private void UpdateAllCount()
    {
        AllControlContent = (AllItems?.Count ?? 0).ToString();
    }

    public List<ColumnFilter>? GetSelectedItems()
    {
        var list = AllItems.Where(i => i.IsSelected).ToList();
        return list.Count == 0 ? null : list;
    }

    public void SetSelectedItems(List<ColumnFilter>? items)
    {
        if (items == null || items.Count == 0)
        {
            foreach (var it in AllItems) it.IsSelected = false;
        }
        else
        {
            var set = items.Select(x => x.Title).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var it in AllItems) it.IsSelected = set.Contains(it.Title);
        }
        UpdateAllCheckBoxState();
        UpdateAllCount();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // 当应用筛选或清除时触发；参数表示是否存在激活的筛选（非全选且非清空）
    public event EventHandler<bool>? FilterApplied;
}
