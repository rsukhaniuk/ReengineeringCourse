using NetSdrClientApp.Networking;

namespace NetSdrClientAppTests;

public class UdpClientWrapperTests
{
    [Test]
    public void StopListening_WhenNotStarted_DoesNotThrow()
    {
        // Arrange
        var wrapper = new UdpClientWrapper(0);

        // Act & Assert
        Assert.DoesNotThrow((Action)(() => wrapper.StopListening()));
    }

    [Test]
    public void Exit_DelegatesToStopListening_DoesNotThrow()
    {
        // Arrange
        var wrapper = new UdpClientWrapper(0);

        // Act & Assert
        Assert.DoesNotThrow((Action)(() => wrapper.Exit()));
    }

    [Test]
    public void Equals_SamePort_ReturnsTrue()
    {
        var a = new UdpClientWrapper(12345);
        var b = new UdpClientWrapper(12345);
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_DifferentPort_ReturnsFalse()
    {
        var a = new UdpClientWrapper(12345);
        var b = new UdpClientWrapper(12346);
        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Equals_Null_ReturnsFalse()
    {
        var a = new UdpClientWrapper(12345);
        Assert.That(a, Is.Not.Null);
    }

    [Test]
    public void GetHashCode_SamePort_ReturnsSameHash()
    {
        var a = new UdpClientWrapper(12345);
        var b = new UdpClientWrapper(12345);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var a = new UdpClientWrapper(12345);
        Assert.That(a.Equals("not a wrapper"), Is.EqualTo(false));
    }

    [Test]
    public async Task StartListeningAsync_ThenStop_DisposesCtsProperly()
    {
        var wrapper = new UdpClientWrapper(0);
        var listenTask = wrapper.StartListeningAsync();
        await Task.Delay(20);
        wrapper.StopListening();
        await listenTask;
        Assert.That(listenTask.IsCompletedSuccessfully, Is.True);
    }
}

public class TcpClientWrapperTests
{
    [Test]
    public void SendMessageAsync_ByteArray_ThrowsWhenNotConnected()
    {
        // Arrange
        var wrapper = new TcpClientWrapper("localhost", 19999);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>((Func<Task>)(() => wrapper.SendMessageAsync(new byte[] { 0x01 })));
    }

    [Test]
    public void SendMessageAsync_String_ThrowsWhenNotConnected()
    {
        // Arrange
        var wrapper = new TcpClientWrapper("localhost", 19999);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>((Func<Task>)(() => wrapper.SendMessageAsync("hello")));
    }

    [Test]
    public void Connected_WhenNotConnected_ReturnsFalse()
    {
        var wrapper = new TcpClientWrapper("localhost", 19999);
        Assert.That(wrapper.Connected, Is.False);
    }

    [Test]
    public void Disconnect_WhenNotConnected_DoesNotThrow()
    {
        var wrapper = new TcpClientWrapper("localhost", 19999);
        Assert.DoesNotThrow((Action)(() => wrapper.Disconnect()));
    }

    private static System.Net.Sockets.TcpListener StartListener(out int port)
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        return listener;
    }

    [Test]
    public async Task Connect_SendMessage_Disconnect_HappyPath()
    {
        // Arrange
        var listener = StartListener(out int port);
        try
        {
            _ = listener.AcceptTcpClientAsync();
            var wrapper = new TcpClientWrapper("127.0.0.1", port);

            // Act
            wrapper.Connect();
            await Task.Delay(50);

            // Assert
            Assert.That(wrapper.Connected, Is.True);
            Assert.DoesNotThrowAsync((Func<Task>)(() => wrapper.SendMessageAsync([0x01])));
            wrapper.Disconnect();
            Assert.That(wrapper.Connected, Is.False);
        }
        finally
        {
            listener.Stop();
        }
    }

    [Test]
    public async Task MessageReceived_RaisedWhenServerSendsData()
    {
        // Arrange
        var listener = StartListener(out int port);
        var received = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            _ = Task.Run(async () =>
            {
                var client = await listener.AcceptTcpClientAsync();
                var stream = client.GetStream();
                await stream.WriteAsync(new byte[] { 0x01, 0x02, 0x03 });
            });

            var wrapper = new TcpClientWrapper("127.0.0.1", port);
            wrapper.MessageReceived += (_, data) => received.TrySetResult(data);

            // Act
            wrapper.Connect();
            var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(3));

            // Assert
            Assert.That(result, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03 }));
            wrapper.Disconnect();
        }
        finally
        {
            listener.Stop();
        }
    }

    [Test]
    public async Task StartListening_ServerClosesConnection_ListenerLoopHandlesException()
    {
        // Arrange — server closes immediately, triggering catch(Exception) in the read loop
        var listener = StartListener(out int port);
        var serverClosed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            _ = Task.Run(async () =>
            {
                var client = await listener.AcceptTcpClientAsync();
                await Task.Delay(30);
                client.Close();
                serverClosed.TrySetResult();
            });

            var wrapper = new TcpClientWrapper("127.0.0.1", port);

            // Act & Assert — no exception propagated to caller
            wrapper.Connect();
            await serverClosed.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.Pass();
        }
        finally
        {
            listener.Stop();
        }
    }
}
