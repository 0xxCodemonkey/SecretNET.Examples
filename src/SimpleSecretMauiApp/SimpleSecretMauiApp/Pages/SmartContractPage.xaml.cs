using SimpleSecretMauiApp.ViewModel;

namespace SimpleSecretMauiApp.Pages;

public partial class SmartContractPage : ContentPage
{
    public SmartContractPage(SmartContractViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}