using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class OldVersionInStore : IProtoBuf
{
	public bool HasLiveVersion;

	private string _LiveVersion;

	public bool HasUpdatestat;

	private string _Updatestat;

	public bool HasSilentGo;

	private bool _SilentGo;

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

	public string Updatestat
	{
		get
		{
			return _Updatestat;
		}
		set
		{
			_Updatestat = value;
			HasUpdatestat = value != null;
		}
	}

	public bool SilentGo
	{
		get
		{
			return _SilentGo;
		}
		set
		{
			_SilentGo = value;
			HasSilentGo = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasLiveVersion)
		{
			hash ^= LiveVersion.GetHashCode();
		}
		if (HasUpdatestat)
		{
			hash ^= Updatestat.GetHashCode();
		}
		if (HasSilentGo)
		{
			hash ^= SilentGo.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is OldVersionInStore other))
		{
			return false;
		}
		if (HasLiveVersion != other.HasLiveVersion || (HasLiveVersion && !LiveVersion.Equals(other.LiveVersion)))
		{
			return false;
		}
		if (HasUpdatestat != other.HasUpdatestat || (HasUpdatestat && !Updatestat.Equals(other.Updatestat)))
		{
			return false;
		}
		if (HasSilentGo != other.HasSilentGo || (HasSilentGo && !SilentGo.Equals(other.SilentGo)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static OldVersionInStore Deserialize(Stream stream, OldVersionInStore instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static OldVersionInStore DeserializeLengthDelimited(Stream stream)
	{
		OldVersionInStore instance = new OldVersionInStore();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static OldVersionInStore DeserializeLengthDelimited(Stream stream, OldVersionInStore instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static OldVersionInStore Deserialize(Stream stream, OldVersionInStore instance, long limit)
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
			case 18:
				instance.Updatestat = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.SilentGo = ProtocolParser.ReadBool(stream);
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

	public static void Serialize(Stream stream, OldVersionInStore instance)
	{
		if (instance.HasLiveVersion)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.LiveVersion));
		}
		if (instance.HasUpdatestat)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Updatestat));
		}
		if (instance.HasSilentGo)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.SilentGo);
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
		if (HasUpdatestat)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(Updatestat);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasSilentGo)
		{
			size++;
			size++;
		}
		return size;
	}
}
