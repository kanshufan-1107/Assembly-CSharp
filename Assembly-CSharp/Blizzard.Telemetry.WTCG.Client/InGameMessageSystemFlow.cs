using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class InGameMessageSystemFlow : IProtoBuf
{
	public enum TelemetryMessageType
	{
		Register_Content_Type = 1,
		Update_Messages = 2,
		Event_Display_Queue = 4
	}

	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasMessageType;

	private string _MessageType;

	public bool HasEventId;

	private string _EventId;

	public bool HasCount;

	private int _Count;

	public bool HasTelemetryMessageType_;

	private TelemetryMessageType _TelemetryMessageType_;

	public bool HasUid;

	private string _Uid;

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

	public string MessageType
	{
		get
		{
			return _MessageType;
		}
		set
		{
			_MessageType = value;
			HasMessageType = value != null;
		}
	}

	public string EventId
	{
		get
		{
			return _EventId;
		}
		set
		{
			_EventId = value;
			HasEventId = value != null;
		}
	}

	public int Count
	{
		get
		{
			return _Count;
		}
		set
		{
			_Count = value;
			HasCount = true;
		}
	}

	public TelemetryMessageType TelemetryMessageType_
	{
		get
		{
			return _TelemetryMessageType_;
		}
		set
		{
			_TelemetryMessageType_ = value;
			HasTelemetryMessageType_ = true;
		}
	}

	public string Uid
	{
		get
		{
			return _Uid;
		}
		set
		{
			_Uid = value;
			HasUid = value != null;
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
		if (HasMessageType)
		{
			hash ^= MessageType.GetHashCode();
		}
		if (HasEventId)
		{
			hash ^= EventId.GetHashCode();
		}
		if (HasCount)
		{
			hash ^= Count.GetHashCode();
		}
		if (HasTelemetryMessageType_)
		{
			hash ^= TelemetryMessageType_.GetHashCode();
		}
		if (HasUid)
		{
			hash ^= Uid.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is InGameMessageSystemFlow other))
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
		if (HasMessageType != other.HasMessageType || (HasMessageType && !MessageType.Equals(other.MessageType)))
		{
			return false;
		}
		if (HasEventId != other.HasEventId || (HasEventId && !EventId.Equals(other.EventId)))
		{
			return false;
		}
		if (HasCount != other.HasCount || (HasCount && !Count.Equals(other.Count)))
		{
			return false;
		}
		if (HasTelemetryMessageType_ != other.HasTelemetryMessageType_ || (HasTelemetryMessageType_ && !TelemetryMessageType_.Equals(other.TelemetryMessageType_)))
		{
			return false;
		}
		if (HasUid != other.HasUid || (HasUid && !Uid.Equals(other.Uid)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static InGameMessageSystemFlow Deserialize(Stream stream, InGameMessageSystemFlow instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static InGameMessageSystemFlow DeserializeLengthDelimited(Stream stream)
	{
		InGameMessageSystemFlow instance = new InGameMessageSystemFlow();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static InGameMessageSystemFlow DeserializeLengthDelimited(Stream stream, InGameMessageSystemFlow instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static InGameMessageSystemFlow Deserialize(Stream stream, InGameMessageSystemFlow instance, long limit)
	{
		instance.TelemetryMessageType_ = TelemetryMessageType.Register_Content_Type;
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
				instance.MessageType = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.EventId = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				instance.Count = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				instance.TelemetryMessageType_ = (TelemetryMessageType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 82:
				instance.Uid = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, InGameMessageSystemFlow instance)
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
		if (instance.HasMessageType)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.MessageType));
		}
		if (instance.HasEventId)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.EventId));
		}
		if (instance.HasCount)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Count);
		}
		if (instance.HasTelemetryMessageType_)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TelemetryMessageType_);
		}
		if (instance.HasUid)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Uid));
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
		if (HasMessageType)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(MessageType);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasEventId)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(EventId);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasCount)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Count);
		}
		if (HasTelemetryMessageType_)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TelemetryMessageType_);
		}
		if (HasUid)
		{
			size++;
			uint byteCount10 = (uint)Encoding.UTF8.GetByteCount(Uid);
			size += ProtocolParser.SizeOfUInt32(byteCount10) + byteCount10;
		}
		return size;
	}
}
