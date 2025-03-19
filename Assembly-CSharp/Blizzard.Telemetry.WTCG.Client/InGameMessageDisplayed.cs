using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class InGameMessageDisplayed : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasMessageType;

	private string _MessageType;

	public bool HasUid;

	private string _Uid;

	public bool HasTitle;

	private string _Title;

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

	public string MessageType
	{
		get
		{
			return _MessageType;
		}
		set
		{
			_MessageType = value;
			HasMessageType = value != null;
		}
	}

	public string Uid
	{
		get
		{
			return _Uid;
		}
		set
		{
			_Uid = value;
			HasUid = value != null;
		}
	}

	public string Title
	{
		get
		{
			return _Title;
		}
		set
		{
			_Title = value;
			HasTitle = value != null;
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
		if (HasMessageType)
		{
			hash ^= MessageType.GetHashCode();
		}
		if (HasUid)
		{
			hash ^= Uid.GetHashCode();
		}
		if (HasTitle)
		{
			hash ^= Title.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is InGameMessageDisplayed other))
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
		if (HasMessageType != other.HasMessageType || (HasMessageType && !MessageType.Equals(other.MessageType)))
		{
			return false;
		}
		if (HasUid != other.HasUid || (HasUid && !Uid.Equals(other.Uid)))
		{
			return false;
		}
		if (HasTitle != other.HasTitle || (HasTitle && !Title.Equals(other.Title)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static InGameMessageDisplayed Deserialize(Stream stream, InGameMessageDisplayed instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static InGameMessageDisplayed DeserializeLengthDelimited(Stream stream)
	{
		InGameMessageDisplayed instance = new InGameMessageDisplayed();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static InGameMessageDisplayed DeserializeLengthDelimited(Stream stream, InGameMessageDisplayed instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static InGameMessageDisplayed Deserialize(Stream stream, InGameMessageDisplayed instance, long limit)
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
				instance.MessageType = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.Uid = ProtocolParser.ReadString(stream);
				continue;
			case 42:
				instance.Title = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, InGameMessageDisplayed instance)
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
		if (instance.HasMessageType)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.MessageType));
		}
		if (instance.HasUid)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Uid));
		}
		if (instance.HasTitle)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Title));
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
		if (HasMessageType)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(MessageType);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (HasUid)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(Uid);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasTitle)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(Title);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		return size;
	}
}
