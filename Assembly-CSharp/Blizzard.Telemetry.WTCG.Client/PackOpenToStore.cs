using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class PackOpenToStore : IProtoBuf
{
	public enum Path
	{
		PACK_OPENING_BUTTON = 1,
		BACK_TO_BOX
	}

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasPath_;

	private Path _Path_;

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

	public Path Path_
	{
		get
		{
			return _Path_;
		}
		set
		{
			_Path_ = value;
			HasPath_ = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasPath_)
		{
			hash ^= Path_.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PackOpenToStore other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasPath_ != other.HasPath_ || (HasPath_ && !Path_.Equals(other.Path_)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static PackOpenToStore Deserialize(Stream stream, PackOpenToStore instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PackOpenToStore DeserializeLengthDelimited(Stream stream)
	{
		PackOpenToStore instance = new PackOpenToStore();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static PackOpenToStore DeserializeLengthDelimited(Stream stream, PackOpenToStore instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static PackOpenToStore Deserialize(Stream stream, PackOpenToStore instance, long limit)
	{
		instance.Path_ = Path.PACK_OPENING_BUTTON;
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
				instance.Path_ = (Path)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, PackOpenToStore instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasPath_)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Path_);
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
		if (HasPath_)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Path_);
		}
		return size;
	}
}
