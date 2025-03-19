using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class NetworkUnreachableRecovered : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasOutageSeconds;

	private int _OutageSeconds;

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

	public int OutageSeconds
	{
		get
		{
			return _OutageSeconds;
		}
		set
		{
			_OutageSeconds = value;
			HasOutageSeconds = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasOutageSeconds)
		{
			hash ^= OutageSeconds.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NetworkUnreachableRecovered other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasOutageSeconds != other.HasOutageSeconds || (HasOutageSeconds && !OutageSeconds.Equals(other.OutageSeconds)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static NetworkUnreachableRecovered Deserialize(Stream stream, NetworkUnreachableRecovered instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static NetworkUnreachableRecovered DeserializeLengthDelimited(Stream stream)
	{
		NetworkUnreachableRecovered instance = new NetworkUnreachableRecovered();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static NetworkUnreachableRecovered DeserializeLengthDelimited(Stream stream, NetworkUnreachableRecovered instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static NetworkUnreachableRecovered Deserialize(Stream stream, NetworkUnreachableRecovered instance, long limit)
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
				instance.OutageSeconds = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, NetworkUnreachableRecovered instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasOutageSeconds)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.OutageSeconds);
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
		if (HasOutageSeconds)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)OutageSeconds);
		}
		return size;
	}
}
