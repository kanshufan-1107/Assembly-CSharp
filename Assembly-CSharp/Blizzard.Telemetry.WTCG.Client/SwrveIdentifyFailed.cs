using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class SwrveIdentifyFailed : IProtoBuf
{
	public bool HasAppName;

	private string _AppName;

	public bool HasAppId;

	private long _AppId;

	public bool HasSwrveUserId;

	private string _SwrveUserId;

	public bool HasErrorCode;

	private long _ErrorCode;

	public bool HasErrorMessage;

	private string _ErrorMessage;

	public string AppName
	{
		get
		{
			return _AppName;
		}
		set
		{
			_AppName = value;
			HasAppName = value != null;
		}
	}

	public long AppId
	{
		get
		{
			return _AppId;
		}
		set
		{
			_AppId = value;
			HasAppId = true;
		}
	}

	public string SwrveUserId
	{
		get
		{
			return _SwrveUserId;
		}
		set
		{
			_SwrveUserId = value;
			HasSwrveUserId = value != null;
		}
	}

	public long ErrorCode
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

	public string ErrorMessage
	{
		get
		{
			return _ErrorMessage;
		}
		set
		{
			_ErrorMessage = value;
			HasErrorMessage = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasAppName)
		{
			hash ^= AppName.GetHashCode();
		}
		if (HasAppId)
		{
			hash ^= AppId.GetHashCode();
		}
		if (HasSwrveUserId)
		{
			hash ^= SwrveUserId.GetHashCode();
		}
		if (HasErrorCode)
		{
			hash ^= ErrorCode.GetHashCode();
		}
		if (HasErrorMessage)
		{
			hash ^= ErrorMessage.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SwrveIdentifyFailed other))
		{
			return false;
		}
		if (HasAppName != other.HasAppName || (HasAppName && !AppName.Equals(other.AppName)))
		{
			return false;
		}
		if (HasAppId != other.HasAppId || (HasAppId && !AppId.Equals(other.AppId)))
		{
			return false;
		}
		if (HasSwrveUserId != other.HasSwrveUserId || (HasSwrveUserId && !SwrveUserId.Equals(other.SwrveUserId)))
		{
			return false;
		}
		if (HasErrorCode != other.HasErrorCode || (HasErrorCode && !ErrorCode.Equals(other.ErrorCode)))
		{
			return false;
		}
		if (HasErrorMessage != other.HasErrorMessage || (HasErrorMessage && !ErrorMessage.Equals(other.ErrorMessage)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static SwrveIdentifyFailed Deserialize(Stream stream, SwrveIdentifyFailed instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static SwrveIdentifyFailed DeserializeLengthDelimited(Stream stream)
	{
		SwrveIdentifyFailed instance = new SwrveIdentifyFailed();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static SwrveIdentifyFailed DeserializeLengthDelimited(Stream stream, SwrveIdentifyFailed instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static SwrveIdentifyFailed Deserialize(Stream stream, SwrveIdentifyFailed instance, long limit)
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
			case 82:
				instance.AppName = ProtocolParser.ReadString(stream);
				continue;
			default:
			{
				Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
				switch (key.Field)
				{
				case 0u:
					throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
				case 20u:
					if (key.WireType == Wire.Varint)
					{
						instance.AppId = (long)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 30u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.SwrveUserId = ProtocolParser.ReadString(stream);
					}
					break;
				case 40u:
					if (key.WireType == Wire.Varint)
					{
						instance.ErrorCode = (long)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 50u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ErrorMessage = ProtocolParser.ReadString(stream);
					}
					break;
				default:
					ProtocolParser.SkipKey(stream, key);
					break;
				}
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

	public static void Serialize(Stream stream, SwrveIdentifyFailed instance)
	{
		if (instance.HasAppName)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AppName));
		}
		if (instance.HasAppId)
		{
			stream.WriteByte(160);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.AppId);
		}
		if (instance.HasSwrveUserId)
		{
			stream.WriteByte(242);
			stream.WriteByte(1);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.SwrveUserId));
		}
		if (instance.HasErrorCode)
		{
			stream.WriteByte(192);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ErrorCode);
		}
		if (instance.HasErrorMessage)
		{
			stream.WriteByte(146);
			stream.WriteByte(3);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ErrorMessage));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasAppName)
		{
			size++;
			uint byteCount10 = (uint)Encoding.UTF8.GetByteCount(AppName);
			size += ProtocolParser.SizeOfUInt32(byteCount10) + byteCount10;
		}
		if (HasAppId)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)AppId);
		}
		if (HasSwrveUserId)
		{
			size += 2;
			uint byteCount30 = (uint)Encoding.UTF8.GetByteCount(SwrveUserId);
			size += ProtocolParser.SizeOfUInt32(byteCount30) + byteCount30;
		}
		if (HasErrorCode)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)ErrorCode);
		}
		if (HasErrorMessage)
		{
			size += 2;
			uint byteCount50 = (uint)Encoding.UTF8.GetByteCount(ErrorMessage);
			size += ProtocolParser.SizeOfUInt32(byteCount50) + byteCount50;
		}
		return size;
	}
}
