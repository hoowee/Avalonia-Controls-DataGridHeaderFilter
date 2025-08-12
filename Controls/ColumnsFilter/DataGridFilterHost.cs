using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls.DataGridHeaderFilter.Models;

namespace Avalonia.Controls.DataGridHeaderFilter.ColumnsFilter;

public sealed class DataGridFilterHost
{
    private readonly List<object> _filterColumnControls = new();
    public List<ColumnFilterGroups> FilterGroups { get; private set; } = new List<ColumnFilterGroups>();

    public DataGridFilterHost() { Deserialize(); }

    private void Serialize()
    {
        try
        {
            foreach (dynamic itemControl in _filterColumnControls)
            {
                List<ColumnFilter> columnFilters = itemControl.GetSelectedItems();
                if (columnFilters != null)
                {
                    var list = columnFilters.Where(x => x.Tag != ColumnFilterTag.All).ToList();
                    if (list?.Count > 0)
                    {
                        var first = FilterGroups.FirstOrDefault(x => x.SortMemberPath == itemControl.PropertyPath);
                        if (first == null)
                        {
                            ColumnFilterGroups groups = new ColumnFilterGroups
                            {
                                SortMemberPath = itemControl.PropertyPath,
                                ColumnFilters = list
                            };
                            FilterGroups.Add(groups);
                        }
                        else
                        {
                            first.ColumnFilters = list;
                        }
                    }
                }
                else
                {
                    var first = FilterGroups.FirstOrDefault(x => x.SortMemberPath == itemControl.PropertyPath);
                    if (first != null)
                        FilterGroups.Remove(first);
                }
            }

            if (FilterGroups.Count > 0)
            {
                var obj = XmlSerializerExtension.Serialize(FilterGroups, ".\\DataGridColumnFilter.config");
            }
            else
            {
                if (File.Exists(".\\DataGridColumnFilter.config"))
                {
                    File.Delete(".\\DataGridColumnFilter.config");
                }
            }
        }
        catch (Exception)
        {
            // ignore
        }
    }

    private void Deserialize()
    {
        try
        {
            if (File.Exists(".\\DataGridColumnFilter.config"))
            {
                var obj = XmlSerializerExtension.Deserialize<List<ColumnFilterGroups>>(".\\DataGridColumnFilter.config");
                if (obj != null)
                {
                    if (obj.Count > 0)
                    {
                        FilterGroups.Clear();
                        FilterGroups.AddRange(obj);
                    }
                    else
                    {
                        FilterGroups.Clear();
                    }
                }
            }
        }
        catch (Exception)
        {
            File.Delete(".\\DataGridColumnFilter.config");
        }
    }

    internal void Enable(bool value)
    {
        if (value && _filterColumnControls?.Any() == true)
            Filter();
    }

    internal void AddColumnControl(object dataGridFilterColumn)
    {
        var key = ((dynamic)dataGridFilterColumn).PropertyPath as string;
        var item = FilterGroups.FirstOrDefault(x => x.SortMemberPath == key);
        ((dynamic)dataGridFilterColumn).SetSelectedItems(item?.ColumnFilters);
        if (_filterColumnControls.FirstOrDefault(x => x == dataGridFilterColumn) == null)
            _filterColumnControls.Add(dataGridFilterColumn);
    }

    internal void Filter() { }

    private Func<object, bool> CreatePredicate(List<object> columnControls)
    {
        if (columnControls == null || columnControls.Count <= 0)
        {
            return item => true;
        }
        if (!columnControls.Any())
        {
            return item => columnControls.All(filter => true);
        }
        return item => columnControls.All(filter => ((dynamic)filter).Matches(item));
    }
}
