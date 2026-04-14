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
        Assert.DoesNotThrow(() => wrapper.StopListening());
    }

    [Test]
    public void Exit_DelegatesToStopListening_DoesNotThrow()
    {
        // Arrange
        var wrapper = new UdpClientWrapper(0);

        // Act & Assert
        Assert.DoesNotThrow(() => wrapper.Exit());
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
        Assert.ThrowsAsync<InvalidOperationException>(() => wrapper.SendMessageAsync(new byte[] { 0x01 }));
    }

    [Test]
    public void SendMessageAsync_String_ThrowsWhenNotConnected()
    {
        // Arrange
        var wrapper = new TcpClientWrapper("localhost", 19999);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => wrapper.SendMessageAsync("hello"));
    }
}
