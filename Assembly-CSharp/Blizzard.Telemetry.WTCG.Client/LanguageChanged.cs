using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class LanguageChanged : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasPreviousLanguage;

	private string _PreviousLanguage;

	public bool HasNextLanguage;

	private string _NextLanguage;

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

	public string PreviousLanguage
	{
		get
		{
			return _PreviousLanguage;
		}
		set
		{
			_PreviousLanguage = value;
			HasPreviousLanguage = value != null;
		}
	}

	public string NextLanguage
	{
		get
		{
			return _NextLanguage;
		}
		set
		{
			_NextLanguage = value;
			HasNextLanguage = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasPreviousLanguage)
		{
			hash ^= PreviousLanguage.GetHashCode();
		}
		if (HasNextLanguage)
		{
			hash ^= NextLanguage.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is LanguageChanged other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasPreviousLanguage != other.HasPreviousLanguage || (HasPreviousLanguage && !PreviousLanguage.Equals(other.PreviousLanguage)))
		{
			return false;
		}
		if (HasNextLanguage != other.HasNextLanguage || (HasNextLanguage && !NextLanguage.Equals(other.NextLanguage)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static LanguageChanged Deserialize(Stream stream, LanguageChanged instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static LanguageChanged DeserializeLengthDelimited(Stream stream)
	{
		LanguageChanged instance = new LanguageChanged();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static LanguageChanged DeserializeLengthDelimited(Stream stream, LanguageChanged instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static LanguageChanged Deserialize(Stream stream, LanguageChanged instance, long limit)
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
				instance.PreviousLanguage = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				instance.NextLanguage = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, LanguageChanged instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasPreviousLanguage)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.PreviousLanguage));
		}
		if (instance.HasNextLanguage)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.NextLanguage));
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
		if (HasPreviousLanguage)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(PreviousLanguage);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasNextLanguage)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(NextLanguage);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		return size;
	}
}
