using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class LocaleModuleDataUpdateStarted : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasLocale;

	private string _Locale;

	public bool HasModuleName;

	private string _ModuleName;

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

	public string Locale
	{
		get
		{
			return _Locale;
		}
		set
		{
			_Locale = value;
			HasLocale = value != null;
		}
	}

	public string ModuleName
	{
		get
		{
			return _ModuleName;
		}
		set
		{
			_ModuleName = value;
			HasModuleName = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasLocale)
		{
			hash ^= Locale.GetHashCode();
		}
		if (HasModuleName)
		{
			hash ^= ModuleName.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is LocaleModuleDataUpdateStarted other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasLocale != other.HasLocale || (HasLocale && !Locale.Equals(other.Locale)))
		{
			return false;
		}
		if (HasModuleName != other.HasModuleName || (HasModuleName && !ModuleName.Equals(other.ModuleName)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static LocaleModuleDataUpdateStarted Deserialize(Stream stream, LocaleModuleDataUpdateStarted instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static LocaleModuleDataUpdateStarted DeserializeLengthDelimited(Stream stream)
	{
		LocaleModuleDataUpdateStarted instance = new LocaleModuleDataUpdateStarted();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static LocaleModuleDataUpdateStarted DeserializeLengthDelimited(Stream stream, LocaleModuleDataUpdateStarted instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static LocaleModuleDataUpdateStarted Deserialize(Stream stream, LocaleModuleDataUpdateStarted instance, long limit)
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
				instance.Locale = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				instance.ModuleName = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, LocaleModuleDataUpdateStarted instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasLocale)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Locale));
		}
		if (instance.HasModuleName)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ModuleName));
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
		if (HasLocale)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(Locale);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasModuleName)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(ModuleName);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		return size;
	}
}
