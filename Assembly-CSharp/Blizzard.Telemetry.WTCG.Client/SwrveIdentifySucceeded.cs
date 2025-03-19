using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class SwrveIdentifySucceeded : IProtoBuf
{
	public bool HasAppName;

	private string _AppName;

	public bool HasAppId;

	private long _AppId;

	public bool HasSwrveUserId;

	private string _SwrveUserId;

	public bool HasStatusCode;

	private string _StatusCode;

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

	public string StatusCode
	{
		get
		{
			return _StatusCode;
		}
		set
		{
			_StatusCode = value;
			HasStatusCode = value != null;
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
		if (HasStatusCode)
		{
			hash ^= StatusCode.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SwrveIdentifySucceeded other))
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
		if (HasStatusCode != other.HasStatusCode || (HasStatusCode && !StatusCode.Equals(other.StatusCode)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static SwrveIdentifySucceeded Deserialize(Stream stream, SwrveIdentifySucceeded instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static SwrveIdentifySucceeded DeserializeLengthDelimited(Stream stream)
	{
		SwrveIdentifySucceeded instance = new SwrveIdentifySucceeded();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static SwrveIdentifySucceeded DeserializeLengthDelimited(Stream stream, SwrveIdentifySucceeded instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static SwrveIdentifySucceeded Deserialize(Stream stream, SwrveIdentifySucceeded instance, long limit)
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
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.StatusCode = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, SwrveIdentifySucceeded instance)
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
		if (instance.HasStatusCode)
		{
			stream.WriteByte(194);
			stream.WriteByte(2);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.StatusCode));
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
		if (HasStatusCode)
		{
			size += 2;
			uint byteCount40 = (uint)Encoding.UTF8.GetByteCount(StatusCode);
			size += ProtocolParser.SizeOfUInt32(byteCount40) + byteCount40;
		}
		return size;
	}
}
