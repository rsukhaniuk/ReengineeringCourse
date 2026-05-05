using System.Net;
using System.Net.Sockets;

namespace EchoTcpServer;

public sealed class UdpTimedSender : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly UdpClient _udpClient;
    private Timer? _timer;
    private ushort _sequence = 0;
    private bool _disposed;

    public UdpTimedSender(string host, int port)
    {
        _host = host;
        _port = port;
        _udpClient = new UdpClient();
    }

    public void StartSending(int intervalMilliseconds)
    {
        if (_timer != null)
            throw new InvalidOperationException("Sender is already running.");

        _timer = new Timer(SendMessageCallback, null, 0, intervalMilliseconds);
    }

    private void SendMessageCallback(object? state)
    {
        try
        {
            byte[] samples = new byte[1024];
            Random.Shared.NextBytes(samples);
            _sequence++;

            byte[] msg = [0x04, 0x84, .. BitConverter.GetBytes(_sequence), .. samples];
            var endpoint = new IPEndPoint(IPAddress.Parse(_host), _port);

            _udpClient.Send(msg, msg.Length, endpoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public void StopSending()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopSending();
        _udpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}