using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class AssetOrphaned : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasFilePath;

	private string _FilePath;

	public bool HasHandleOwner;

	private string _HandleOwner;

	public bool HasHandleType;

	private string _HandleType;

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

	public string FilePath
	{
		get
		{
			return _FilePath;
		}
		set
		{
			_FilePath = value;
			HasFilePath = value != null;
		}
	}

	public string HandleOwner
	{
		get
		{
			return _HandleOwner;
		}
		set
		{
			_HandleOwner = value;
			HasHandleOwner = value != null;
		}
	}

	public string HandleType
	{
		get
		{
			return _HandleType;
		}
		set
		{
			_HandleType = value;
			HasHandleType = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasFilePath)
		{
			hash ^= FilePath.GetHashCode();
		}
		if (HasHandleOwner)
		{
			hash ^= HandleOwner.GetHashCode();
		}
		if (HasHandleType)
		{
			hash ^= HandleType.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AssetOrphaned other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasFilePath != other.HasFilePath || (HasFilePath && !FilePath.Equals(other.FilePath)))
		{
			return false;
		}
		if (HasHandleOwner != other.HasHandleOwner || (HasHandleOwner && !HandleOwner.Equals(other.HandleOwner)))
		{
			return false;
		}
		if (HasHandleType != other.HasHandleType || (HasHandleType && !HandleType.Equals(other.HandleType)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static AssetOrphaned Deserialize(Stream stream, AssetOrphaned instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static AssetOrphaned DeserializeLengthDelimited(Stream stream)
	{
		AssetOrphaned instance = new AssetOrphaned();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static AssetOrphaned DeserializeLengthDelimited(Stream stream, AssetOrphaned instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static AssetOrphaned Deserialize(Stream stream, AssetOrphaned instance, long limit)
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
			case 18:
				instance.FilePath = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				instance.HandleOwner = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.HandleType = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, AssetOrphaned instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasFilePath)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.FilePath));
		}
		if (instance.HasHandleOwner)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.HandleOwner));
		}
		if (instance.HasHandleType)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.HandleType));
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
		if (HasFilePath)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(FilePath);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasHandleOwner)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(HandleOwner);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasHandleType)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(HandleType);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		return size;
	}
}
