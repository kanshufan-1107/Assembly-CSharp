using System.IO;

namespace Networking;

internal class DefaultProtobufPacketDecoder<T> : IPacketDecoder where T : IProtoBuf, new()
{
	public PegasusPacket DecodePacket(PegasusPacket packet)
	{
		byte[] bytes = (byte[])packet.Body;
		T result = new T();
		result.Deserialize(new MemoryStream(bytes));
		packet.Body = result;
		return packet;
	}

	public PegasusPacket DecodePacket(int packetType, int context, byte[] body)
	{
		T result = new T();
		result.Deserialize(new MemoryStream(body));
		return new PegasusPacket
		{
			Type = packetType,
			Body = result,
			Context = context
		};
	}
}
