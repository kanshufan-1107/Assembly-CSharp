using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class BnetConnectionStatus : IProtoBuf
{
	public enum Result
	{
		Success,
		Failure,
		Cancelled
	}

	public enum FailureReason
	{
		DnSLookupFailed,
		RPCConnectionException,
		RPCConnectionFailure,
		LoginStartFailed,
		LoginCompleteFailed,
		SessionStartFailure,
		SessionCreationFailure,
		UnknownException
	}

	public bool HasResult_;

	private Result _Result_;

	public bool HasFailureReason_;

	private FailureReason _FailureReason_;

	public bool HasBnetErrorCode;

	private uint _BnetErrorCode;

	public bool HasBnetErrorCodeName;

	private string _BnetErrorCodeName;

	public Result Result_
	{
		get
		{
			return _Result_;
		}
		set
		{
			_Result_ = value;
			HasResult_ = true;
		}
	}

	public FailureReason FailureReason_
	{
		get
		{
			return _FailureReason_;
		}
		set
		{
			_FailureReason_ = value;
			HasFailureReason_ = true;
		}
	}

	public uint BnetErrorCode
	{
		get
		{
			return _BnetErrorCode;
		}
		set
		{
			_BnetErrorCode = value;
			HasBnetErrorCode = true;
		}
	}

	public string BnetErrorCodeName
	{
		get
		{
			return _BnetErrorCodeName;
		}
		set
		{
			_BnetErrorCodeName = value;
			HasBnetErrorCodeName = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasResult_)
		{
			hash ^= Result_.GetHashCode();
		}
		if (HasFailureReason_)
		{
			hash ^= FailureReason_.GetHashCode();
		}
		if (HasBnetErrorCode)
		{
			hash ^= BnetErrorCode.GetHashCode();
		}
		if (HasBnetErrorCodeName)
		{
			hash ^= BnetErrorCodeName.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BnetConnectionStatus other))
		{
			return false;
		}
		if (HasResult_ != other.HasResult_ || (HasResult_ && !Result_.Equals(other.Result_)))
		{
			return false;
		}
		if (HasFailureReason_ != other.HasFailureReason_ || (HasFailureReason_ && !FailureReason_.Equals(other.FailureReason_)))
		{
			return false;
		}
		if (HasBnetErrorCode != other.HasBnetErrorCode || (HasBnetErrorCode && !BnetErrorCode.Equals(other.BnetErrorCode)))
		{
			return false;
		}
		if (HasBnetErrorCodeName != other.HasBnetErrorCodeName || (HasBnetErrorCodeName && !BnetErrorCodeName.Equals(other.BnetErrorCodeName)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static BnetConnectionStatus Deserialize(Stream stream, BnetConnectionStatus instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static BnetConnectionStatus DeserializeLengthDelimited(Stream stream)
	{
		BnetConnectionStatus instance = new BnetConnectionStatus();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static BnetConnectionStatus DeserializeLengthDelimited(Stream stream, BnetConnectionStatus instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static BnetConnectionStatus Deserialize(Stream stream, BnetConnectionStatus instance, long limit)
	{
		instance.Result_ = Result.Success;
		instance.FailureReason_ = FailureReason.DnSLookupFailed;
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
			case 8:
				instance.Result_ = (Result)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				instance.FailureReason_ = (FailureReason)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				instance.BnetErrorCode = ProtocolParser.ReadUInt32(stream);
				continue;
			case 34:
				instance.BnetErrorCodeName = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, BnetConnectionStatus instance)
	{
		if (instance.HasResult_)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Result_);
		}
		if (instance.HasFailureReason_)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.FailureReason_);
		}
		if (instance.HasBnetErrorCode)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.BnetErrorCode);
		}
		if (instance.HasBnetErrorCodeName)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.BnetErrorCodeName));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasResult_)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Result_);
		}
		if (HasFailureReason_)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)FailureReason_);
		}
		if (HasBnetErrorCode)
		{
			size++;
			size += ProtocolParser.SizeOfUInt32(BnetErrorCode);
		}
		if (HasBnetErrorCodeName)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(BnetErrorCodeName);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		return size;
	}
}
