namespace Networking;

public interface IPacketDecoder
{
	PegasusPacket DecodePacket(PegasusPacket p);

	PegasusPacket DecodePacket(int packetType, int context, byte[] body);
}
