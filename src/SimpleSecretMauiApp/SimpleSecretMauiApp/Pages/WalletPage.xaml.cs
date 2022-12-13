using SimpleSecretMauiApp.ViewModel;

namespace SimpleSecretMauiApp.Pages;

public partial class WalletPage : ContentPage
{

	public WalletPage(WalletViewModel viewModel)
	{
        BindingContext = viewModel;
        InitializeComponent();
	}
}