using System.Net.Sockets;

namespace EchoTcpServer;

public interface ITcpListener
{
    void Start();
    void Stop();
    Task<TcpClient> AcceptTcpClientAsync(CancellationToken token);
}