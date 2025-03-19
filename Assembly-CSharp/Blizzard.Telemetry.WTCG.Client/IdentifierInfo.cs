using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class IdentifierInfo : IProtoBuf
{
	public bool HasAppInstallId;

	private string _AppInstallId;

	public bool HasDeviceId;

	private string _DeviceId;

	public bool HasAdvertisingId;

	private string _AdvertisingId;

	public string AppInstallId
	{
		get
		{
			return _AppInstallId;
		}
		set
		{
			_AppInstallId = value;
			HasAppInstallId = value != null;
		}
	}

	public string DeviceId
	{
		get
		{
			return _DeviceId;
		}
		set
		{
			_DeviceId = value;
			HasDeviceId = value != null;
		}
	}

	public string AdvertisingId
	{
		get
		{
			return _AdvertisingId;
		}
		set
		{
			_AdvertisingId = value;
			HasAdvertisingId = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasAppInstallId)
		{
			hash ^= AppInstallId.GetHashCode();
		}
		if (HasDeviceId)
		{
			hash ^= DeviceId.GetHashCode();
		}
		if (HasAdvertisingId)
		{
			hash ^= AdvertisingId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is IdentifierInfo other))
		{
			return false;
		}
		if (HasAppInstallId != other.HasAppInstallId || (HasAppInstallId && !AppInstallId.Equals(other.AppInstallId)))
		{
			return false;
		}
		if (HasDeviceId != other.HasDeviceId || (HasDeviceId && !DeviceId.Equals(other.DeviceId)))
		{
			return false;
		}
		if (HasAdvertisingId != other.HasAdvertisingId || (HasAdvertisingId && !AdvertisingId.Equals(other.AdvertisingId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static IdentifierInfo Deserialize(Stream stream, IdentifierInfo instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static IdentifierInfo DeserializeLengthDelimited(Stream stream)
	{
		IdentifierInfo instance = new IdentifierInfo();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static IdentifierInfo DeserializeLengthDelimited(Stream stream, IdentifierInfo instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static IdentifierInfo Deserialize(Stream stream, IdentifierInfo instance, long limit)
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
			if (keyByte == -1)
			{
				if (limit < 0)
				{
					break;
				}
				throw new EndOfStreamException();
			}
			Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
			switch (key.Field)
			{
			case 0u:
				throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
			case 101u:
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.AppInstallId = ProtocolParser.ReadString(stream);
				}
				break;
			case 102u:
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.DeviceId = ProtocolParser.ReadString(stream);
				}
				break;
			case 103u:
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.AdvertisingId = ProtocolParser.ReadString(stream);
				}
				break;
			default:
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public void Serialize(Stream stream)
	{
		Serialize(stream, this);
	}

	public static void Serialize(Stream stream, IdentifierInfo instance)
	{
		if (instance.HasAppInstallId)
		{
			stream.WriteByte(170);
			stream.WriteByte(6);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AppInstallId));
		}
		if (instance.HasDeviceId)
		{
			stream.WriteByte(178);
			stream.WriteByte(6);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeviceId));
		}
		if (instance.HasAdvertisingId)
		{
			stream.WriteByte(186);
			stream.WriteByte(6);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AdvertisingId));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasAppInstallId)
		{
			size += 2;
			uint byteCount101 = (uint)Encoding.UTF8.GetByteCount(AppInstallId);
			size += ProtocolParser.SizeOfUInt32(byteCount101) + byteCount101;
		}
		if (HasDeviceId)
		{
			size += 2;
			uint byteCount102 = (uint)Encoding.UTF8.GetByteCount(DeviceId);
			size += ProtocolParser.SizeOfUInt32(byteCount102) + byteCount102;
		}
		if (HasAdvertisingId)
		{
			size += 2;
			uint byteCount103 = (uint)Encoding.UTF8.GetByteCount(AdvertisingId);
			size += ProtocolParser.SizeOfUInt32(byteCount103) + byteCount103;
		}
		return size;
	}
}
