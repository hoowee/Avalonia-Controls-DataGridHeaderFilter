# Huwei96.AvaloniaControls.DataGridHeaderFilter 使用指南

本指南介绍如何在 Avalonia 11 应用中集成并使用 DataGrid 列头筛选组件（Huwei96.AvaloniaControls.DataGridHeaderFilter）。

## 前提

- Avalonia 11.x
- 已将本库添加为引用（项目引用或打包后引用）

常用命名空间：

- DataGrid：`xmlns:controls="clr-namespace:Avalonia.Controls;assembly=Avalonia.Controls.DataGrid"`
- 筛选控件：`xmlns:filter="clr-namespace:Huwei96.AvaloniaControls.DataGridHeaderFilter.ColumnsFilter;assembly=Huwei96.AvaloniaControls.DataGridHeaderFilter"`

## 1) 在 App.axaml 引入样式

引入库的主题样式，自动为 DataGridColumnHeader 注入筛选按钮，并为右侧排序箭头预留区域。

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Application.Styles>
    <FluentTheme />
    <!-- 引入库的默认样式 -->
  <StyleInclude Source="avares://Huwei96.AvaloniaControls.DataGridHeaderFilter/Themes/Generic.axaml" />
  </Application.Styles>
</Application>
```

> 如果你只想在个别 DataGrid 使用筛选，可以不全局引入上面样式，改用“手动在列头放置筛选控件”的方式（见 3B）。

## 2) ViewModel/数据源

Items 建议使用 `ObservableCollection<T>`。库在筛选时会自动使用 `DataGridCollectionView` 包装集合，以确保列头排序可用。

```csharp
public ObservableCollection<MyRow> Items { get; } = new();
```

## 3A) 方式一：自动注入筛选按钮（零改列定义）

引入样式后，所有 DataGrid 列头会自动显示筛选按钮，无需更改列的 Header：

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="clr-namespace:Avalonia.Controls;assembly=Avalonia.Controls.DataGrid">
  <controls:DataGrid ItemsSource="{Binding Items}"
                     AutoGenerateColumns="False"
                     Height="400">
    <controls:DataGrid.Columns>
      <controls:DataGridTextColumn Header="ClientId" Binding="{Binding ClientId}" />
      <controls:DataGridTextColumn Header="FundAccount" Binding="{Binding FundAccount}" />
      <controls:DataGridTextColumn Header="TradeSymbol" Binding="{Binding TradeSymbol}" />
    </controls:DataGrid.Columns>
  </controls:DataGrid>
</Window>
```

自动注入模式下，筛选字段路径取自列绑定表达式（上例分别为 `ClientId`、`FundAccount`、`TradeSymbol`）。

## 3B) 方式二：手动在列头放置筛选控件（精确控制）

如果你想逐列控制或使用自定义字段路径，可在该列 Header 中手动放置筛选控件，并指定 `PropertyPath`：

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="clr-namespace:Avalonia.Controls;assembly=Avalonia.Controls.DataGrid"
  xmlns:filter="clr-namespace:Huwei96.AvaloniaControls.DataGridHeaderFilter.ColumnsFilter;assembly=Huwei96.AvaloniaControls.DataGridHeaderFilter">
  <controls:DataGrid ItemsSource="{Binding Items}" AutoGenerateColumns="False">
    <controls:DataGrid.Columns>
      <controls:DataGridTextColumn>
        <controls:DataGridTextColumn.Header>
          <StackPanel Orientation="Horizontal" Spacing="6">
            <TextBlock Text="ClientId" />
            <!-- 指定筛选字段路径，支持嵌套路径如 Customer.Name -->
            <filter:DataGridFilterColumnControl PropertyPath="ClientId" />
          </StackPanel>
        </controls:DataGridTextColumn.Header>
        <controls:DataGridTextColumn.Binding>
          <Binding Path="ClientId" />
        </controls:DataGridTextColumn.Binding>
      </controls:DataGridTextColumn>
      <!-- 其他列同理 -->
    </controls:DataGrid.Columns>
  </controls:DataGrid>
</Window>
```

> 若同时使用了全局样式注入和手动放置，注意避免同一列重复出现按钮；通常二选一即可。

## 4) 交互与行为说明

- 排序：筛选开启/关闭后，列头排序仍然可用（内部使用 `DataGridCollectionView.Filter`，不会破坏排序管道）。
- 数据重置：“数据重置”按钮会清空筛选、重建候选项，并“全部选中”，与首次打开弹出面板时一致。
- 视觉提示：有筛选时列头图标将以主题高亮显示，并出现右上角小圆点徽标。
- 弹出面板：支持搜索、行点击切换勾选、三态全选（全选/部分/全不选）、全选/反选/数据重置/应用等操作。

## 5) 常见问题

- 看不到筛选按钮：
  - 确认已在 App.axaml 引入 `Generic.axaml` 样式；或在列头手动放置筛选控件。
- 排序失效：
  - 库已保证筛选与排序兼容。如仍遇到，请确认外部代码没有把 `ItemsSource` 直接替换为 `List`；推荐始终使用 `DataGridCollectionView`。
- 绑定嵌套属性：
  - `PropertyPath` 支持嵌套路径（例如 `Customer.Name`）。

## 6) 可定制项

- 组件属性：
  - `PropertyPath`（手动模式）：指定当前列用于筛选的属性路径。
  - `HasActiveFilter`（只读）：是否存在激活筛选；可用于样式触发。
- 样式覆盖：
  - 你可以在应用侧覆盖控件模板、按钮大小、图标颜色、徽标样式等（见库内 `Themes/DataGridFilterColumnControl.axaml`）。

## 7) 最小可运行示例（整合）

```xml
<!-- App.axaml -->
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Application.Styles>
    <FluentTheme />
  <StyleInclude Source="avares://Huwei96.AvaloniaControls.DataGridHeaderFilter/Themes/Generic.axaml" />
  </Application.Styles>
</Application>
```

```xml
<!-- MainWindow.axaml -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="clr-namespace:Avalonia.Controls;assembly=Avalonia.Controls.DataGrid">
  <controls:DataGrid ItemsSource="{Binding Items}" AutoGenerateColumns="False" Height="400">
    <controls:DataGrid.Columns>
      <controls:DataGridTextColumn Header="ClientId" Binding="{Binding ClientId}" />
      <controls:DataGridTextColumn Header="FundAccount" Binding="{Binding FundAccount}" />
      <controls:DataGridTextColumn Header="TradeSymbol" Binding="{Binding TradeSymbol}" />
    </controls:DataGrid.Columns>
  </controls:DataGrid>
</Window>
```

```csharp
// ViewModel.cs
public ObservableCollection<MyRow> Items { get; } = new ObservableCollection<MyRow>(
    Enumerable.Range(1, 500).Select(i => new MyRow {
        ClientId = $"C{i:000}",
        FundAccount = $"F{i%5}",
        TradeSymbol = new[]{"AAPL","MSFT","NVDA","AMZN","TSLA"}[i%5]
    })
);
```

---

