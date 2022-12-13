using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SecretNET;
using SecretNET.Tx;
using SimpleSecretMauiApp.Common;
using System.Diagnostics;

namespace SimpleSecretMauiApp.ViewModel;

public partial class SmartContractViewModel : ObservableObject
{
    private SecretNetworkClient _secretClient;
    private SmartContractInstance _smartContractInstance;
    private ulong _contractStoreCodeId = 17710;
    private string _contractInstanceInfoFile = "smartContractInstance.json";
    private string _contractCodeHash;

    [ObservableProperty]
    public string contractAddress = "-";

    [ObservableProperty]
    public int counter = 0;

    [ObservableProperty]
    private bool hasContract = false;

    public SmartContractViewModel(SecretNetworkClient secretClient)
    {
        _secretClient = secretClient;
        Init();
    }

    private async void Init()
    {
        var stored_Instance = await AppUtils.ReadFile(_contractInstanceInfoFile);
        if (!string.IsNullOrEmpty(stored_Instance))
        {
            _smartContractInstance = JsonConvert.DeserializeObject<SmartContractInstance>(stored_Instance);
            if (!String.IsNullOrWhiteSpace(_smartContractInstance.Address))
            {
                HasContract = true;
                ContractAddress = _smartContractInstance.Address;
                _contractCodeHash = _smartContractInstance.CodeHash;
            }
        }
    }

    [RelayCommand]
    public async void InitSmartContract()
    {
        if (!HasContract && _secretClient.Wallet != null)
        {
            try
            {
                _contractCodeHash = await _secretClient.Query.Compute.GetCodeHashByCodeId(_contractStoreCodeId);
                if (!String.IsNullOrWhiteSpace(_contractCodeHash))
                {
                    var msgInitContract = new SecretNET.Tx.MsgInstantiateContract(
                                        codeId: _contractStoreCodeId,
                                        label: $"SECRET.NET COUNTER {_contractStoreCodeId} #{new Random().Next(1, 999999)}", // must be unique
                                        initMsg: new { count = 1 },
                                        codeHash: _contractCodeHash); // optional but way faster

                    var initContractResponse = await _secretClient.Tx.Compute.InstantiateContract(msgInitContract);

                    if (initContractResponse.Code == 0 && initContractResponse?.Response?.Address != null)
                    {
                        ContractAddress = initContractResponse.Response.Address;
                        var instance = new SmartContractInstance()
                        {
                            Address = ContractAddress,
                            CodeHash = _contractCodeHash,
                        };
                        await AppUtils.WriteFile(_contractInstanceInfoFile, JsonConvert.SerializeObject(instance));
                        HasContract = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

    [RelayCommand]
    public async Task GetCounter()
    {
        if (!string.IsNullOrWhiteSpace(this.ContractAddress))
        {
            try
            {
                // Query
                var queryMsg = new { get_count = new { } };

                var queryContractResult = await _secretClient.Query.Compute.QueryContract<string>(contractAddress, queryMsg, _contractCodeHash);

                if (queryContractResult?.Response != null)
                {
                    // here you can also use dedicated DTOs for strongly typed deserialization instead of 'dynamic'
                    dynamic response = JObject.Parse(queryContractResult.Response);
                    Counter = (int)response.count;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

    [RelayCommand]
    public async Task IncrementCounter()
    {
        if (!string.IsNullOrWhiteSpace(this.ContractAddress))
        {
            try
            {
                // Execute
                var executeMsg = new { increment = new { } };

                var msgExecuteContract = new MsgExecuteContract(
                            contractAddress: contractAddress,
                            msg: executeMsg,
                            codeHash: _contractCodeHash);

                var executeContractResponse = await _secretClient.Tx.Compute.ExecuteContract(msgExecuteContract);

                if (executeContractResponse.Code == 0)
                {
                    Thread.Sleep(1000); // give some time to let it distribute

                    await GetCounter(); // get updated count
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

    [RelayCommand]
    public async Task ResetCounter()
    {
        if (!string.IsNullOrWhiteSpace(this.ContractAddress))
        {
            try
            {
                // Execute
                var executeMsg = new { reset = new { count = 0 } };

                var msgExecuteContract = new MsgExecuteContract(
                            contractAddress: contractAddress,
                            msg: executeMsg,
                            codeHash: _contractCodeHash);

                var executeContractResponse = await _secretClient.Tx.Compute.ExecuteContract(msgExecuteContract);

                if (executeContractResponse.Code == 0)
                {
                    Thread.Sleep(1000); // give some time to let it distribute

                    await GetCounter(); // get updated count
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
