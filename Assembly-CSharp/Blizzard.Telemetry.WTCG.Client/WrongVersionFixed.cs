using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class WrongVersionFixed : IProtoBuf
{
	public bool HasLiveVersion;

	private string _LiveVersion;

	public bool HasReties;

	private int _Reties;

	public bool HasSucceeded;

	private bool _Succeeded;

	public string LiveVersion
	{
		get
		{
			return _LiveVersion;
		}
		set
		{
			_LiveVersion = value;
			HasLiveVersion = value != null;
		}
	}

	public int Reties
	{
		get
		{
			return _Reties;
		}
		set
		{
			_Reties = value;
			HasReties = true;
		}
	}

	public bool Succeeded
	{
		get
		{
			return _Succeeded;
		}
		set
		{
			_Succeeded = value;
			HasSucceeded = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasLiveVersion)
		{
			hash ^= LiveVersion.GetHashCode();
		}
		if (HasReties)
		{
			hash ^= Reties.GetHashCode();
		}
		if (HasSucceeded)
		{
			hash ^= Succeeded.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is WrongVersionFixed other))
		{
			return false;
		}
		if (HasLiveVersion != other.HasLiveVersion || (HasLiveVersion && !LiveVersion.Equals(other.LiveVersion)))
		{
			return false;
		}
		if (HasReties != other.HasReties || (HasReties && !Reties.Equals(other.Reties)))
		{
			return false;
		}
		if (HasSucceeded != other.HasSucceeded || (HasSucceeded && !Succeeded.Equals(other.Succeeded)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static WrongVersionFixed Deserialize(Stream stream, WrongVersionFixed instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static WrongVersionFixed DeserializeLengthDelimited(Stream stream)
	{
		WrongVersionFixed instance = new WrongVersionFixed();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static WrongVersionFixed DeserializeLengthDelimited(Stream stream, WrongVersionFixed instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static WrongVersionFixed Deserialize(Stream stream, WrongVersionFixed instance, long limit)
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
				instance.LiveVersion = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				instance.Reties = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				instance.Succeeded = ProtocolParser.ReadBool(stream);
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

	public static void Serialize(Stream stream, WrongVersionFixed instance)
	{
		if (instance.HasLiveVersion)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.LiveVersion));
		}
		if (instance.HasReties)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Reties);
		}
		if (instance.HasSucceeded)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.Succeeded);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasLiveVersion)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(LiveVersion);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasReties)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Reties);
		}
		if (HasSucceeded)
		{
			size++;
			size++;
		}
		return size;
	}
}
