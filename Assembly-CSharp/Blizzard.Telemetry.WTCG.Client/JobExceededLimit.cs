using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class JobExceededLimit : IProtoBuf
{
	public bool HasJobId;

	private string _JobId;

	public bool HasJobDuration;

	private long _JobDuration;

	public bool HasTestType;

	private string _TestType;

	public string JobId
	{
		get
		{
			return _JobId;
		}
		set
		{
			_JobId = value;
			HasJobId = value != null;
		}
	}

	public long JobDuration
	{
		get
		{
			return _JobDuration;
		}
		set
		{
			_JobDuration = value;
			HasJobDuration = true;
		}
	}

	public string TestType
	{
		get
		{
			return _TestType;
		}
		set
		{
			_TestType = value;
			HasTestType = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasJobId)
		{
			hash ^= JobId.GetHashCode();
		}
		if (HasJobDuration)
		{
			hash ^= JobDuration.GetHashCode();
		}
		if (HasTestType)
		{
			hash ^= TestType.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is JobExceededLimit other))
		{
			return false;
		}
		if (HasJobId != other.HasJobId || (HasJobId && !JobId.Equals(other.JobId)))
		{
			return false;
		}
		if (HasJobDuration != other.HasJobDuration || (HasJobDuration && !JobDuration.Equals(other.JobDuration)))
		{
			return false;
		}
		if (HasTestType != other.HasTestType || (HasTestType && !TestType.Equals(other.TestType)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static JobExceededLimit Deserialize(Stream stream, JobExceededLimit instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static JobExceededLimit DeserializeLengthDelimited(Stream stream)
	{
		JobExceededLimit instance = new JobExceededLimit();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static JobExceededLimit DeserializeLengthDelimited(Stream stream, JobExceededLimit instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static JobExceededLimit Deserialize(Stream stream, JobExceededLimit instance, long limit)
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
				instance.JobId = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				instance.JobDuration = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				instance.TestType = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, JobExceededLimit instance)
	{
		if (instance.HasJobId)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.JobId));
		}
		if (instance.HasJobDuration)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.JobDuration);
		}
		if (instance.HasTestType)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.TestType));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasJobId)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(JobId);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasJobDuration)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)JobDuration);
		}
		if (HasTestType)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(TestType);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		return size;
	}
}
