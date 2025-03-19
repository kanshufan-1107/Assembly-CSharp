using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class PresenceChanged : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasNewPresenceStatus;

	private PresenceStatus _NewPresenceStatus;

	public bool HasPrevPresenceStatus;

	private PresenceStatus _PrevPresenceStatus;

	public bool HasMillisecondsSincePrev;

	private long _MillisecondsSincePrev;

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

	public PresenceStatus NewPresenceStatus
	{
		get
		{
			return _NewPresenceStatus;
		}
		set
		{
			_NewPresenceStatus = value;
			HasNewPresenceStatus = value != null;
		}
	}

	public PresenceStatus PrevPresenceStatus
	{
		get
		{
			return _PrevPresenceStatus;
		}
		set
		{
			_PrevPresenceStatus = value;
			HasPrevPresenceStatus = value != null;
		}
	}

	public long MillisecondsSincePrev
	{
		get
		{
			return _MillisecondsSincePrev;
		}
		set
		{
			_MillisecondsSincePrev = value;
			HasMillisecondsSincePrev = true;
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
		if (HasNewPresenceStatus)
		{
			hash ^= NewPresenceStatus.GetHashCode();
		}
		if (HasPrevPresenceStatus)
		{
			hash ^= PrevPresenceStatus.GetHashCode();
		}
		if (HasMillisecondsSincePrev)
		{
			hash ^= MillisecondsSincePrev.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PresenceChanged other))
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
		if (HasNewPresenceStatus != other.HasNewPresenceStatus || (HasNewPresenceStatus && !NewPresenceStatus.Equals(other.NewPresenceStatus)))
		{
			return false;
		}
		if (HasPrevPresenceStatus != other.HasPrevPresenceStatus || (HasPrevPresenceStatus && !PrevPresenceStatus.Equals(other.PrevPresenceStatus)))
		{
			return false;
		}
		if (HasMillisecondsSincePrev != other.HasMillisecondsSincePrev || (HasMillisecondsSincePrev && !MillisecondsSincePrev.Equals(other.MillisecondsSincePrev)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static PresenceChanged Deserialize(Stream stream, PresenceChanged instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PresenceChanged DeserializeLengthDelimited(Stream stream)
	{
		PresenceChanged instance = new PresenceChanged();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static PresenceChanged DeserializeLengthDelimited(Stream stream, PresenceChanged instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static PresenceChanged Deserialize(Stream stream, PresenceChanged instance, long limit)
	{
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
			case 26:
				if (instance.NewPresenceStatus == null)
				{
					instance.NewPresenceStatus = PresenceStatus.DeserializeLengthDelimited(stream);
				}
				else
				{
					PresenceStatus.DeserializeLengthDelimited(stream, instance.NewPresenceStatus);
				}
				continue;
			case 34:
				if (instance.PrevPresenceStatus == null)
				{
					instance.PrevPresenceStatus = PresenceStatus.DeserializeLengthDelimited(stream);
				}
				else
				{
					PresenceStatus.DeserializeLengthDelimited(stream, instance.PrevPresenceStatus);
				}
				continue;
			case 40:
				instance.MillisecondsSincePrev = (long)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, PresenceChanged instance)
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
		if (instance.HasNewPresenceStatus)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteUInt32(stream, instance.NewPresenceStatus.GetSerializedSize());
			PresenceStatus.Serialize(stream, instance.NewPresenceStatus);
		}
		if (instance.HasPrevPresenceStatus)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteUInt32(stream, instance.PrevPresenceStatus.GetSerializedSize());
			PresenceStatus.Serialize(stream, instance.PrevPresenceStatus);
		}
		if (instance.HasMillisecondsSincePrev)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MillisecondsSincePrev);
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
		if (HasNewPresenceStatus)
		{
			size++;
			uint size4 = NewPresenceStatus.GetSerializedSize();
			size += size4 + ProtocolParser.SizeOfUInt32(size4);
		}
		if (HasPrevPresenceStatus)
		{
			size++;
			uint size5 = PrevPresenceStatus.GetSerializedSize();
			size += size5 + ProtocolParser.SizeOfUInt32(size5);
		}
		if (HasMillisecondsSincePrev)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)MillisecondsSincePrev);
		}
		return size;
	}
}
