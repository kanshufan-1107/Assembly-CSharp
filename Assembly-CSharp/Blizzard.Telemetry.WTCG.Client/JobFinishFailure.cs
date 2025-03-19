using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class JobFinishFailure : IProtoBuf
{
	public bool HasJobId;

	private string _JobId;

	public bool HasJobFailureReason;

	private string _JobFailureReason;

	public bool HasTestType;

	private string _TestType;

	public bool HasClientChangelist;

	private string _ClientChangelist;

	public bool HasDuration;

	private float _Duration;

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

	public string JobFailureReason
	{
		get
		{
			return _JobFailureReason;
		}
		set
		{
			_JobFailureReason = value;
			HasJobFailureReason = value != null;
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

	public string ClientChangelist
	{
		get
		{
			return _ClientChangelist;
		}
		set
		{
			_ClientChangelist = value;
			HasClientChangelist = value != null;
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

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasJobId)
		{
			hash ^= JobId.GetHashCode();
		}
		if (HasJobFailureReason)
		{
			hash ^= JobFailureReason.GetHashCode();
		}
		if (HasTestType)
		{
			hash ^= TestType.GetHashCode();
		}
		if (HasClientChangelist)
		{
			hash ^= ClientChangelist.GetHashCode();
		}
		if (HasDuration)
		{
			hash ^= Duration.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is JobFinishFailure other))
		{
			return false;
		}
		if (HasJobId != other.HasJobId || (HasJobId && !JobId.Equals(other.JobId)))
		{
			return false;
		}
		if (HasJobFailureReason != other.HasJobFailureReason || (HasJobFailureReason && !JobFailureReason.Equals(other.JobFailureReason)))
		{
			return false;
		}
		if (HasTestType != other.HasTestType || (HasTestType && !TestType.Equals(other.TestType)))
		{
			return false;
		}
		if (HasClientChangelist != other.HasClientChangelist || (HasClientChangelist && !ClientChangelist.Equals(other.ClientChangelist)))
		{
			return false;
		}
		if (HasDuration != other.HasDuration || (HasDuration && !Duration.Equals(other.Duration)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static JobFinishFailure Deserialize(Stream stream, JobFinishFailure instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static JobFinishFailure DeserializeLengthDelimited(Stream stream)
	{
		JobFinishFailure instance = new JobFinishFailure();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static JobFinishFailure DeserializeLengthDelimited(Stream stream, JobFinishFailure instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static JobFinishFailure Deserialize(Stream stream, JobFinishFailure instance, long limit)
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
				instance.JobId = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				instance.JobFailureReason = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				instance.TestType = ProtocolParser.ReadString(stream);
				continue;
			case 50:
				instance.ClientChangelist = ProtocolParser.ReadString(stream);
				continue;
			case 61:
				instance.Duration = br.ReadSingle();
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

	public static void Serialize(Stream stream, JobFinishFailure instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasJobId)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.JobId));
		}
		if (instance.HasJobFailureReason)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.JobFailureReason));
		}
		if (instance.HasTestType)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.TestType));
		}
		if (instance.HasClientChangelist)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ClientChangelist));
		}
		if (instance.HasDuration)
		{
			stream.WriteByte(61);
			bw.Write(instance.Duration);
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
		if (HasJobFailureReason)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(JobFailureReason);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasTestType)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(TestType);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasClientChangelist)
		{
			size++;
			uint byteCount6 = (uint)Encoding.UTF8.GetByteCount(ClientChangelist);
			size += ProtocolParser.SizeOfUInt32(byteCount6) + byteCount6;
		}
		if (HasDuration)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
