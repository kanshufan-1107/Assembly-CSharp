using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class InitTraceroute : IProtoBuf
{
	public bool HasSuccess;

	private bool _Success;

	public bool HasInitStatus;

	private string _InitStatus;

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

	public string InitStatus
	{
		get
		{
			return _InitStatus;
		}
		set
		{
			_InitStatus = value;
			HasInitStatus = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasSuccess)
		{
			hash ^= Success.GetHashCode();
		}
		if (HasInitStatus)
		{
			hash ^= InitStatus.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is InitTraceroute other))
		{
			return false;
		}
		if (HasSuccess != other.HasSuccess || (HasSuccess && !Success.Equals(other.Success)))
		{
			return false;
		}
		if (HasInitStatus != other.HasInitStatus || (HasInitStatus && !InitStatus.Equals(other.InitStatus)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static InitTraceroute Deserialize(Stream stream, InitTraceroute instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static InitTraceroute DeserializeLengthDelimited(Stream stream)
	{
		InitTraceroute instance = new InitTraceroute();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static InitTraceroute DeserializeLengthDelimited(Stream stream, InitTraceroute instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static InitTraceroute Deserialize(Stream stream, InitTraceroute instance, long limit)
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
			case 8:
				instance.Success = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				instance.InitStatus = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, InitTraceroute instance)
	{
		if (instance.HasSuccess)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.Success);
		}
		if (instance.HasInitStatus)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.InitStatus));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasSuccess)
		{
			size++;
			size++;
		}
		if (HasInitStatus)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(InitStatus);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		return size;
	}
}
