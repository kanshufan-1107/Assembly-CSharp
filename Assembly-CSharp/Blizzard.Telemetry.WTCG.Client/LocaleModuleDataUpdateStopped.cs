using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class LocaleModuleDataUpdateStopped : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasDuration;

	private float _Duration;

	public bool HasRealDownloadBytes;

	private long _RealDownloadBytes;

	public bool HasExpectedDownloadBytes;

	private long _ExpectedDownloadBytes;

	public bool HasByUser;

	private bool _ByUser;

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

	public float Duration
	{
		get
		{
			return _Duration;
		}
		set
		{
			_Duration = value;
			HasDuration = true;
		}
	}

	public long RealDownloadBytes
	{
		get
		{
			return _RealDownloadBytes;
		}
		set
		{
			_RealDownloadBytes = value;
			HasRealDownloadBytes = true;
		}
	}

	public long ExpectedDownloadBytes
	{
		get
		{
			return _ExpectedDownloadBytes;
		}
		set
		{
			_ExpectedDownloadBytes = value;
			HasExpectedDownloadBytes = true;
		}
	}

	public bool ByUser
	{
		get
		{
			return _ByUser;
		}
		set
		{
			_ByUser = value;
			HasByUser = true;
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
		if (HasDuration)
		{
			hash ^= Duration.GetHashCode();
		}
		if (HasRealDownloadBytes)
		{
			hash ^= RealDownloadBytes.GetHashCode();
		}
		if (HasExpectedDownloadBytes)
		{
			hash ^= ExpectedDownloadBytes.GetHashCode();
		}
		if (HasByUser)
		{
			hash ^= ByUser.GetHashCode();
		}
		if (HasModuleName)
		{
			hash ^= ModuleName.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is LocaleModuleDataUpdateStopped other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasDuration != other.HasDuration || (HasDuration && !Duration.Equals(other.Duration)))
		{
			return false;
		}
		if (HasRealDownloadBytes != other.HasRealDownloadBytes || (HasRealDownloadBytes && !RealDownloadBytes.Equals(other.RealDownloadBytes)))
		{
			return false;
		}
		if (HasExpectedDownloadBytes != other.HasExpectedDownloadBytes || (HasExpectedDownloadBytes && !ExpectedDownloadBytes.Equals(other.ExpectedDownloadBytes)))
		{
			return false;
		}
		if (HasByUser != other.HasByUser || (HasByUser && !ByUser.Equals(other.ByUser)))
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

	public static LocaleModuleDataUpdateStopped Deserialize(Stream stream, LocaleModuleDataUpdateStopped instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static LocaleModuleDataUpdateStopped DeserializeLengthDelimited(Stream stream)
	{
		LocaleModuleDataUpdateStopped instance = new LocaleModuleDataUpdateStopped();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static LocaleModuleDataUpdateStopped DeserializeLengthDelimited(Stream stream, LocaleModuleDataUpdateStopped instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static LocaleModuleDataUpdateStopped Deserialize(Stream stream, LocaleModuleDataUpdateStopped instance, long limit)
	{
		BinaryReader br = new BinaryReader(stream);
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
			case 21:
				instance.Duration = br.ReadSingle();
				continue;
			case 24:
				instance.RealDownloadBytes = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.ExpectedDownloadBytes = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				instance.ByUser = ProtocolParser.ReadBool(stream);
				continue;
			case 50:
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

	public static void Serialize(Stream stream, LocaleModuleDataUpdateStopped instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasDuration)
		{
			stream.WriteByte(21);
			bw.Write(instance.Duration);
		}
		if (instance.HasRealDownloadBytes)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.RealDownloadBytes);
		}
		if (instance.HasExpectedDownloadBytes)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ExpectedDownloadBytes);
		}
		if (instance.HasByUser)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.ByUser);
		}
		if (instance.HasModuleName)
		{
			stream.WriteByte(50);
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
		if (HasDuration)
		{
			size++;
			size += 4;
		}
		if (HasRealDownloadBytes)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)RealDownloadBytes);
		}
		if (HasExpectedDownloadBytes)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ExpectedDownloadBytes);
		}
		if (HasByUser)
		{
			size++;
			size++;
		}
		if (HasModuleName)
		{
			size++;
			uint byteCount6 = (uint)Encoding.UTF8.GetByteCount(ModuleName);
			size += ProtocolParser.SizeOfUInt32(byteCount6) + byteCount6;
		}
		return size;
	}
}
