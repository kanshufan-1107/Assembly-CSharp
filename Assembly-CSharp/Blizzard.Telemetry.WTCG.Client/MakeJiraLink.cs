using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class MakeJiraLink : IProtoBuf
{
	public enum StatusValues
	{
		SUCCEEDED = 1,
		BEING_CREATED,
		NETWORK_ERROR,
		HTTP_ERROR,
		DUPLICATED_ERROR,
		FAILED
	}

	public bool HasExceptionId;

	private string _ExceptionId;

	public bool HasJiraId;

	private string _JiraId;

	public bool HasStatus;

	private StatusValues _Status;

	public string ExceptionId
	{
		get
		{
			return _ExceptionId;
		}
		set
		{
			_ExceptionId = value;
			HasExceptionId = value != null;
		}
	}

	public string JiraId
	{
		get
		{
			return _JiraId;
		}
		set
		{
			_JiraId = value;
			HasJiraId = value != null;
		}
	}

	public StatusValues Status
	{
		get
		{
			return _Status;
		}
		set
		{
			_Status = value;
			HasStatus = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasExceptionId)
		{
			hash ^= ExceptionId.GetHashCode();
		}
		if (HasJiraId)
		{
			hash ^= JiraId.GetHashCode();
		}
		if (HasStatus)
		{
			hash ^= Status.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MakeJiraLink other))
		{
			return false;
		}
		if (HasExceptionId != other.HasExceptionId || (HasExceptionId && !ExceptionId.Equals(other.ExceptionId)))
		{
			return false;
		}
		if (HasJiraId != other.HasJiraId || (HasJiraId && !JiraId.Equals(other.JiraId)))
		{
			return false;
		}
		if (HasStatus != other.HasStatus || (HasStatus && !Status.Equals(other.Status)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static MakeJiraLink Deserialize(Stream stream, MakeJiraLink instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static MakeJiraLink DeserializeLengthDelimited(Stream stream)
	{
		MakeJiraLink instance = new MakeJiraLink();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static MakeJiraLink DeserializeLengthDelimited(Stream stream, MakeJiraLink instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static MakeJiraLink Deserialize(Stream stream, MakeJiraLink instance, long limit)
	{
		instance.Status = StatusValues.SUCCEEDED;
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
				instance.ExceptionId = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				instance.JiraId = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.Status = (StatusValues)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, MakeJiraLink instance)
	{
		if (instance.HasExceptionId)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ExceptionId));
		}
		if (instance.HasJiraId)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.JiraId));
		}
		if (instance.HasStatus)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Status);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasExceptionId)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(ExceptionId);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasJiraId)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(JiraId);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasStatus)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Status);
		}
		return size;
	}
}
