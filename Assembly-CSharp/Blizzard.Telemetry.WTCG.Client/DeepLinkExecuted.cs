using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class DeepLinkExecuted : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasDeepLink;

	private string _DeepLink;

	public bool HasSource;

	private string _Source;

	public bool HasCompleted;

	private bool _Completed;

	public bool HasQuestId;

	private int _QuestId;

	public Player Player
	{
		get
		{
			return _Player;
		}
		set
		{
			_Player = value;
			HasPlayer = value != null;
		}
	}

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

	public string DeepLink
	{
		get
		{
			return _DeepLink;
		}
		set
		{
			_DeepLink = value;
			HasDeepLink = value != null;
		}
	}

	public string Source
	{
		get
		{
			return _Source;
		}
		set
		{
			_Source = value;
			HasSource = value != null;
		}
	}

	public bool Completed
	{
		get
		{
			return _Completed;
		}
		set
		{
			_Completed = value;
			HasCompleted = true;
		}
	}

	public int QuestId
	{
		get
		{
			return _QuestId;
		}
		set
		{
			_QuestId = value;
			HasQuestId = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasDeepLink)
		{
			hash ^= DeepLink.GetHashCode();
		}
		if (HasSource)
		{
			hash ^= Source.GetHashCode();
		}
		if (HasCompleted)
		{
			hash ^= Completed.GetHashCode();
		}
		if (HasQuestId)
		{
			hash ^= QuestId.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DeepLinkExecuted other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasDeepLink != other.HasDeepLink || (HasDeepLink && !DeepLink.Equals(other.DeepLink)))
		{
			return false;
		}
		if (HasSource != other.HasSource || (HasSource && !Source.Equals(other.Source)))
		{
			return false;
		}
		if (HasCompleted != other.HasCompleted || (HasCompleted && !Completed.Equals(other.Completed)))
		{
			return false;
		}
		if (HasQuestId != other.HasQuestId || (HasQuestId && !QuestId.Equals(other.QuestId)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static DeepLinkExecuted Deserialize(Stream stream, DeepLinkExecuted instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static DeepLinkExecuted DeserializeLengthDelimited(Stream stream)
	{
		DeepLinkExecuted instance = new DeepLinkExecuted();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static DeepLinkExecuted DeserializeLengthDelimited(Stream stream, DeepLinkExecuted instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static DeepLinkExecuted Deserialize(Stream stream, DeepLinkExecuted instance, long limit)
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
			case 10:
				if (instance.Player == null)
				{
					instance.Player = Player.DeserializeLengthDelimited(stream);
				}
				else
				{
					Player.DeserializeLengthDelimited(stream, instance.Player);
				}
				continue;
			case 18:
				if (instance.DeviceInfo == null)
				{
					instance.DeviceInfo = DeviceInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					DeviceInfo.DeserializeLengthDelimited(stream, instance.DeviceInfo);
				}
				continue;
			case 26:
				instance.DeepLink = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.Source = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				instance.Completed = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				instance.QuestId = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, DeepLinkExecuted instance)
	{
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasDeepLink)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeepLink));
		}
		if (instance.HasSource)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Source));
		}
		if (instance.HasCompleted)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.Completed);
		}
		if (instance.HasQuestId)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.QuestId);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasPlayer)
		{
			size++;
			uint size2 = Player.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		if (HasDeviceInfo)
		{
			size++;
			uint size3 = DeviceInfo.GetSerializedSize();
			size += size3 + ProtocolParser.SizeOfUInt32(size3);
		}
		if (HasDeepLink)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(DeepLink);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasSource)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(Source);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasCompleted)
		{
			size++;
			size++;
		}
		if (HasQuestId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)QuestId);
		}
		return size;
	}
}
