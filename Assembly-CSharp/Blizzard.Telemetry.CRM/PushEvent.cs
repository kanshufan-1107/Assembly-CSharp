using System.IO;
using System.Text;

namespace Blizzard.Telemetry.CRM;

public class PushEvent : IProtoBuf
{
	public bool HasCampaignId;

	private string _CampaignId;

	public bool HasEventPayload;

	private string _EventPayload;

	public bool HasApplicationId;

	private string _ApplicationId;

	public string CampaignId
	{
		get
		{
			return _CampaignId;
		}
		set
		{
			_CampaignId = value;
			HasCampaignId = value != null;
		}
	}

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
		if (HasCampaignId)
		{
			hash ^= CampaignId.GetHashCode();
		}
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
		if (!(obj is PushEvent other))
		{
			return false;
		}
		if (HasCampaignId != other.HasCampaignId || (HasCampaignId && !CampaignId.Equals(other.CampaignId)))
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

	public static PushEvent Deserialize(Stream stream, PushEvent instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PushEvent Deserialize(Stream stream, PushEvent instance, long limit)
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
				instance.CampaignId = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, PushEvent instance)
	{
		if (instance.HasCampaignId)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.CampaignId));
		}
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
		if (HasCampaignId)
		{
			size++;
			uint byteCount10 = (uint)Encoding.UTF8.GetByteCount(CampaignId);
			size += ProtocolParser.SizeOfUInt32(byteCount10) + byteCount10;
		}
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
