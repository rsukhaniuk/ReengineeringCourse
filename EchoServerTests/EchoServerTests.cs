using System.Net;
using System.Net.Sockets;
using EchoTcpServer;
using Moq;

namespace EchoServerTests;

public class EchoServerTests
{
    [Test]
    public async Task HandleStreamAsync_EchoesBytesBack()
    {
        var logs = new List<string>();
        var server = new EchoServer(Mock.Of<ITcpListener>(), logs.Add);

        byte[] input = [1, 2, 3, 4, 5];
        var stream = new TestStream(new MemoryStream(input), new MemoryStream());

        await server.HandleStreamAsync(stream, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(logs, Has.Some.Contains("Echoed 5 bytes."));
            Assert.That(stream.WrittenData, Is.EqualTo(input));
        });
    }

    [Test]
    public async Task StartAsync_StopsCleanly_WhenStopCalled()
    {
        var mockListener = new Mock<ITcpListener>();
        var tcs = new TaskCompletionSource<TcpClient>();

        mockListener
            .Setup(l => l.AcceptTcpClientAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(ct =>
            {
                ct.Register(() => tcs.TrySetCanceled());
                return tcs.Task;
            });

        var logs = new List<string>();
        var server = new EchoServer(mockListener.Object, logs.Add);

        var serverTask = Task.Run(() => server.StartAsync());
        await Task.Delay(50);

        server.Stop();
        await serverTask.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.That(logs, Has.Some.Contains("Server stopped."));
    }

    [Test]
    public void UdpTimedSender_StartSending_ThrowsIfAlreadyRunning()
    {
        using var sender = new UdpTimedSender("127.0.0.1", 19999);
        sender.StartSending(10000);

        Assert.Throws<InvalidOperationException>(() => sender.StartSending(10000));

        sender.StopSending();
    }

    [Test]
    public void UdpTimedSender_StopSending_CanBeCalledMultipleTimes()
    {
        using var sender = new UdpTimedSender("127.0.0.1", 19998);
        sender.StartSending(10000);
        sender.StopSending();

        Assert.DoesNotThrow(() => sender.StopSending());
    }

    [Test]
    public async Task HandleClientAsync_EchoesDataAndLogsDisconnect()
    {
        var logs = new List<string>();
        var server = new EchoServer(Mock.Of<ITcpListener>(), logs.Add);

        // Use a real loopback TcpClient pair so GetStream() works
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        using var clientSide = new TcpClient();
        await clientSide.ConnectAsync(IPAddress.Loopback, port);
        using var serverSide = await listener.AcceptTcpClientAsync();
        listener.Stop();

        byte[] data = [10, 20, 30];
        await clientSide.GetStream().WriteAsync(data);
        clientSide.GetStream().Close(); // triggers EOF on server side

        await server.HandleClientAsync(serverSide, CancellationToken.None);

        Assert.That(logs, Has.Some.Contains("Client disconnected."));
    }

    [Test]
    public void EchoServer_DefaultLogger_UsesConsole()
    {
        // Verifies the null-log branch: constructor with no logger must not throw
        var server = new EchoServer(Mock.Of<ITcpListener>());
        Assert.That(server, Is.Not.Null);
    }
}

// In-memory stream double: reads from readFrom, writes to writeTo.
file sealed class TestStream(MemoryStream readFrom, MemoryStream writeTo) : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public byte[] WrittenData => writeTo.ToArray();

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) =>
        readFrom.ReadAsync(buffer, ct);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default) =>
        writeTo.WriteAsync(buffer, ct);

    public override void Flush() { }
    public override int Read(byte[] buffer, int offset, int count) => readFrom.Read(buffer, offset, count);
    public override void Write(byte[] buffer, int offset, int count) => writeTo.Write(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}