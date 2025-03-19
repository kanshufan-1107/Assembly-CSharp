using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class SwrveUserIdChanged : IProtoBuf
{
	public bool HasAppName;

	private string _AppName;

	public bool HasAppId;

	private long _AppId;

	public bool HasPreviousSwrveUserId;

	private string _PreviousSwrveUserId;

	public bool HasCurrentSwrveUserId;

	private string _CurrentSwrveUserId;

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

	public string PreviousSwrveUserId
	{
		get
		{
			return _PreviousSwrveUserId;
		}
		set
		{
			_PreviousSwrveUserId = value;
			HasPreviousSwrveUserId = value != null;
		}
	}

	public string CurrentSwrveUserId
	{
		get
		{
			return _CurrentSwrveUserId;
		}
		set
		{
			_CurrentSwrveUserId = value;
			HasCurrentSwrveUserId = value != null;
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
		if (HasPreviousSwrveUserId)
		{
			hash ^= PreviousSwrveUserId.GetHashCode();
		}
		if (HasCurrentSwrveUserId)
		{
			hash ^= CurrentSwrveUserId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SwrveUserIdChanged other))
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
		if (HasPreviousSwrveUserId != other.HasPreviousSwrveUserId || (HasPreviousSwrveUserId && !PreviousSwrveUserId.Equals(other.PreviousSwrveUserId)))
		{
			return false;
		}
		if (HasCurrentSwrveUserId != other.HasCurrentSwrveUserId || (HasCurrentSwrveUserId && !CurrentSwrveUserId.Equals(other.CurrentSwrveUserId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static SwrveUserIdChanged Deserialize(Stream stream, SwrveUserIdChanged instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static SwrveUserIdChanged DeserializeLengthDelimited(Stream stream)
	{
		SwrveUserIdChanged instance = new SwrveUserIdChanged();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static SwrveUserIdChanged DeserializeLengthDelimited(Stream stream, SwrveUserIdChanged instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static SwrveUserIdChanged Deserialize(Stream stream, SwrveUserIdChanged instance, long limit)
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
						instance.PreviousSwrveUserId = ProtocolParser.ReadString(stream);
					}
					break;
				case 40u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.CurrentSwrveUserId = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, SwrveUserIdChanged instance)
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
		if (instance.HasPreviousSwrveUserId)
		{
			stream.WriteByte(242);
			stream.WriteByte(1);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.PreviousSwrveUserId));
		}
		if (instance.HasCurrentSwrveUserId)
		{
			stream.WriteByte(194);
			stream.WriteByte(2);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.CurrentSwrveUserId));
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
		if (HasPreviousSwrveUserId)
		{
			size += 2;
			uint byteCount30 = (uint)Encoding.UTF8.GetByteCount(PreviousSwrveUserId);
			size += ProtocolParser.SizeOfUInt32(byteCount30) + byteCount30;
		}
		if (HasCurrentSwrveUserId)
		{
			size += 2;
			uint byteCount40 = (uint)Encoding.UTF8.GetByteCount(CurrentSwrveUserId);
			size += ProtocolParser.SizeOfUInt32(byteCount40) + byteCount40;
		}
		return size;
	}
}
