namespace Networking;

public interface IPacketDecoderManager
{
	bool CanDecodePacket(int packetId);

	PegasusPacket DecodePacket(PegasusPacket packet);

	PegasusPacket DecodePacket(int packetType, int context, byte[] body);
}
