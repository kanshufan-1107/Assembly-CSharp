using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class DeviceInfo : IProtoBuf
{
	public bool HasAndroidId;

	private string _AndroidId;

	public bool HasAndroidModel;

	private string _AndroidModel;

	public bool HasAndroidSdkVersion;

	private uint _AndroidSdkVersion;

	public bool HasIsConnectedToWifi;

	private bool _IsConnectedToWifi;

	public bool HasGpuTextureFormat;

	private string _GpuTextureFormat;

	public bool HasLocale;

	private string _Locale;

	public bool HasBnetRegion;

	private string _BnetRegion;

	public string AndroidId
	{
		get
		{
			return _AndroidId;
		}
		set
		{
			_AndroidId = value;
			HasAndroidId = value != null;
		}
	}

	public string AndroidModel
	{
		get
		{
			return _AndroidModel;
		}
		set
		{
			_AndroidModel = value;
			HasAndroidModel = value != null;
		}
	}

	public uint AndroidSdkVersion
	{
		get
		{
			return _AndroidSdkVersion;
		}
		set
		{
			_AndroidSdkVersion = value;
			HasAndroidSdkVersion = true;
		}
	}

	public bool IsConnectedToWifi
	{
		get
		{
			return _IsConnectedToWifi;
		}
		set
		{
			_IsConnectedToWifi = value;
			HasIsConnectedToWifi = true;
		}
	}

	public string GpuTextureFormat
	{
		get
		{
			return _GpuTextureFormat;
		}
		set
		{
			_GpuTextureFormat = value;
			HasGpuTextureFormat = value != null;
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

	public string BnetRegion
	{
		get
		{
			return _BnetRegion;
		}
		set
		{
			_BnetRegion = value;
			HasBnetRegion = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasAndroidId)
		{
			hash ^= AndroidId.GetHashCode();
		}
		if (HasAndroidModel)
		{
			hash ^= AndroidModel.GetHashCode();
		}
		if (HasAndroidSdkVersion)
		{
			hash ^= AndroidSdkVersion.GetHashCode();
		}
		if (HasIsConnectedToWifi)
		{
			hash ^= IsConnectedToWifi.GetHashCode();
		}
		if (HasGpuTextureFormat)
		{
			hash ^= GpuTextureFormat.GetHashCode();
		}
		if (HasLocale)
		{
			hash ^= Locale.GetHashCode();
		}
		if (HasBnetRegion)
		{
			hash ^= BnetRegion.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DeviceInfo other))
		{
			return false;
		}
		if (HasAndroidId != other.HasAndroidId || (HasAndroidId && !AndroidId.Equals(other.AndroidId)))
		{
			return false;
		}
		if (HasAndroidModel != other.HasAndroidModel || (HasAndroidModel && !AndroidModel.Equals(other.AndroidModel)))
		{
			return false;
		}
		if (HasAndroidSdkVersion != other.HasAndroidSdkVersion || (HasAndroidSdkVersion && !AndroidSdkVersion.Equals(other.AndroidSdkVersion)))
		{
			return false;
		}
		if (HasIsConnectedToWifi != other.HasIsConnectedToWifi || (HasIsConnectedToWifi && !IsConnectedToWifi.Equals(other.IsConnectedToWifi)))
		{
			return false;
		}
		if (HasGpuTextureFormat != other.HasGpuTextureFormat || (HasGpuTextureFormat && !GpuTextureFormat.Equals(other.GpuTextureFormat)))
		{
			return false;
		}
		if (HasLocale != other.HasLocale || (HasLocale && !Locale.Equals(other.Locale)))
		{
			return false;
		}
		if (HasBnetRegion != other.HasBnetRegion || (HasBnetRegion && !BnetRegion.Equals(other.BnetRegion)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static DeviceInfo Deserialize(Stream stream, DeviceInfo instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static DeviceInfo DeserializeLengthDelimited(Stream stream)
	{
		DeviceInfo instance = new DeviceInfo();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static DeviceInfo DeserializeLengthDelimited(Stream stream, DeviceInfo instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static DeviceInfo Deserialize(Stream stream, DeviceInfo instance, long limit)
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
				instance.AndroidId = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				instance.AndroidModel = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.AndroidSdkVersion = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				instance.IsConnectedToWifi = ProtocolParser.ReadBool(stream);
				continue;
			case 42:
				instance.GpuTextureFormat = ProtocolParser.ReadString(stream);
				continue;
			case 50:
				instance.Locale = ProtocolParser.ReadString(stream);
				continue;
			case 58:
				instance.BnetRegion = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, DeviceInfo instance)
	{
		if (instance.HasAndroidId)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AndroidId));
		}
		if (instance.HasAndroidModel)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AndroidModel));
		}
		if (instance.HasAndroidSdkVersion)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.AndroidSdkVersion);
		}
		if (instance.HasIsConnectedToWifi)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.IsConnectedToWifi);
		}
		if (instance.HasGpuTextureFormat)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.GpuTextureFormat));
		}
		if (instance.HasLocale)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Locale));
		}
		if (instance.HasBnetRegion)
		{
			stream.WriteByte(58);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.BnetRegion));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasAndroidId)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(AndroidId);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasAndroidModel)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(AndroidModel);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasAndroidSdkVersion)
		{
			size++;
			size += ProtocolParser.SizeOfUInt32(AndroidSdkVersion);
		}
		if (HasIsConnectedToWifi)
		{
			size++;
			size++;
		}
		if (HasGpuTextureFormat)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(GpuTextureFormat);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		if (HasLocale)
		{
			size++;
			uint byteCount6 = (uint)Encoding.UTF8.GetByteCount(Locale);
			size += ProtocolParser.SizeOfUInt32(byteCount6) + byteCount6;
		}
		if (HasBnetRegion)
		{
			size++;
			uint byteCount7 = (uint)Encoding.UTF8.GetByteCount(BnetRegion);
			size += ProtocolParser.SizeOfUInt32(byteCount7) + byteCount7;
		}
		return size;
	}
}
