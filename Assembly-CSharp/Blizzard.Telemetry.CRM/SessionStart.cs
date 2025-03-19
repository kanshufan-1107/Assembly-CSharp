using System.IO;
using System.Text;

namespace Blizzard.Telemetry.CRM;

public class SessionStart : IProtoBuf
{
	public bool HasEventPayload;

	private string _EventPayload;

	public bool HasApplicationId;

	private string _ApplicationId;

	public string EventPayload
	{
		get
		{
			return _EventPayload;
		}
		set
		{
			_EventPayload = value;
			HasEventPayload = value != null;
		}
	}

	public string ApplicationId
	{
		get
		{
			return _ApplicationId;
		}
		set
		{
			_ApplicationId = value;
			HasApplicationId = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasEventPayload)
		{
			hash ^= EventPayload.GetHashCode();
		}
		if (HasApplicationId)
		{
			hash ^= ApplicationId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SessionStart other))
		{
			return false;
		}
		if (HasEventPayload != other.HasEventPayload || (HasEventPayload && !EventPayload.Equals(other.EventPayload)))
		{
			return false;
		}
		if (HasApplicationId != other.HasApplicationId || (HasApplicationId && !ApplicationId.Equals(other.ApplicationId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static SessionStart Deserialize(Stream stream, SessionStart instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static SessionStart Deserialize(Stream stream, SessionStart instance, long limit)
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
			if (keyByte == -1)
			{
				if (limit < 0)
				{
					break;
				}
				throw new EndOfStreamException();
			}
			Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
			switch (key.Field)
			{
			case 0u:
				throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
			case 20u:
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.EventPayload = ProtocolParser.ReadString(stream);
				}
				break;
			case 30u:
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.ApplicationId = ProtocolParser.ReadString(stream);
				}
				break;
			default:
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public void Serialize(Stream stream)
	{
		Serialize(stream, this);
	}

	public static void Serialize(Stream stream, SessionStart instance)
	{
		if (instance.HasEventPayload)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.EventPayload));
		}
		if (instance.HasApplicationId)
		{
			stream.WriteByte(242);
			stream.WriteByte(1);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ApplicationId));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasEventPayload)
		{
			size += 2;
			uint byteCount20 = (uint)Encoding.UTF8.GetByteCount(EventPayload);
			size += ProtocolParser.SizeOfUInt32(byteCount20) + byteCount20;
		}
		if (HasApplicationId)
		{
			size += 2;
			uint byteCount30 = (uint)Encoding.UTF8.GetByteCount(ApplicationId);
			size += ProtocolParser.SizeOfUInt32(byteCount30) + byteCount30;
		}
		return size;
	}
}
