using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecretNET;
using SecretNET.Common;
using SecretNET.Common.Storage;
using System.Diagnostics;

namespace SimpleSecretMauiApp.ViewModel;

public partial class WalletViewModel : ObservableObject
{
    private SecretNetworkClient _secretClient;
    private IPrivateKeyStorage _privateKeyStorage;
    private CreateWalletOptions _createWalletOptions;

    [ObservableProperty]
    private string address = "-";

    [ObservableProperty]
    private float balance = 0;

    [ObservableProperty]
    private bool hasWallet = false;

    public WalletViewModel(SecretNetworkClient secretClient, IPrivateKeyStorage privateKeyStorage)
    {
        _secretClient = secretClient;
        _privateKeyStorage = privateKeyStorage;
        _createWalletOptions = new CreateWalletOptions(_privateKeyStorage);
        Init();
    }

    private async void Init()
    {
        if (await _privateKeyStorage.HasPrivateKey())
        {
            var wallet = await Wallet.Create(await _privateKeyStorage.GetFirstMnemonic(), options: _createWalletOptions);

            // attach the wallet to the secretClient instance
            _secretClient.Wallet = wallet;
            Address = wallet.Address;
            HasWallet = true;
            Debug.WriteLine("Existing address: " + Address);
            GetBalance();
        }
    }

    [RelayCommand]
    public async void CreateWallet()
    {
        if (!HasWallet)
        {
            var wallet = await Wallet.Create(options: _createWalletOptions);
            _secretClient.Wallet = wallet;
            Address = wallet.Address;
            HasWallet = true;
            Debug.WriteLine("New address: " + Address);
        }            
    }

    [RelayCommand]
    public async void GetBalance()
    {
        if (HasWallet)
        {
            try
            {
                var balanceResponse = await _secretClient.Query.Bank.Balance(Address);
                if (balanceResponse?.Amount != null)
                {
                    float amount;
                    if (float.TryParse(balanceResponse.Amount, out amount))
                    {
                        Balance = amount / 1000000;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
