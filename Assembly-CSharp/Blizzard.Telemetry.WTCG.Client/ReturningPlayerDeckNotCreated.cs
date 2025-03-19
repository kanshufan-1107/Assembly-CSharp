using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class ReturningPlayerDeckNotCreated : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasABGroup;

	private uint _ABGroup;

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

	public uint ABGroup
	{
		get
		{
			return _ABGroup;
		}
		set
		{
			_ABGroup = value;
			HasABGroup = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasABGroup)
		{
			hash ^= ABGroup.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ReturningPlayerDeckNotCreated other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasABGroup != other.HasABGroup || (HasABGroup && !ABGroup.Equals(other.ABGroup)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ReturningPlayerDeckNotCreated Deserialize(Stream stream, ReturningPlayerDeckNotCreated instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ReturningPlayerDeckNotCreated DeserializeLengthDelimited(Stream stream)
	{
		ReturningPlayerDeckNotCreated instance = new ReturningPlayerDeckNotCreated();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ReturningPlayerDeckNotCreated DeserializeLengthDelimited(Stream stream, ReturningPlayerDeckNotCreated instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ReturningPlayerDeckNotCreated Deserialize(Stream stream, ReturningPlayerDeckNotCreated instance, long limit)
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
			case 16:
				instance.ABGroup = ProtocolParser.ReadUInt32(stream);
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

	public static void Serialize(Stream stream, ReturningPlayerDeckNotCreated instance)
	{
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasABGroup)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.ABGroup);
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
		if (HasABGroup)
		{
			size++;
			size += ProtocolParser.SizeOfUInt32(ABGroup);
		}
		return size;
	}
}
