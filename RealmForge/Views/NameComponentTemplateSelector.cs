using System.Windows;
using System.Windows.Controls;
using RealmForge.Models;

namespace RealmForge.Views;

/// <summary>
/// Selects the appropriate DataTemplate for NameComponentBase based on its actual type
/// </summary>
public class NameComponentTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NpcComponentTemplate { get; set; }
    public DataTemplate? ItemComponentTemplate { get; set; }
    public DataTemplate? DefaultComponentTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            NpcNameComponent => NpcComponentTemplate ?? DefaultComponentTemplate ?? base.SelectTemplate(item, container),
            ItemNameComponent => ItemComponentTemplate ?? DefaultComponentTemplate ?? base.SelectTemplate(item, container),
            _ => DefaultComponentTemplate ?? base.SelectTemplate(item, container)
        };
    }
}

/// <summary>
/// Selects the appropriate DataTemplate for NamePatternBase based on its actual type
/// </summary>
public class NamePatternTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NpcPatternTemplate { get; set; }
    public DataTemplate? ItemPatternTemplate { get; set; }
    public DataTemplate? DefaultPatternTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            NpcNamePattern => NpcPatternTemplate ?? DefaultPatternTemplate ?? base.SelectTemplate(item, container),
            ItemNamePattern => ItemPatternTemplate ?? DefaultPatternTemplate ?? base.SelectTemplate(item, container),
            _ => DefaultPatternTemplate ?? base.SelectTemplate(item, container)
        };
    }
}
