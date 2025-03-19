using System;
using Blizzard.GameService.SDK.Client.Integration;

public interface IDispatcherListener
{
	Action<BattleNetErrors> OnGameServerConnect { get; set; }

	Action<BattleNetErrors> OnGameServerDisconnect { get; }

	Action<PegasusPacket> OnGamePacketReceived { get; set; }

	Action<PegasusPacket> OnGamePacketSent { get; }

	Action<float> OnGameServerPing { get; }

	Action<PegasusPacket> OnUtilPacketReceived { get; }
}
