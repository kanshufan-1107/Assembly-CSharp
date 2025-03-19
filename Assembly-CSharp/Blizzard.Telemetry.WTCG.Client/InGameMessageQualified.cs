using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class InGameMessageQualified : IProtoBuf
{
	public bool HasPlayer;

	private Player _Player;

	public bool HasMessageType;

	private string _MessageType;

	private List<string> _Uids = new List<string>();

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

	public List<string> Uids
	{
		get
		{
			return _Uids;
		}
		set
		{
			_Uids = value;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasMessageType)
		{
			hash ^= MessageType.GetHashCode();
		}
		foreach (string i in Uids)
		{
			hash ^= i.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is InGameMessageQualified other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasMessageType != other.HasMessageType || (HasMessageType && !MessageType.Equals(other.MessageType)))
		{
			return false;
		}
		if (Uids.Count != other.Uids.Count)
		{
			return false;
		}
		for (int i = 0; i < Uids.Count; i++)
		{
			if (!Uids[i].Equals(other.Uids[i]))
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

	public static InGameMessageQualified Deserialize(Stream stream, InGameMessageQualified instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static InGameMessageQualified DeserializeLengthDelimited(Stream stream)
	{
		InGameMessageQualified instance = new InGameMessageQualified();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static InGameMessageQualified DeserializeLengthDelimited(Stream stream, InGameMessageQualified instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static InGameMessageQualified Deserialize(Stream stream, InGameMessageQualified instance, long limit)
	{
		if (instance.Uids == null)
		{
			instance.Uids = new List<string>();
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
			case 26:
				instance.MessageType = ProtocolParser.ReadString(stream);
				continue;
			case 42:
				instance.Uids.Add(ProtocolParser.ReadString(stream));
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

	public static void Serialize(Stream stream, InGameMessageQualified instance)
	{
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasMessageType)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.MessageType));
		}
		if (instance.Uids.Count <= 0)
		{
			return;
		}
		foreach (string i5 in instance.Uids)
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
		if (HasMessageType)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(MessageType);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		if (Uids.Count > 0)
		{
			foreach (string i5 in Uids)
			{
				size++;
				uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(i5);
				size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
			}
		}
		return size;
	}
}
