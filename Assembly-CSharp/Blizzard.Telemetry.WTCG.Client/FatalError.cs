using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class FatalError : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasReason;

	private string _Reason;

	public bool HasDataVersion;

	private int _DataVersion;

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

	public string Reason
	{
		get
		{
			return _Reason;
		}
		set
		{
			_Reason = value;
			HasReason = value != null;
		}
	}

	public int DataVersion
	{
		get
		{
			return _DataVersion;
		}
		set
		{
			_DataVersion = value;
			HasDataVersion = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasReason)
		{
			hash ^= Reason.GetHashCode();
		}
		if (HasDataVersion)
		{
			hash ^= DataVersion.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is FatalError other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasReason != other.HasReason || (HasReason && !Reason.Equals(other.Reason)))
		{
			return false;
		}
		if (HasDataVersion != other.HasDataVersion || (HasDataVersion && !DataVersion.Equals(other.DataVersion)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static FatalError Deserialize(Stream stream, FatalError instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static FatalError DeserializeLengthDelimited(Stream stream)
	{
		FatalError instance = new FatalError();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static FatalError DeserializeLengthDelimited(Stream stream, FatalError instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static FatalError Deserialize(Stream stream, FatalError instance, long limit)
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
				instance.Reason = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.DataVersion = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, FatalError instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasReason)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Reason));
		}
		if (instance.HasDataVersion)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.DataVersion);
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
		if (HasReason)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(Reason);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasDataVersion)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)DataVersion);
		}
		return size;
	}
}
