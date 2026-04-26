using NetArchTest.Rules;
using NetSdrClientApp;
using NetSdrClientApp.Networking;

namespace NetSdrClientAppTests;

public class ArchitectureTests
{
    private const string NetworkingNamespace = "NetSdrClientApp.Networking";
    private const string MessagesNamespace = "NetSdrClientApp.Messages";

    [Test]
    public void MessagesLayer_HasNoDependency_OnNetworking()
    {
        var result = Types.InAssembly(typeof(TcpClientWrapper).Assembly)
            .That().ResideInNamespace(MessagesNamespace)
            .ShouldNot().HaveDependencyOn(NetworkingNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            $"Messages layer must not depend on Networking. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void NetworkingLayer_HasNoDependency_OnMessages()
    {
        var result = Types.InAssembly(typeof(TcpClientWrapper).Assembly)
            .That().ResideInNamespace(NetworkingNamespace)
            .ShouldNot().HaveDependencyOn(MessagesNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            $"Networking layer must not depend on Messages. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void NetSdrClient_HasNoDependency_OnConcreteWrappers()
    {
        var result = Types.InAssembly(typeof(TcpClientWrapper).Assembly)
            .That().HaveName(nameof(NetSdrClient))
            .ShouldNot().HaveDependencyOnAny(
                typeof(TcpClientWrapper).FullName!,
                typeof(UdpClientWrapper).FullName!)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            $"NetSdrClient must depend on interfaces, not concrete wrappers. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Test]
    public void InterfaceImplementations_ShouldResideIn_NetworkingNamespace()
    {
        var result = Types.InAssembly(typeof(TcpClientWrapper).Assembly)
            .That().ImplementInterface(typeof(ITcpClient))
            .Or().ImplementInterface(typeof(IUdpClient))
            .Should().ResideInNamespace(NetworkingNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            $"All ITcpClient/IUdpClient implementations must be in {NetworkingNamespace}. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
