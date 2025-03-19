using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class BenchmarkResult : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasCpuAverageFrameTimeMs;

	private int _CpuAverageFrameTimeMs;

	public bool HasGpuAverageFrameTimeMs;

	private int _GpuAverageFrameTimeMs;

	public bool HasBenchmarkVersion;

	private int _BenchmarkVersion;

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

	public int CpuAverageFrameTimeMs
	{
		get
		{
			return _CpuAverageFrameTimeMs;
		}
		set
		{
			_CpuAverageFrameTimeMs = value;
			HasCpuAverageFrameTimeMs = true;
		}
	}

	public int GpuAverageFrameTimeMs
	{
		get
		{
			return _GpuAverageFrameTimeMs;
		}
		set
		{
			_GpuAverageFrameTimeMs = value;
			HasGpuAverageFrameTimeMs = true;
		}
	}

	public int BenchmarkVersion
	{
		get
		{
			return _BenchmarkVersion;
		}
		set
		{
			_BenchmarkVersion = value;
			HasBenchmarkVersion = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasCpuAverageFrameTimeMs)
		{
			hash ^= CpuAverageFrameTimeMs.GetHashCode();
		}
		if (HasGpuAverageFrameTimeMs)
		{
			hash ^= GpuAverageFrameTimeMs.GetHashCode();
		}
		if (HasBenchmarkVersion)
		{
			hash ^= BenchmarkVersion.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BenchmarkResult other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasCpuAverageFrameTimeMs != other.HasCpuAverageFrameTimeMs || (HasCpuAverageFrameTimeMs && !CpuAverageFrameTimeMs.Equals(other.CpuAverageFrameTimeMs)))
		{
			return false;
		}
		if (HasGpuAverageFrameTimeMs != other.HasGpuAverageFrameTimeMs || (HasGpuAverageFrameTimeMs && !GpuAverageFrameTimeMs.Equals(other.GpuAverageFrameTimeMs)))
		{
			return false;
		}
		if (HasBenchmarkVersion != other.HasBenchmarkVersion || (HasBenchmarkVersion && !BenchmarkVersion.Equals(other.BenchmarkVersion)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static BenchmarkResult Deserialize(Stream stream, BenchmarkResult instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static BenchmarkResult DeserializeLengthDelimited(Stream stream)
	{
		BenchmarkResult instance = new BenchmarkResult();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static BenchmarkResult DeserializeLengthDelimited(Stream stream, BenchmarkResult instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static BenchmarkResult Deserialize(Stream stream, BenchmarkResult instance, long limit)
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
			case 16:
				instance.CpuAverageFrameTimeMs = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				instance.GpuAverageFrameTimeMs = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.BenchmarkVersion = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, BenchmarkResult instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasCpuAverageFrameTimeMs)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CpuAverageFrameTimeMs);
		}
		if (instance.HasGpuAverageFrameTimeMs)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.GpuAverageFrameTimeMs);
		}
		if (instance.HasBenchmarkVersion)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.BenchmarkVersion);
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
		if (HasCpuAverageFrameTimeMs)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CpuAverageFrameTimeMs);
		}
		if (HasGpuAverageFrameTimeMs)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)GpuAverageFrameTimeMs);
		}
		if (HasBenchmarkVersion)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)BenchmarkVersion);
		}
		return size;
	}
}
