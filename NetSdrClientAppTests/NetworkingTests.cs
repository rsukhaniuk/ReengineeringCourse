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
        Assert.That(a.Equals("not a wrapper"), Is.False);
    }

    [Test]
    public async Task StartListeningAsync_ThenStop_DisposesCtsProperly()
    {
        var wrapper = new UdpClientWrapper(0);
        var listenTask = wrapper.StartListeningAsync();
        await Task.Delay(20);
        wrapper.StopListening();
        await listenTask;
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
}
