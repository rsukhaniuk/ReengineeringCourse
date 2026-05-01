using System.Net;
using System.Net.Sockets;

namespace EchoTcpServer;

public class TcpListenerAdapter : ITcpListener
{
    private readonly TcpListener _listener;

    public TcpListenerAdapter(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public void Start() => _listener.Start();

    public void Stop() => _listener.Stop();

    public Task<TcpClient> AcceptTcpClientAsync(CancellationToken token) =>
        _listener.AcceptTcpClientAsync(token).AsTask();
}