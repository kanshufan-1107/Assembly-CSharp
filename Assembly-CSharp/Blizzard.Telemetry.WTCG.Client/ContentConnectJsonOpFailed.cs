using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ContentConnectJsonOpFailed : IProtoBuf
{
	public enum JsonOp
	{
		WRITE = 100,
		READ
	}

	public bool HasPlayer;

	private Player _Player;

	public bool HasOp;

	private JsonOp _Op;

	public bool HasFilename;

	private string _Filename;

	public bool HasReason;

	private string _Reason;

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

	public JsonOp Op
	{
		get
		{
			return _Op;
		}
		set
		{
			_Op = value;
			HasOp = true;
		}
	}

	public string Filename
	{
		get
		{
			return _Filename;
		}
		set
		{
			_Filename = value;
			HasFilename = value != null;
		}
	}

	public string Reason
	{
		get
		{
			return _Reason;
		}
		set
		{
			_Reason = value;
			HasReason = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPlayer)
		{
			hash ^= Player.GetHashCode();
		}
		if (HasOp)
		{
			hash ^= Op.GetHashCode();
		}
		if (HasFilename)
		{
			hash ^= Filename.GetHashCode();
		}
		if (HasReason)
		{
			hash ^= Reason.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ContentConnectJsonOpFailed other))
		{
			return false;
		}
		if (HasPlayer != other.HasPlayer || (HasPlayer && !Player.Equals(other.Player)))
		{
			return false;
		}
		if (HasOp != other.HasOp || (HasOp && !Op.Equals(other.Op)))
		{
			return false;
		}
		if (HasFilename != other.HasFilename || (HasFilename && !Filename.Equals(other.Filename)))
		{
			return false;
		}
		if (HasReason != other.HasReason || (HasReason && !Reason.Equals(other.Reason)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ContentConnectJsonOpFailed Deserialize(Stream stream, ContentConnectJsonOpFailed instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ContentConnectJsonOpFailed DeserializeLengthDelimited(Stream stream)
	{
		ContentConnectJsonOpFailed instance = new ContentConnectJsonOpFailed();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ContentConnectJsonOpFailed DeserializeLengthDelimited(Stream stream, ContentConnectJsonOpFailed instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ContentConnectJsonOpFailed Deserialize(Stream stream, ContentConnectJsonOpFailed instance, long limit)
	{
		instance.Op = JsonOp.WRITE;
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
			case 24:
				instance.Op = (JsonOp)ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				instance.Filename = ProtocolParser.ReadString(stream);
				continue;
			case 58:
				instance.Reason = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, ContentConnectJsonOpFailed instance)
	{
		if (instance.HasPlayer)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Player.GetSerializedSize());
			Player.Serialize(stream, instance.Player);
		}
		if (instance.HasOp)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Op);
		}
		if (instance.HasFilename)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Filename));
		}
		if (instance.HasReason)
		{
			stream.WriteByte(58);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Reason));
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
		if (HasOp)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Op);
		}
		if (HasFilename)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(Filename);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		if (HasReason)
		{
			size++;
			uint byteCount7 = (uint)Encoding.UTF8.GetByteCount(Reason);
			size += ProtocolParser.SizeOfUInt32(byteCount7) + byteCount7;
		}
		return size;
	}
}
