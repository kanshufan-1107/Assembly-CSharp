using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class InitialDataRequested : IProtoBuf
{
	public bool HasVersion;

	private string _Version;

	public bool HasDataVersion;

	private int _DataVersion;

	public string Version
	{
		get
		{
			return _Version;
		}
		set
		{
			_Version = value;
			HasVersion = value != null;
		}
	}

	public int DataVersion
	{
		get
		{
			return _DataVersion;
		}
		set
		{
			_DataVersion = value;
			HasDataVersion = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasVersion)
		{
			hash ^= Version.GetHashCode();
		}
		if (HasDataVersion)
		{
			hash ^= DataVersion.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is InitialDataRequested other))
		{
			return false;
		}
		if (HasVersion != other.HasVersion || (HasVersion && !Version.Equals(other.Version)))
		{
			return false;
		}
		if (HasDataVersion != other.HasDataVersion || (HasDataVersion && !DataVersion.Equals(other.DataVersion)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static InitialDataRequested Deserialize(Stream stream, InitialDataRequested instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static InitialDataRequested DeserializeLengthDelimited(Stream stream)
	{
		InitialDataRequested instance = new InitialDataRequested();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static InitialDataRequested DeserializeLengthDelimited(Stream stream, InitialDataRequested instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static InitialDataRequested Deserialize(Stream stream, InitialDataRequested instance, long limit)
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
				instance.Version = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				instance.DataVersion = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, InitialDataRequested instance)
	{
		if (instance.HasVersion)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Version));
		}
		if (instance.HasDataVersion)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.DataVersion);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasVersion)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(Version);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasDataVersion)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)DataVersion);
		}
		return size;
	}
}
