using System.Net.Sockets;

namespace EchoTcpServer;

public class EchoServer
{
    private readonly ITcpListener _listener;
    private readonly Action<string> _log;
    private readonly CancellationTokenSource _cts = new();

    public EchoServer(ITcpListener listener, Action<string>? log = null)
    {
        _listener = listener;
        _log = log ?? Console.WriteLine;
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _log($"Server started.");

        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(_cts.Token);
                _log("Client connected.");
                _ = Task.Run(() => HandleClientAsync(client, _cts.Token));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }

        _log("Server shutdown.");
    }

    internal async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using NetworkStream stream = client.GetStream();
        try
        {
            await HandleStreamAsync(stream, token);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log($"Error: {ex.Message}");
        }
        finally
        {
            client.Close();
            _log("Client disconnected.");
        }
    }

    internal async Task HandleStreamAsync(Stream stream, CancellationToken token)
    {
        byte[] buffer = new byte[8192];
        int bytesRead;
        while (!token.IsCancellationRequested &&
               (bytesRead = await stream.ReadAsync(buffer, token)) > 0)
        {
            await stream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
            _log($"Echoed {bytesRead} bytes.");
        }
    }

    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
        _cts.Dispose();
        _log("Server stopped.");
    }
}