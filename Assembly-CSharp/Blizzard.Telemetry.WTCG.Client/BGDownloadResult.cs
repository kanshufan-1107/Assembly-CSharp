using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class BGDownloadResult : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasDuration;

	private float _Duration;

	public bool HasPrevRemainingBytes;

	private long _PrevRemainingBytes;

	public bool HasDownloadedBytes;

	private long _DownloadedBytes;

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

	public long PrevRemainingBytes
	{
		get
		{
			return _PrevRemainingBytes;
		}
		set
		{
			_PrevRemainingBytes = value;
			HasPrevRemainingBytes = true;
		}
	}

	public long DownloadedBytes
	{
		get
		{
			return _DownloadedBytes;
		}
		set
		{
			_DownloadedBytes = value;
			HasDownloadedBytes = true;
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
		if (HasPrevRemainingBytes)
		{
			hash ^= PrevRemainingBytes.GetHashCode();
		}
		if (HasDownloadedBytes)
		{
			hash ^= DownloadedBytes.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BGDownloadResult other))
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
		if (HasPrevRemainingBytes != other.HasPrevRemainingBytes || (HasPrevRemainingBytes && !PrevRemainingBytes.Equals(other.PrevRemainingBytes)))
		{
			return false;
		}
		if (HasDownloadedBytes != other.HasDownloadedBytes || (HasDownloadedBytes && !DownloadedBytes.Equals(other.DownloadedBytes)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static BGDownloadResult Deserialize(Stream stream, BGDownloadResult instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static BGDownloadResult DeserializeLengthDelimited(Stream stream)
	{
		BGDownloadResult instance = new BGDownloadResult();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static BGDownloadResult DeserializeLengthDelimited(Stream stream, BGDownloadResult instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static BGDownloadResult Deserialize(Stream stream, BGDownloadResult instance, long limit)
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
				instance.PrevRemainingBytes = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.DownloadedBytes = (long)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, BGDownloadResult instance)
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
		if (instance.HasPrevRemainingBytes)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.PrevRemainingBytes);
		}
		if (instance.HasDownloadedBytes)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.DownloadedBytes);
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
		if (HasPrevRemainingBytes)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)PrevRemainingBytes);
		}
		if (HasDownloadedBytes)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)DownloadedBytes);
		}
		return size;
	}
}
