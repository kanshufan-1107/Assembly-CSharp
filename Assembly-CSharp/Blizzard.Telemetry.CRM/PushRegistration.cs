using System.IO;
using System.Text;

namespace Blizzard.Telemetry.CRM;

public class PushRegistration : IProtoBuf
{
	public bool HasPushId;

	private string _PushId;

	public bool HasUtcOffset;

	private int _UtcOffset;

	public bool HasTimezone;

	private string _Timezone;

	public bool HasApplicationId;

	private string _ApplicationId;

	public bool HasLanguage;

	private string _Language;

	public bool HasOs;

	private string _Os;

	public bool HasOsVersion;

	private string _OsVersion;

	public bool HasDeviceHeight;

	private string _DeviceHeight;

	public bool HasDeviceWidth;

	private string _DeviceWidth;

	public bool HasDeviceDpi;

	private string _DeviceDpi;

	public string PushId
	{
		get
		{
			return _PushId;
		}
		set
		{
			_PushId = value;
			HasPushId = value != null;
		}
	}

	public int UtcOffset
	{
		get
		{
			return _UtcOffset;
		}
		set
		{
			_UtcOffset = value;
			HasUtcOffset = true;
		}
	}

	public string Timezone
	{
		get
		{
			return _Timezone;
		}
		set
		{
			_Timezone = value;
			HasTimezone = value != null;
		}
	}

	public string ApplicationId
	{
		get
		{
			return _ApplicationId;
		}
		set
		{
			_ApplicationId = value;
			HasApplicationId = value != null;
		}
	}

	public string Language
	{
		get
		{
			return _Language;
		}
		set
		{
			_Language = value;
			HasLanguage = value != null;
		}
	}

	public string Os
	{
		get
		{
			return _Os;
		}
		set
		{
			_Os = value;
			HasOs = value != null;
		}
	}

	public string OsVersion
	{
		get
		{
			return _OsVersion;
		}
		set
		{
			_OsVersion = value;
			HasOsVersion = value != null;
		}
	}

	public string DeviceHeight
	{
		get
		{
			return _DeviceHeight;
		}
		set
		{
			_DeviceHeight = value;
			HasDeviceHeight = value != null;
		}
	}

	public string DeviceWidth
	{
		get
		{
			return _DeviceWidth;
		}
		set
		{
			_DeviceWidth = value;
			HasDeviceWidth = value != null;
		}
	}

