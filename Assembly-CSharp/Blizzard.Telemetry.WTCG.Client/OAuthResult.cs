using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class OAuthResult : IProtoBuf
{
	public enum AuthResult
	{
		SUCCESS,
		FAILURE_STATUS_CODE,
		FAILURE_HTTP_EXCEPTION,
		FAILURE_INVALID_OP_EXCEPTION,
		FAILURE_EXCEPTION,
		FAILURE_TIMEOUT,
		FAILURE_JSON_PARSE
	}

	public bool HasResult;

	private AuthResult _Result;

	public bool HasErrorStatusCode;

	private int _ErrorStatusCode;

	public AuthResult Result
	{
		get
		{
			return _Result;
		}
		set
		{
			_Result = value;
			HasResult = true;
		}
	}

	public int ErrorStatusCode
	{
		get
		{
			return _ErrorStatusCode;
		}
		set
		{
			_ErrorStatusCode = value;
			HasErrorStatusCode = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasResult)
		{
			hash ^= Result.GetHashCode();
		}
		if (HasErrorStatusCode)
		{
			hash ^= ErrorStatusCode.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is OAuthResult other))
		{
			return false;
		}
		if (HasResult != other.HasResult || (HasResult && !Result.Equals(other.Result)))
		{
			return false;
		}
		if (HasErrorStatusCode != other.HasErrorStatusCode || (HasErrorStatusCode && !ErrorStatusCode.Equals(other.ErrorStatusCode)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static OAuthResult Deserialize(Stream stream, OAuthResult instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static OAuthResult DeserializeLengthDelimited(Stream stream)
	{
		OAuthResult instance = new OAuthResult();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static OAuthResult DeserializeLengthDelimited(Stream stream, OAuthResult instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static OAuthResult Deserialize(Stream stream, OAuthResult instance, long limit)
	{
		instance.Result = AuthResult.SUCCESS;
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
				instance.Result = (AuthResult)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				instance.ErrorStatusCode = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, OAuthResult instance)
	{
		if (instance.HasResult)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Result);
		}
		if (instance.HasErrorStatusCode)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ErrorStatusCode);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasResult)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Result);
		}
		if (HasErrorStatusCode)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ErrorStatusCode);
		}
		return size;
	}
}
