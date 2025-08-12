using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Huwei96.AvaloniaControls.DataGridHeaderFilter.Models;

public enum ColumnFilterTag
{
    All,
    Single
}

public class ColumnFilter : INotifyPropertyChanged
{
    private bool _isSelected;
    private string _title = string.Empty;
    private int _count;
    private ColumnFilterTag _tag;

    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
    }

    public string Title
    {
        get => _title;
        set { if (_title != value) { _title = value; OnPropertyChanged(); } }
    }

    public int Count
    {
        get => _count;
        set { if (_count != value) { _count = value; OnPropertyChanged(); } }
    }

    public ColumnFilterTag Tag
    {
        get => _tag;
        set { if (_tag != value) { _tag = value; OnPropertyChanged(); } }
    }

    public ColumnFilter() { }
    public ColumnFilter(bool isSelected, string title, int count, ColumnFilterTag tag = ColumnFilterTag.Single)
    {
        _isSelected = isSelected;
        _title = title;
        _count = count;
        _tag = tag;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
