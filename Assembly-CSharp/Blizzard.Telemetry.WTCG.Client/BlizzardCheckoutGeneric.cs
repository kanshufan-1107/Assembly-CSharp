using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class BlizzardCheckoutGeneric : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasMessageKey;

	private string _MessageKey;

	public bool HasMessageValue;

	private string _MessageValue;

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

	public string MessageKey
	{
		get
		{
			return _MessageKey;
		}
		set
		{
			_MessageKey = value;
			HasMessageKey = value != null;
		}
	}

	public string MessageValue
	{
		get
		{
			return _MessageValue;
		}
		set
		{
			_MessageValue = value;
			HasMessageValue = value != null;
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
		if (HasMessageKey)
		{
			hash ^= MessageKey.GetHashCode();
		}
		if (HasMessageValue)
		{
			hash ^= MessageValue.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BlizzardCheckoutGeneric other))
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
		if (HasMessageKey != other.HasMessageKey || (HasMessageKey && !MessageKey.Equals(other.MessageKey)))
		{
			return false;
		}
		if (HasMessageValue != other.HasMessageValue || (HasMessageValue && !MessageValue.Equals(other.MessageValue)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static BlizzardCheckoutGeneric Deserialize(Stream stream, BlizzardCheckoutGeneric instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static BlizzardCheckoutGeneric DeserializeLengthDelimited(Stream stream)
	{
		BlizzardCheckoutGeneric instance = new BlizzardCheckoutGeneric();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static BlizzardCheckoutGeneric DeserializeLengthDelimited(Stream stream, BlizzardCheckoutGeneric instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static BlizzardCheckoutGeneric Deserialize(Stream stream, BlizzardCheckoutGeneric instance, long limit)
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
				instance.MessageKey = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.MessageValue = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, BlizzardCheckoutGeneric instance)
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
		if (instance.HasMessageKey)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.MessageKey));
		}
		if (instance.HasMessageValue)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.MessageValue));
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
		if (HasMessageKey)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(MessageKey);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasMessageValue)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(MessageValue);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		return size;
	}
}
