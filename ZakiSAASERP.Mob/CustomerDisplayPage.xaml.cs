namespace ZakiSAASERP.Mob;

public partial class CustomerDisplayPage : ContentPage
{
    public CustomerDisplayPage(CustomerDisplayViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}
