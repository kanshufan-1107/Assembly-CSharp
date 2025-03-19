using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class PushNotificationSystemNotificationDeleted : IProtoBuf
{
	public bool HasAppName;

	private string _AppName;

	public bool HasUserId;

	private string _UserId;

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

	public string UserId
	{
		get
		{
			return _UserId;
		}
		set
		{
			_UserId = value;
			HasUserId = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasAppName)
		{
			hash ^= AppName.GetHashCode();
		}
		if (HasUserId)
		{
			hash ^= UserId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PushNotificationSystemNotificationDeleted other))
		{
			return false;
		}
		if (HasAppName != other.HasAppName || (HasAppName && !AppName.Equals(other.AppName)))
		{
			return false;
		}
		if (HasUserId != other.HasUserId || (HasUserId && !UserId.Equals(other.UserId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static PushNotificationSystemNotificationDeleted Deserialize(Stream stream, PushNotificationSystemNotificationDeleted instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PushNotificationSystemNotificationDeleted DeserializeLengthDelimited(Stream stream)
	{
		PushNotificationSystemNotificationDeleted instance = new PushNotificationSystemNotificationDeleted();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static PushNotificationSystemNotificationDeleted DeserializeLengthDelimited(Stream stream, PushNotificationSystemNotificationDeleted instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static PushNotificationSystemNotificationDeleted Deserialize(Stream stream, PushNotificationSystemNotificationDeleted instance, long limit)
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
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.UserId = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, PushNotificationSystemNotificationDeleted instance)
	{
		if (instance.HasAppName)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AppName));
		}
		if (instance.HasUserId)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.UserId));
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
		if (HasUserId)
		{
			size += 2;
			uint byteCount20 = (uint)Encoding.UTF8.GetByteCount(UserId);
			size += ProtocolParser.SizeOfUInt32(byteCount20) + byteCount20;
		}
		return size;
	}
}
