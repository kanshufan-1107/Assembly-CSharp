using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class MissingAssetError : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasMissingAssetPath;

	private string _MissingAssetPath;

	public bool HasAssetContext;

	private string _AssetContext;

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

	public string MissingAssetPath
	{
		get
		{
			return _MissingAssetPath;
		}
		set
		{
			_MissingAssetPath = value;
			HasMissingAssetPath = value != null;
		}
	}

	public string AssetContext
	{
		get
		{
			return _AssetContext;
		}
		set
		{
			_AssetContext = value;
			HasAssetContext = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasMissingAssetPath)
		{
			hash ^= MissingAssetPath.GetHashCode();
		}
		if (HasAssetContext)
		{
			hash ^= AssetContext.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MissingAssetError other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasMissingAssetPath != other.HasMissingAssetPath || (HasMissingAssetPath && !MissingAssetPath.Equals(other.MissingAssetPath)))
		{
			return false;
		}
		if (HasAssetContext != other.HasAssetContext || (HasAssetContext && !AssetContext.Equals(other.AssetContext)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static MissingAssetError Deserialize(Stream stream, MissingAssetError instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static MissingAssetError DeserializeLengthDelimited(Stream stream)
	{
		MissingAssetError instance = new MissingAssetError();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static MissingAssetError DeserializeLengthDelimited(Stream stream, MissingAssetError instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static MissingAssetError Deserialize(Stream stream, MissingAssetError instance, long limit)
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
				instance.MissingAssetPath = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				instance.AssetContext = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, MissingAssetError instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasMissingAssetPath)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.MissingAssetPath));
		}
		if (instance.HasAssetContext)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AssetContext));
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
		if (HasMissingAssetPath)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(MissingAssetPath);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasAssetContext)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(AssetContext);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		return size;
	}
}
