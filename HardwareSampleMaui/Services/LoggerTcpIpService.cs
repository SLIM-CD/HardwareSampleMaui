using System.Net.Sockets;
using System.Net;
using System.Text;

namespace HardwareSampleMaui.Services;

public interface ILogger
{
    Task Write(string message);
}

public class LoggerTcpIpService(IPEndPoint address) : ILogger, IDisposable, IAsyncDisposable
{
    private IPEndPoint Address { get; set; } = address;
    private TcpClient? Client { get; set; }
    private NetworkStream? ClientStream { get; set; }

    public async Task Write(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;
        if (!await EnsureConnection())
            return;

        var messageBytes = Encoding.UTF8.GetBytes(message);
        try
        {
            var clientStream = ClientStream;
            if (clientStream != null)
                await clientStream.WriteAsync(messageBytes, CancellationToken.None);
        }
        catch { await DisposeAsync(); }
    }

    private async Task<bool> EnsureConnection()
    {
        try
        {
            Client ??= new TcpClient();
            if (Client.Connected)
                return true;

            try { await Client.ConnectAsync(Address); }
            catch { return false; }

            for (var i = 0; !Client.Connected && i < 100; i++)
            {
                using var delayTask = Task.Delay(10);
                await delayTask;
            }

            ClientStream = Client.GetStream();
            return Client.Connected;
        }
        catch
        {
            await DisposeAsync();
            return false;
        }
    }

    public void Dispose()
    {
        var clientStream = ClientStream;
        if (clientStream != null)
        {
            try { clientStream.Dispose(); }
            catch { /* ignore */ }
            ClientStream = null;
        }

        var client = Client;
        if (client == null) 
            return;
        try { client.Dispose(); }
        catch { /* ignore */ }
        Client = null;
    }

    public async ValueTask DisposeAsync()
    {
        var client = Client;
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (client is IAsyncDisposable clientAsyncDisposable)
            await clientAsyncDisposable.DisposeAsync();
        else
            client?.Dispose();
        if (ClientStream != null) 
            await ClientStream.DisposeAsync();
    }
}