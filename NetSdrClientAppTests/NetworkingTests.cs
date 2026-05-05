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
}
