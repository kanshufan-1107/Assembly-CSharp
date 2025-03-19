using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class LocaleDataUpdateFailed : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasDuration;

	private float _Duration;

	public bool HasRealDownloadBytes;

	private long _RealDownloadBytes;

	public bool HasExpectedDownloadBytes;

	private long _ExpectedDownloadBytes;

	public bool HasErrorCode;

	private int _ErrorCode;

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

	public int ErrorCode
	{
		get
		{
			return _ErrorCode;
		}
		set
		{
			_ErrorCode = value;
			HasErrorCode = true;
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
		if (HasErrorCode)
		{
			hash ^= ErrorCode.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is LocaleDataUpdateFailed other))
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
		if (HasErrorCode != other.HasErrorCode || (HasErrorCode && !ErrorCode.Equals(other.ErrorCode)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static LocaleDataUpdateFailed Deserialize(Stream stream, LocaleDataUpdateFailed instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static LocaleDataUpdateFailed DeserializeLengthDelimited(Stream stream)
	{
		LocaleDataUpdateFailed instance = new LocaleDataUpdateFailed();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static LocaleDataUpdateFailed DeserializeLengthDelimited(Stream stream, LocaleDataUpdateFailed instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static LocaleDataUpdateFailed Deserialize(Stream stream, LocaleDataUpdateFailed instance, long limit)
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
				instance.ErrorCode = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, LocaleDataUpdateFailed instance)
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
		if (instance.HasErrorCode)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ErrorCode);
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
		if (HasErrorCode)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ErrorCode);
		}
		return size;
	}
}
