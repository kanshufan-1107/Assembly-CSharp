using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class NetworkError : IProtoBuf
{
	public enum ErrorType
	{
		PRIVATE_SERVER = 1,
		SERVICE_UNAVAILABLE,
		PEER_UNAVAILABLE,
		TIMEOUT_DEFERRED_RESPONSE,
		TIMEOUT_NOT_DEFERRED_RESPONSE,
		REQUEST_ERROR,
		OTHER_UNKNOWN,
		FATAL_LOG
	}

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasErrorType_;

	private ErrorType _ErrorType_;

	public bool HasDescription;

	private string _Description;

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

	public ErrorType ErrorType_
	{
		get
		{
			return _ErrorType_;
		}
		set
		{
			_ErrorType_ = value;
			HasErrorType_ = true;
		}
	}

	public string Description
	{
		get
		{
			return _Description;
		}
		set
		{
			_Description = value;
			HasDescription = value != null;
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
		if (HasErrorType_)
		{
			hash ^= ErrorType_.GetHashCode();
		}
		if (HasDescription)
		{
			hash ^= Description.GetHashCode();
		}
		if (HasErrorCode)
		{
			hash ^= ErrorCode.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NetworkError other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasErrorType_ != other.HasErrorType_ || (HasErrorType_ && !ErrorType_.Equals(other.ErrorType_)))
		{
			return false;
		}
		if (HasDescription != other.HasDescription || (HasDescription && !Description.Equals(other.Description)))
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

	public static NetworkError Deserialize(Stream stream, NetworkError instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static NetworkError DeserializeLengthDelimited(Stream stream)
	{
		NetworkError instance = new NetworkError();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static NetworkError DeserializeLengthDelimited(Stream stream, NetworkError instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static NetworkError Deserialize(Stream stream, NetworkError instance, long limit)
	{
		instance.ErrorType_ = ErrorType.PRIVATE_SERVER;
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
				instance.ErrorType_ = (ErrorType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				instance.Description = ProtocolParser.ReadString(stream);
				continue;
			case 32:
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

	public static void Serialize(Stream stream, NetworkError instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasErrorType_)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ErrorType_);
		}
		if (instance.HasDescription)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Description));
		}
		if (instance.HasErrorCode)
		{
			stream.WriteByte(32);
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
		if (HasErrorType_)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ErrorType_);
		}
		if (HasDescription)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(Description);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasErrorCode)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ErrorCode);
		}
		return size;
	}
}
