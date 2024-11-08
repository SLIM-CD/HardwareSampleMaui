using System.Net.Sockets;
using System.Net;
using System.Text;

namespace HardwareSampleMaui.Services;

public interface ILogger
{
    Task Write(string message);
}

public partial class LoggerTcpIpService(IPEndPoint address) : ILogger, IDisposable, IAsyncDisposable
{
    private IPEndPoint Address { get; } = address;
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

    #region Dispose Implementation
    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) 
            return;

        if (disposing)
        {
            ClientStream?.Dispose();
            Client?.Dispose();
        }

        ClientStream = null;
        Client = null;
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        
        if (ClientStream != null)
            await ClientStream.DisposeAsync();

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }
    #endregion
}