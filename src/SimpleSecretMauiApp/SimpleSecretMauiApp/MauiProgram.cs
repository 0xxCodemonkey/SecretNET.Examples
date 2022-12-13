using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SecretNET;
using SecretNET.Common;
using SecretNET.Common.Storage;
using SimpleSecretMauiApp.Pages;
using SimpleSecretMauiApp.ViewModel;

namespace SimpleSecretMauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        RegisterViewsAndViewModels(builder);

        return builder.Build();
	}

	public static void RegisterViewsAndViewModels(MauiAppBuilder builder)
	{
        // Setup Secret.NET client
        var gprcUrl = "https://grpc.testnet.secretsaturn.net"; // get from https://docs.scrt.network/secret-network-documentation/development/connecting-to-the-network
        var chainId = "pulsar-2";

        // client options
        var createClientOptions = new CreateClientOptions(gprcUrl, chainId, wallet: null) // init read-only instance (no wallet attached)
        {
            AlwaysSimulateTransactions = true, // WARNING: On mainnet it's recommended to not simulate every transaction as this can burden your node provider. 
        };

        var secretClient = new SecretNetworkClient(createClientOptions);

        // register the instance as singleton
        builder.Services.AddSingleton<SecretNetworkClient>(secretClient);

        // register the IPrivateKeyStorage instance
        builder.Services.AddSingleton<IPrivateKeyStorage>(new MauiSecureStorage());

        // register pages & view models
        builder.Services.AddSingleton<WalletPage>();
        builder.Services.AddSingleton<WalletViewModel>();

        builder.Services.AddSingleton<SmartContractPage>();
        builder.Services.AddSingleton<SmartContractViewModel>();
    }


}
