using NetSdrClientApp.Messages;

namespace NetSdrClientAppTests
{
    public class NetSdrMessageHelperTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetControlItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, code, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var codeBytes = msg.Skip(2).Take(2);
            var parametersBytes = msg.Skip(4);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);
            var actualCode = BitConverter.ToInt16(codeBytes.ToArray());

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(actualCode, Is.EqualTo((short)code));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void GetDataItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem2;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var parametersBytes = msg.Skip(2);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void TranslateMessage_UnknownItemCode_ReturnsFalse()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.SetControlItem;
            var unknownCode = new byte[] { 0xFF, 0xFF };
            var header = BitConverter.GetBytes((ushort)((int)type << 13 | (2 + unknownCode.Length + 2)));
            var msg = header.Concat(unknownCode).Concat<byte>([0x01, 0x02]).ToArray();

            //Act
            bool success = NetSdrMessageHelper.TranslateMessage(msg, out _, out _, out _, out _);

            //Assert
            Assert.That(success, Is.False);
        }

        [Test]
        public void GetSamples_OddByteCount_IgnoresRemainder()
        {
            //Arrange
            var body = new byte[] { 0x05, 0x00, 0xFF };

            //Act
            var samples = NetSdrMessageHelper.GetSamples(16, body).ToList();

            //Assert
            Assert.That(samples, Has.Count.EqualTo(1));
            Assert.That(samples[0], Is.EqualTo(5));
        }

        [Test]
        public void GetControlItemMessage_PreservesParameterBytes()
        {
            //Arrange
            var parameters = new byte[] { 0x11, 0x22, 0x33, 0x44 };

            //Act
            var msg = NetSdrMessageHelper.GetControlItemMessage(
                NetSdrMessageHelper.MsgTypes.SetControlItem,
                NetSdrMessageHelper.ControlItemCodes.ReceiverState,
                parameters);

            //Assert
            var actualParameters = msg.Skip(4).ToArray();
            Assert.That(actualParameters, Is.EqualTo(parameters));
        }

        [Test]
        public void TranslateMessage_ControlItemRange_ReturnsCorrectType()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.ControlItemRange;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency;
            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, code, [0x01, 0x02]);

            //Act
            bool success = NetSdrMessageHelper.TranslateMessage(msg, out var actualType, out var actualCode, out _, out _);

            //Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(success, Is.True);
                Assert.That(actualType, Is.EqualTo(type));
                Assert.That(actualCode, Is.EqualTo(code));
            }
        }
    }
}