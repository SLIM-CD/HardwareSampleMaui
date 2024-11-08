using HardwareSampleMaui.Services;
using SlimCDTypeLib;

namespace HardwareSampleMaui.Commands;

public interface IConnectionParameters
{
    ConnectionType SelectedConnectionType { get; }
    ICommChannel? SelectedCommChannel { get; }
}

// The class needs to be marked as partial for trimming and AOT (Ahead-Of-Time) compatibility when passed across the WinRT ABI (Application Binary Interface).
public partial class ConnectCommand(IDeviceService deviceService) : CommandBase
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

            if (parameter is not IConnectionParameters connectionParameters)
            {
                OnFinished(Response<object>.Failure("Invalid connection parameters"));
                return;
            }
                
            var connectRequest = GetDeviceConnectRequest(connectionParameters, DeviceService.AssemblyInfo?.Name ?? "");
            var response = await device.DeviceConnectAsync(connectRequest, Cts.Token);

            OnFinished(response.IsSuccess
                ? Response<object>.Success(response.Data)
                : Response<object>.Failure(response.Message));
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

    private static DeviceConnectRequest GetDeviceConnectRequest(IConnectionParameters connectionParameters, string assemblyName)
    {
        var channelId = connectionParameters.SelectedCommChannel?.Id;
        var connectionType = connectionParameters.SelectedConnectionType;
        return connectionType switch
        {
            ConnectionType.TcpIp => CreateTcpIpConnectRequest(channelId, assemblyName),
            ConnectionType.Serial => CreateSerialConnectRequest(channelId, assemblyName),
            _ => CreateDefaultConnectRequest(connectionType, channelId, assemblyName)
        };
    }

    private static DeviceConnectRequest CreateTcpIpConnectRequest(string? channelId, string assemblyName)
    {
        var (address, port) = SplitAddressAndPort(channelId);
        return new DeviceConnectRequest
        {
            connectiontype = nameof(ConnectionType.TcpIp),
            ip = address,
            port = port,
            DLLclass = assemblyName
        };
    }

    private static (string address, int port) SplitAddressAndPort(string? addressAndPort)
    {
        if (string.IsNullOrWhiteSpace(addressAndPort))
            return ("", 0);
        var dividerOffset = addressAndPort.IndexOf(':');
        return dividerOffset switch
        {
            -1 => (addressAndPort, 0),
            _ => (addressAndPort[..dividerOffset], int.TryParse(addressAndPort[(dividerOffset + 1)..], out var parsedPort) ? parsedPort : 0)
        };
    }

    private static DeviceConnectRequest CreateSerialConnectRequest(string? channelId, string assemblyName)
    {
        return new DeviceConnectRequest
        {
            connectiontype = nameof(ConnectionType.Serial),
            commport = channelId,
            baudrate = 115200,
            databits = 8,
            stopbits = 1,
            parity = "None",
            flowcontrol = "None",
            DLLclass = assemblyName
        };
    }

    private static DeviceConnectRequest CreateDefaultConnectRequest(ConnectionType connectionType, string? channelId, string assemblyName)
    {
        return new DeviceConnectRequest
        {
            connectiontype = connectionType.ToString(),
            ip = channelId,
            DLLclass = assemblyName
        };
    }
}