using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class DeckCopied : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasDeckId;

	private long _DeckId;

	public bool HasDeckHash;

	private string _DeckHash;

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

	public long DeckId
	{
		get
		{
			return _DeckId;
		}
		set
		{
			_DeckId = value;
			HasDeckId = true;
		}
	}

	public string DeckHash
	{
		get
		{
			return _DeckHash;
		}
		set
		{
			_DeckHash = value;
			HasDeckHash = value != null;
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
		if (HasDeckId)
		{
			hash ^= DeckId.GetHashCode();
		}
		if (HasDeckHash)
		{
			hash ^= DeckHash.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DeckCopied other))
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
		if (HasDeckId != other.HasDeckId || (HasDeckId && !DeckId.Equals(other.DeckId)))
		{
			return false;
		}
		if (HasDeckHash != other.HasDeckHash || (HasDeckHash && !DeckHash.Equals(other.DeckHash)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static DeckCopied Deserialize(Stream stream, DeckCopied instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static DeckCopied DeserializeLengthDelimited(Stream stream)
	{
		DeckCopied instance = new DeckCopied();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static DeckCopied DeserializeLengthDelimited(Stream stream, DeckCopied instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static DeckCopied Deserialize(Stream stream, DeckCopied instance, long limit)
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
			case 24:
				instance.DeckId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				instance.DeckHash = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, DeckCopied instance)
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
		if (instance.HasDeckId)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.DeckId);
		}
		if (instance.HasDeckHash)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeckHash));
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
		if (HasDeckId)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)DeckId);
		}
		if (HasDeckHash)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(DeckHash);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		return size;
	}
}
