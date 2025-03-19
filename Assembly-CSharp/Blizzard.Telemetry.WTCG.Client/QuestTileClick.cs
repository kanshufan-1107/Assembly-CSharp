using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class QuestTileClick : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasQuestID;

	private int _QuestID;

	public bool HasDisplayContext;

	private DisplayContext _DisplayContext;

	public bool HasClickType;

	private QuestTileClickType _ClickType;

	public bool HasDeepLinkValue;

	private string _DeepLinkValue;

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

	public int QuestID
	{
		get
		{
			return _QuestID;
		}
		set
		{
			_QuestID = value;
			HasQuestID = true;
		}
	}

	public DisplayContext DisplayContext
	{
		get
		{
			return _DisplayContext;
		}
		set
		{
			_DisplayContext = value;
			HasDisplayContext = true;
		}
	}

	public QuestTileClickType ClickType
	{
		get
		{
			return _ClickType;
		}
		set
		{
			_ClickType = value;
			HasClickType = true;
		}
	}

	public string DeepLinkValue
	{
		get
		{
			return _DeepLinkValue;
		}
		set
		{
			_DeepLinkValue = value;
			HasDeepLinkValue = value != null;
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
		if (HasQuestID)
		{
			hash ^= QuestID.GetHashCode();
		}
		if (HasDisplayContext)
		{
			hash ^= DisplayContext.GetHashCode();
		}
		if (HasClickType)
		{
			hash ^= ClickType.GetHashCode();
		}
		if (HasDeepLinkValue)
		{
			hash ^= DeepLinkValue.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is QuestTileClick other))
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
		if (HasQuestID != other.HasQuestID || (HasQuestID && !QuestID.Equals(other.QuestID)))
		{
			return false;
		}
		if (HasDisplayContext != other.HasDisplayContext || (HasDisplayContext && !DisplayContext.Equals(other.DisplayContext)))
		{
			return false;
		}
		if (HasClickType != other.HasClickType || (HasClickType && !ClickType.Equals(other.ClickType)))
		{
			return false;
		}
		if (HasDeepLinkValue != other.HasDeepLinkValue || (HasDeepLinkValue && !DeepLinkValue.Equals(other.DeepLinkValue)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static QuestTileClick Deserialize(Stream stream, QuestTileClick instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static QuestTileClick DeserializeLengthDelimited(Stream stream)
	{
		QuestTileClick instance = new QuestTileClick();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static QuestTileClick DeserializeLengthDelimited(Stream stream, QuestTileClick instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static QuestTileClick Deserialize(Stream stream, QuestTileClick instance, long limit)
	{
		instance.DisplayContext = DisplayContext.DC_CONTEXT_UNKNOWN;
		instance.ClickType = QuestTileClickType.QTCT_UNKNOWN;
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
			case 24:
				instance.QuestID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.DisplayContext = (DisplayContext)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				instance.ClickType = (QuestTileClickType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 50:
				instance.DeepLinkValue = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, QuestTileClick instance)
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
		if (instance.HasQuestID)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.QuestID);
		}
		if (instance.HasDisplayContext)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.DisplayContext);
		}
		if (instance.HasClickType)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ClickType);
		}
		if (instance.HasDeepLinkValue)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeepLinkValue));
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
		if (HasQuestID)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)QuestID);
		}
		if (HasDisplayContext)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)DisplayContext);
		}
		if (HasClickType)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ClickType);
		}
		if (HasDeepLinkValue)
		{
			size++;
			uint byteCount6 = (uint)Encoding.UTF8.GetByteCount(DeepLinkValue);
			size += ProtocolParser.SizeOfUInt32(byteCount6) + byteCount6;
		}
		return size;
	}
}
