using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class ExternalAccountLinkingState : IProtoBuf
{
	public enum Status
	{
		STARTED = 1,
		SUCCESS,
		CHALLENGE_URL_POST_ERROR,
		CHALLENGE_URL_RESPONSE_ERROR,
		TOKEN_EXPIRED,
		CHALLENGE_URL_NO_RESPONSE,
		POLLING_NO_RESPONSE,
		LINKING_PENDING,
		UNKNOWN_ERROR
	}

	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasState;

	private Status _State;

	public bool HasExternalAccountId;

	private ulong _ExternalAccountId;

	public Player Player
	{
		get
		{
			return _Player;
		}
		set
		{
			_Player = value;
			HasPlayer = value != null;
		}
	}

	public DeviceInfo DeviceInfo
	{
		get
		{
			return _DeviceInfo;
		}
		set
		{
			_DeviceInfo = value;
			HasDeviceInfo = value != null;
		}
	}

	public Status State
	{
		get
		{
			return _State;
		}
		set
		{
			_State = value;
			HasState = true;
		}
	}

	public ulong ExternalAccountId
	{
		get
		{
			return _ExternalAccountId;
		}
		set
		{
			_ExternalAccountId = value;
			HasExternalAccountId = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasState)
		{
			hash ^= State.GetHashCode();
		}
		if (HasExternalAccountId)
		{
			hash ^= ExternalAccountId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ExternalAccountLinkingState other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasState != other.HasState || (HasState && !State.Equals(other.State)))
		{
			return false;
		}
		if (HasExternalAccountId != other.HasExternalAccountId || (HasExternalAccountId && !ExternalAccountId.Equals(other.ExternalAccountId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ExternalAccountLinkingState Deserialize(Stream stream, ExternalAccountLinkingState instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ExternalAccountLinkingState DeserializeLengthDelimited(Stream stream)
	{
		ExternalAccountLinkingState instance = new ExternalAccountLinkingState();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ExternalAccountLinkingState DeserializeLengthDelimited(Stream stream, ExternalAccountLinkingState instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ExternalAccountLinkingState Deserialize(Stream stream, ExternalAccountLinkingState instance, long limit)
	{
		instance.State = Status.STARTED;
		while (true)
		{
			if (limit >= 0 && stream.Position >= limit)
			{
				if (stream.Position == limit)
				{
					break;
				}
				throw new ProtocolBufferException("Read past max limit");
			}
			int keyByte = stream.ReadByte();
			switch (keyByte)
			{
			case -1:
				break;
			case 10:
				if (instance.Player == null)
				{
					instance.Player = Player.DeserializeLengthDelimited(stream);
				}
				else
				{
					Player.DeserializeLengthDelimited(stream, instance.Player);
				}
				continue;
			case 18:
				if (instance.DeviceInfo == null)
				{
					instance.DeviceInfo = DeviceInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					DeviceInfo.DeserializeLengthDelimited(stream, instance.DeviceInfo);
				}
				continue;
			case 24:
				instance.State = (Status)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.ExternalAccountId = ProtocolParser.ReadUInt64(stream);
				continue;
			default:
			{
				Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
				if (key.Field == 0)
				{
					throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
				}
				ProtocolParser.SkipKey(stream, key);
				continue;
			}
			}
			if (limit < 0)
			{
				break;
			}
			throw new EndOfStreamException();
		}
		return instance;
	}

	public void Serialize(Stream stream)
	{
		Serialize(stream, this);
	}

	public static void Serialize(Stream stream, ExternalAccountLinkingState instance)
	{
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasState)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.State);
		}
		if (instance.HasExternalAccountId)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.ExternalAccountId);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasPlayer)
		{
			size++;
			uint size2 = Player.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		if (HasDeviceInfo)
		{
			size++;
			uint size3 = DeviceInfo.GetSerializedSize();
			size += size3 + ProtocolParser.SizeOfUInt32(size3);
		}
		if (HasState)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)State);
		}
		if (HasExternalAccountId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64(ExternalAccountId);
		}
		return size;
	}
}
