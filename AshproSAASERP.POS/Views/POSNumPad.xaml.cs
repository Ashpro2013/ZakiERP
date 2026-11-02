using System.Windows;
using System.Windows.Controls;

namespace AshproSAASERP.POS.Views;

public partial class POSNumPad : UserControl
{
    public static readonly RoutedEvent KeyClickEvent = EventManager.RegisterRoutedEvent(
        nameof(KeyClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(POSNumPad));

    public event RoutedEventHandler KeyClick
    {
        add => AddHandler(KeyClickEvent, value);
        remove => RemoveHandler(KeyClickEvent, value);
    }

    public POSNumPad()
    {
        InitializeComponent();
    }

    private void OnKeyClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string key)
        {
            var args = new RoutedEventArgs(KeyClickEvent, key);
            RaiseEvent(args);
        }
    }
}

