using System;
using Blizzard.GameService.SDK.Client.Integration;

public class DispatchListener : IDispatcherListener
{
	public Action<BattleNetErrors> OnGameServerConnect { get; set; }

	public Action<BattleNetErrors> OnGameServerDisconnect { get; set; }

	public Action<PegasusPacket> OnGamePacketReceived { get; set; }

	public Action<PegasusPacket> OnGamePacketSent { get; set; }

	public Action<PegasusPacket> OnUtilPacketReceived { get; }

	public Action<PegasusPacket> OnUtilPacketSent { get; }

	public Action<float> OnGameServerPing { get; set; }
}
