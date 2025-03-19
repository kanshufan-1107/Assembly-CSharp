using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class PreviousInstanceStatus : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasTotalCrashCount;

	private int _TotalCrashCount;

	public bool HasTotalExceptionCount;

	private int _TotalExceptionCount;

	public bool HasLowMemoryWarningCount;

	private int _LowMemoryWarningCount;

	public bool HasCrashInARowCount;

	private int _CrashInARowCount;

	public bool HasSameExceptionCount;

	private int _SameExceptionCount;

	public bool HasCrashed;

	private bool _Crashed;

	public bool HasExceptionHash;

	private string _ExceptionHash;

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

	public int TotalCrashCount
	{
		get
		{
			return _TotalCrashCount;
		}
		set
		{
			_TotalCrashCount = value;
			HasTotalCrashCount = true;
		}
	}

	public int TotalExceptionCount
	{
		get
		{
			return _TotalExceptionCount;
		}
		set
		{
			_TotalExceptionCount = value;
			HasTotalExceptionCount = true;
		}
	}

	public int LowMemoryWarningCount
	{
		get
		{
			return _LowMemoryWarningCount;
		}
		set
		{
			_LowMemoryWarningCount = value;
			HasLowMemoryWarningCount = true;
		}
	}

	public int CrashInARowCount
	{
		get
		{
			return _CrashInARowCount;
		}
		set
		{
			_CrashInARowCount = value;
			HasCrashInARowCount = true;
		}
	}

	public int SameExceptionCount
	{
		get
		{
			return _SameExceptionCount;
		}
		set
		{
			_SameExceptionCount = value;
			HasSameExceptionCount = true;
		}
	}

	public bool Crashed
	{
		get
		{
			return _Crashed;
		}
		set
		{
			_Crashed = value;
			HasCrashed = true;
		}
	}

	public string ExceptionHash
	{
		get
		{
			return _ExceptionHash;
		}
		set
		{
			_ExceptionHash = value;
			HasExceptionHash = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasTotalCrashCount)
		{
			hash ^= TotalCrashCount.GetHashCode();
		}
		if (HasTotalExceptionCount)
		{
			hash ^= TotalExceptionCount.GetHashCode();
		}
		if (HasLowMemoryWarningCount)
		{
			hash ^= LowMemoryWarningCount.GetHashCode();
		}
		if (HasCrashInARowCount)
		{
			hash ^= CrashInARowCount.GetHashCode();
		}
		if (HasSameExceptionCount)
		{
			hash ^= SameExceptionCount.GetHashCode();
		}
		if (HasCrashed)
		{
			hash ^= Crashed.GetHashCode();
		}
		if (HasExceptionHash)
		{
			hash ^= ExceptionHash.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PreviousInstanceStatus other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasTotalCrashCount != other.HasTotalCrashCount || (HasTotalCrashCount && !TotalCrashCount.Equals(other.TotalCrashCount)))
		{
			return false;
		}
		if (HasTotalExceptionCount != other.HasTotalExceptionCount || (HasTotalExceptionCount && !TotalExceptionCount.Equals(other.TotalExceptionCount)))
		{
			return false;
		}
		if (HasLowMemoryWarningCount != other.HasLowMemoryWarningCount || (HasLowMemoryWarningCount && !LowMemoryWarningCount.Equals(other.LowMemoryWarningCount)))
		{
			return false;
		}
		if (HasCrashInARowCount != other.HasCrashInARowCount || (HasCrashInARowCount && !CrashInARowCount.Equals(other.CrashInARowCount)))
		{
			return false;
		}
		if (HasSameExceptionCount != other.HasSameExceptionCount || (HasSameExceptionCount && !SameExceptionCount.Equals(other.SameExceptionCount)))
		{
			return false;
		}
		if (HasCrashed != other.HasCrashed || (HasCrashed && !Crashed.Equals(other.Crashed)))
		{
			return false;
		}
		if (HasExceptionHash != other.HasExceptionHash || (HasExceptionHash && !ExceptionHash.Equals(other.ExceptionHash)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static PreviousInstanceStatus Deserialize(Stream stream, PreviousInstanceStatus instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PreviousInstanceStatus DeserializeLengthDelimited(Stream stream)
	{
		PreviousInstanceStatus instance = new PreviousInstanceStatus();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static PreviousInstanceStatus DeserializeLengthDelimited(Stream stream, PreviousInstanceStatus instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static PreviousInstanceStatus Deserialize(Stream stream, PreviousInstanceStatus instance, long limit)
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
				instance.TotalCrashCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				instance.TotalExceptionCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.LowMemoryWarningCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				instance.CrashInARowCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				instance.SameExceptionCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				instance.Crashed = ProtocolParser.ReadBool(stream);
				continue;
			case 66:
				instance.ExceptionHash = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, PreviousInstanceStatus instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasTotalCrashCount)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TotalCrashCount);
		}
		if (instance.HasTotalExceptionCount)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.TotalExceptionCount);
		}
		if (instance.HasLowMemoryWarningCount)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.LowMemoryWarningCount);
		}
		if (instance.HasCrashInARowCount)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CrashInARowCount);
		}
		if (instance.HasSameExceptionCount)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SameExceptionCount);
		}
		if (instance.HasCrashed)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.Crashed);
		}
		if (instance.HasExceptionHash)
		{
			stream.WriteByte(66);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ExceptionHash));
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
		if (HasTotalCrashCount)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TotalCrashCount);
		}
		if (HasTotalExceptionCount)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)TotalExceptionCount);
		}
		if (HasLowMemoryWarningCount)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)LowMemoryWarningCount);
		}
		if (HasCrashInARowCount)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CrashInARowCount);
		}
		if (HasSameExceptionCount)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)SameExceptionCount);
		}
		if (HasCrashed)
		{
			size++;
			size++;
		}
		if (HasExceptionHash)
		{
			size++;
			uint byteCount8 = (uint)Encoding.UTF8.GetByteCount(ExceptionHash);
			size += ProtocolParser.SizeOfUInt32(byteCount8) + byteCount8;
		}
		return size;
	}
}
