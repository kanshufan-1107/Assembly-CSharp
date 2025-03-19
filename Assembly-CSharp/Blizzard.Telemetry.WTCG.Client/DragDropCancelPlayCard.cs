using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class DragDropCancelPlayCard : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasScenarioId;

	private long _ScenarioId;

	public bool HasCardType;

	private string _CardType;

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

	public long ScenarioId
	{
		get
		{
			return _ScenarioId;
		}
		set
		{
			_ScenarioId = value;
			HasScenarioId = true;
		}
	}

	public string CardType
	{
		get
		{
			return _CardType;
		}
		set
		{
			_CardType = value;
			HasCardType = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasScenarioId)
		{
			hash ^= ScenarioId.GetHashCode();
		}
		if (HasCardType)
		{
			hash ^= CardType.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DragDropCancelPlayCard other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasScenarioId != other.HasScenarioId || (HasScenarioId && !ScenarioId.Equals(other.ScenarioId)))
		{
			return false;
		}
		if (HasCardType != other.HasCardType || (HasCardType && !CardType.Equals(other.CardType)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static DragDropCancelPlayCard Deserialize(Stream stream, DragDropCancelPlayCard instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static DragDropCancelPlayCard DeserializeLengthDelimited(Stream stream)
	{
		DragDropCancelPlayCard instance = new DragDropCancelPlayCard();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static DragDropCancelPlayCard DeserializeLengthDelimited(Stream stream, DragDropCancelPlayCard instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static DragDropCancelPlayCard Deserialize(Stream stream, DragDropCancelPlayCard instance, long limit)
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
				if (instance.DeviceInfo == null)
				{
					instance.DeviceInfo = DeviceInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					DeviceInfo.DeserializeLengthDelimited(stream, instance.DeviceInfo);
				}
				continue;
			case 16:
				instance.ScenarioId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				instance.CardType = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, DragDropCancelPlayCard instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasScenarioId)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ScenarioId);
		}
		if (instance.HasCardType)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.CardType));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasDeviceInfo)
		{
			size++;
			uint size2 = DeviceInfo.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		if (HasScenarioId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ScenarioId);
		}
		if (HasCardType)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(CardType);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		return size;
	}
}
