namespace Networking;

internal class NoOpPacketDecoder : IPacketDecoder
{
	public PegasusPacket DecodePacket(PegasusPacket packet)
	{
		return DecodePacket(packet.Type, packet.Context, null);
	}

	public PegasusPacket DecodePacket(int packetType, int context, byte[] body)
	{
		return new PegasusPacket
		{
			Type = 254,
			Context = context
		};
	}
}
