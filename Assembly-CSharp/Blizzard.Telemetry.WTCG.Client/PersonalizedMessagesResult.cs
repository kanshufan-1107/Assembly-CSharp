using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class PersonalizedMessagesResult : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasSuccess;

	private bool _Success;

	public bool HasMessageCount;

	private int _MessageCount;

	private List<string> _MessageUids = new List<string>();

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

	public bool Success
	{
		get
		{
			return _Success;
		}
		set
		{
			_Success = value;
			HasSuccess = true;
		}
	}

	public int MessageCount
	{
		get
		{
			return _MessageCount;
		}
		set
		{
			_MessageCount = value;
			HasMessageCount = true;
		}
	}

	public List<string> MessageUids
	{
		get
		{
			return _MessageUids;
		}
		set
		{
			_MessageUids = value;
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
		if (HasSuccess)
		{
			hash ^= Success.GetHashCode();
		}
		if (HasMessageCount)
		{
			hash ^= MessageCount.GetHashCode();
		}
		foreach (string i in MessageUids)
		{
			hash ^= i.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PersonalizedMessagesResult other))
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
		if (HasSuccess != other.HasSuccess || (HasSuccess && !Success.Equals(other.Success)))
		{
			return false;
		}
		if (HasMessageCount != other.HasMessageCount || (HasMessageCount && !MessageCount.Equals(other.MessageCount)))
		{
			return false;
		}
		if (MessageUids.Count != other.MessageUids.Count)
		{
			return false;
		}
		for (int i = 0; i < MessageUids.Count; i++)
		{
			if (!MessageUids[i].Equals(other.MessageUids[i]))
			{
				return false;
			}
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static PersonalizedMessagesResult Deserialize(Stream stream, PersonalizedMessagesResult instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PersonalizedMessagesResult DeserializeLengthDelimited(Stream stream)
	{
		PersonalizedMessagesResult instance = new PersonalizedMessagesResult();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static PersonalizedMessagesResult DeserializeLengthDelimited(Stream stream, PersonalizedMessagesResult instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static PersonalizedMessagesResult Deserialize(Stream stream, PersonalizedMessagesResult instance, long limit)
	{
		if (instance.MessageUids == null)
		{
			instance.MessageUids = new List<string>();
		}
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
				instance.Success = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				instance.MessageCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				instance.MessageUids.Add(ProtocolParser.ReadString(stream));
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

	public static void Serialize(Stream stream, PersonalizedMessagesResult instance)
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
		if (instance.HasSuccess)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.Success);
		}
		if (instance.HasMessageCount)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MessageCount);
		}
		if (instance.MessageUids.Count <= 0)
		{
			return;
		}
		foreach (string i5 in instance.MessageUids)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(i5));
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
		if (HasSuccess)
		{
			size++;
			size++;
		}
		if (HasMessageCount)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)MessageCount);
		}
		if (MessageUids.Count > 0)
		{
			foreach (string i5 in MessageUids)
			{
				size++;
				uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(i5);
				size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
			}
		}
		return size;
	}
}
