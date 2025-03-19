using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class Balance : IProtoBuf
{
	public bool HasName;

	private string _Name;

	public bool HasAmount;

	private double _Amount;

	public string Name
	{
		get
		{
			return _Name;
		}
		set
		{
			_Name = value;
			HasName = value != null;
		}
	}

	public double Amount
	{
		get
		{
			return _Amount;
		}
		set
		{
			_Amount = value;
			HasAmount = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasName)
		{
			hash ^= Name.GetHashCode();
		}
		if (HasAmount)
		{
			hash ^= Amount.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Balance other))
		{
			return false;
		}
		if (HasName != other.HasName || (HasName && !Name.Equals(other.Name)))
		{
			return false;
		}
		if (HasAmount != other.HasAmount || (HasAmount && !Amount.Equals(other.Amount)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static Balance Deserialize(Stream stream, Balance instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static Balance DeserializeLengthDelimited(Stream stream)
	{
		Balance instance = new Balance();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static Balance DeserializeLengthDelimited(Stream stream, Balance instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static Balance Deserialize(Stream stream, Balance instance, long limit)
	{
		BinaryReader br = new BinaryReader(stream);
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
				instance.Name = ProtocolParser.ReadString(stream);
				continue;
			case 17:
				instance.Amount = br.ReadDouble();
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

	public static void Serialize(Stream stream, Balance instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasName)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Name));
		}
		if (instance.HasAmount)
		{
			stream.WriteByte(17);
			bw.Write(instance.Amount);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasName)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(Name);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasAmount)
		{
			size++;
			size += 8;
		}
		return size;
	}
}
