using System.Collections.Generic;

namespace Huwei96.AvaloniaControls.DataGridHeaderFilter.Models;

public class ColumnFilterGroups
{
    public string? SortMemberPath { get; set; }
    public List<ColumnFilter> ColumnFilters { get; set; } = new List<ColumnFilter>();
}
