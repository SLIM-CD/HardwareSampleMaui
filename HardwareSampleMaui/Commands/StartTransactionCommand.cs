using System.Globalization;
using HardwareSampleMaui.Services;
using SlimCDTypeLib;

namespace HardwareSampleMaui.Commands;

public interface ITransactionParameters
{
    decimal Amount { get; }
    string Status { get; set; }
}

// The class needs to be marked as partial for trimming and AOT (Ahead-Of-Time) compatibility when passed across the WinRT ABI (Application Binary Interface).
// ReSharper disable once PartialTypeWithSinglePart
public partial class StartTransactionCommand(IDeviceService deviceService) : CommandBase
{
    private IDeviceService DeviceService { get; } = deviceService;

    public override async void Execute(object? parameter)
    {
        var executing = false;
        var executionSync = ExecutionSync;
        if (executionSync == null)
            return;

        try
        {
            executing = await executionSync.WaitAsync(0);
            if (!executing)
                return;

            OnStarting();

            CanBeExecuted = false;

            CanCancel = true;
            Cts = new CancellationTokenSource();

            var device = DeviceService.Device;
            if (device == null)
            {
                OnFinished(Response<object>.Failure("The device library is not found"));
                return;
            }

            if (parameter is not ITransactionParameters transactionParameters)
            {
                OnFinished(Response<object>.Failure("Invalid connection parameters"));
                return;
            }
            
            var config = GetTransactionConfig();
            var request = GetTransactionRequest(transactionParameters);

            using var replyAwaiter = new SemaphoreSlim(0, 1);
            DeviceTransactionReply? transactionReply = null;

            await device.DeviceProcessTransactionAsync(config, request, ReplyCallback, StatusCallback);
            
            try { await replyAwaiter.WaitAsync(Cts.Token); }
            catch (Exception ex)
            {
                OnFinished(Response<object>.Failure(ex.Message));
                return;
            }

            OnFinished(Response<object>.Success(transactionReply!));
            return;

            Task ReplyCallback(DeviceTransactionReply? reply)
            {
                transactionReply = reply;
                replyAwaiter.Release();
                return Task.CompletedTask;
            }

            Task StatusCallback(string status)
            {
                transactionParameters.Status = status;
                return Task.CompletedTask;
            }
        }
        catch (Exception ex)
        {
            OnFinished(Response<object>.Failure(ex.Message));
        }
        finally
        {
            if (executing)
            {
                CanCancel = false;
                Cts?.Dispose();
                Cts = null;

                CanBeExecuted = true;

                executionSync.Release();
            }
        }
    }

    private static DeviceTransactionConfig GetTransactionConfig()
    {
        return new DeviceTransactionConfig
        {
            cfg_tip = "0",
            cfg_PINdebit = "1",
            cfg_credit = "1",
            cfg_gift = "0",
            cfg_loyalty = "0",
            cfg_cashback = "0",
            cfg_surcharge = "1",
            cfg_convenienceFee = "0",
            cfg_convenienceFeeLabel = null,
            cfg_totals = "1",
            cfg_signature = "1",
            cfg_receipt = "0",
            cfg_EBT = "None",
            cfg_FoodStamps = "0",
            cfg_WIC = "0",
            cfg_emvEnabled = "1",
            cfg_swipeEnabled = "1",
            cfg_contactlessMode = "EMV",
            cfg_quickchip = "1",
            cfg_deferredAmount = "0",
            cfg_emvMerchantDecision = "0",
            cfg_emvConnectivityStatus = "0",
            cfg_swipeReceiptThreshold = "0",
            cfg_NOCVM_SignatureThreshold = "0.00",
            cfg_requireEncryption = "1",
            cfg_requestBinFlags = "0"
        };
    }

    private static DeviceTransactionRequest GetTransactionRequest(ITransactionParameters transactionParameters)
    {
        return new DeviceTransactionRequest
        {
            username = "1032",       // Test system public credentials
            password = "289075",
            clientid = 1032,
            siteid = 228226448,
            priceid = 74,
            product = "HardwareSample",
            version = "1.1.0",
            key = "",
            transtype = "Sale",
            authcode = "",
            clienttransref = "",
            gateid = 0,
            amount = transactionParameters.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            gratuity = "",
            surcharge = "",
            conveniencefee = "",
            allow_partial = "0",
            allow_duplicates = "0",
            emvmode = "",
            postype = "ECR",
            company = "",
            firstname = "",
            lastname = "",
            address = "",
            city = "",
            state = "",
            zip = "",
            country = "US",
            phone = "",
            email = "",
            corporatecard = "",
            po = "",
            salestax = "",
            salestaxtype = "0",
            clerkname = "CLERK1",
            deviceid = "11223344",
            comment = "",
            industry_specifics = new() { { "blind_credit", "1" } }
        };
    }
}