	public string DeviceDpi
	{
		get
		{
			return _DeviceDpi;
		}
		set
		{
			_DeviceDpi = value;
			HasDeviceDpi = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPushId)
		{
			hash ^= PushId.GetHashCode();
		}
		if (HasUtcOffset)
		{
			hash ^= UtcOffset.GetHashCode();
		}
		if (HasTimezone)
		{
			hash ^= Timezone.GetHashCode();
		}
		if (HasApplicationId)
		{
			hash ^= ApplicationId.GetHashCode();
		}
		if (HasLanguage)
		{
			hash ^= Language.GetHashCode();
		}
		if (HasOs)
		{
			hash ^= Os.GetHashCode();
		}
		if (HasOsVersion)
		{
			hash ^= OsVersion.GetHashCode();
		}
		if (HasDeviceHeight)
		{
			hash ^= DeviceHeight.GetHashCode();
		}
		if (HasDeviceWidth)
		{
			hash ^= DeviceWidth.GetHashCode();
		}
		if (HasDeviceDpi)
		{
			hash ^= DeviceDpi.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PushRegistration other))
		{
			return false;
		}
		if (HasPushId != other.HasPushId || (HasPushId && !PushId.Equals(other.PushId)))
		{
			return false;
		}
		if (HasUtcOffset != other.HasUtcOffset || (HasUtcOffset && !UtcOffset.Equals(other.UtcOffset)))
		{
			return false;
		}
		if (HasTimezone != other.HasTimezone || (HasTimezone && !Timezone.Equals(other.Timezone)))
		{
			return false;
		}
		if (HasApplicationId != other.HasApplicationId || (HasApplicationId && !ApplicationId.Equals(other.ApplicationId)))
		{
			return false;
		}
		if (HasLanguage != other.HasLanguage || (HasLanguage && !Language.Equals(other.Language)))
		{
			return false;
		}
		if (HasOs != other.HasOs || (HasOs && !Os.Equals(other.Os)))
		{
			return false;
		}
		if (HasOsVersion != other.HasOsVersion || (HasOsVersion && !OsVersion.Equals(other.OsVersion)))
		{
			return false;
		}
		if (HasDeviceHeight != other.HasDeviceHeight || (HasDeviceHeight && !DeviceHeight.Equals(other.DeviceHeight)))
		{
			return false;
		}
		if (HasDeviceWidth != other.HasDeviceWidth || (HasDeviceWidth && !DeviceWidth.Equals(other.DeviceWidth)))
		{
			return false;
		}
		if (HasDeviceDpi != other.HasDeviceDpi || (HasDeviceDpi && !DeviceDpi.Equals(other.DeviceDpi)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static PushRegistration Deserialize(Stream stream, PushRegistration instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PushRegistration Deserialize(Stream stream, PushRegistration instance, long limit)
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
			case 82:
				instance.PushId = ProtocolParser.ReadString(stream);
				continue;
			default:
			{
				Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
				switch (key.Field)
				{
				case 0u:
					throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
				case 20u:
					if (key.WireType == Wire.Varint)
					{
						instance.UtcOffset = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 30u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.Timezone = ProtocolParser.ReadString(stream);
					}
					break;
				case 40u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ApplicationId = ProtocolParser.ReadString(stream);
					}
					break;
				case 50u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.Language = ProtocolParser.ReadString(stream);
					}
					break;
				case 60u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.Os = ProtocolParser.ReadString(stream);
					}
					break;
				case 70u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.OsVersion = ProtocolParser.ReadString(stream);
					}
					break;
				case 80u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.DeviceHeight = ProtocolParser.ReadString(stream);
					}
					break;
				case 90u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.DeviceWidth = ProtocolParser.ReadString(stream);
					}
					break;
				case 100u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.DeviceDpi = ProtocolParser.ReadString(stream);
					}
					break;
				default:
					ProtocolParser.SkipKey(stream, key);
					break;
				}
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

	public static void Serialize(Stream stream, PushRegistration instance)
	{
		if (instance.HasPushId)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.PushId));
		}
		if (instance.HasUtcOffset)
		{
			stream.WriteByte(160);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.UtcOffset);
		}
		if (instance.HasTimezone)
		{
			stream.WriteByte(242);
			stream.WriteByte(1);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Timezone));
		}
		if (instance.HasApplicationId)
		{
			stream.WriteByte(194);
			stream.WriteByte(2);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ApplicationId));
		}
		if (instance.HasLanguage)
		{
			stream.WriteByte(146);
			stream.WriteByte(3);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Language));
		}
		if (instance.HasOs)
		{
			stream.WriteByte(226);
			stream.WriteByte(3);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Os));
		}
		if (instance.HasOsVersion)
		{
			stream.WriteByte(178);
			stream.WriteByte(4);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.OsVersion));
		}
		if (instance.HasDeviceHeight)
		{
			stream.WriteByte(130);
			stream.WriteByte(5);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeviceHeight));
		}
		if (instance.HasDeviceWidth)
		{
			stream.WriteByte(210);
			stream.WriteByte(5);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeviceWidth));
		}
		if (instance.HasDeviceDpi)
		{
			stream.WriteByte(162);
			stream.WriteByte(6);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeviceDpi));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasPushId)
		{
			size++;
			uint byteCount10 = (uint)Encoding.UTF8.GetByteCount(PushId);
			size += ProtocolParser.SizeOfUInt32(byteCount10) + byteCount10;
		}
		if (HasUtcOffset)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)UtcOffset);
		}
		if (HasTimezone)
		{
			size += 2;
			uint byteCount30 = (uint)Encoding.UTF8.GetByteCount(Timezone);
			size += ProtocolParser.SizeOfUInt32(byteCount30) + byteCount30;
		}
		if (HasApplicationId)
		{
			size += 2;
			uint byteCount40 = (uint)Encoding.UTF8.GetByteCount(ApplicationId);
			size += ProtocolParser.SizeOfUInt32(byteCount40) + byteCount40;
		}
		if (HasLanguage)
		{
			size += 2;
			uint byteCount50 = (uint)Encoding.UTF8.GetByteCount(Language);
			size += ProtocolParser.SizeOfUInt32(byteCount50) + byteCount50;
		}
		if (HasOs)
		{
			size += 2;
			uint byteCount60 = (uint)Encoding.UTF8.GetByteCount(Os);
			size += ProtocolParser.SizeOfUInt32(byteCount60) + byteCount60;
		}
		if (HasOsVersion)
		{
			size += 2;
			uint byteCount70 = (uint)Encoding.UTF8.GetByteCount(OsVersion);
			size += ProtocolParser.SizeOfUInt32(byteCount70) + byteCount70;
		}
		if (HasDeviceHeight)
		{
			size += 2;
			uint byteCount80 = (uint)Encoding.UTF8.GetByteCount(DeviceHeight);
			size += ProtocolParser.SizeOfUInt32(byteCount80) + byteCount80;
		}
		if (HasDeviceWidth)
		{
			size += 2;
			uint byteCount90 = (uint)Encoding.UTF8.GetByteCount(DeviceWidth);
			size += ProtocolParser.SizeOfUInt32(byteCount90) + byteCount90;
		}
		if (HasDeviceDpi)
		{
			size += 2;
			uint byteCount100 = (uint)Encoding.UTF8.GetByteCount(DeviceDpi);
			size += ProtocolParser.SizeOfUInt32(byteCount100) + byteCount100;
		}
		return size;
	}
}
