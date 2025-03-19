using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class AppStart : IProtoBuf
{
	public bool HasTestType;

	private string _TestType;

	public bool HasDuration;

	private float _Duration;

	public bool HasClientChangelist;

	private string _ClientChangelist;

	public string TestType
	{
		get
		{
			return _TestType;
		}
		set
		{
			_TestType = value;
			HasTestType = value != null;
		}
	}

	public float Duration
	{
		get
		{
			return _Duration;
		}
		set
		{
			_Duration = value;
			HasDuration = true;
		}
	}

	public string ClientChangelist
	{
		get
		{
			return _ClientChangelist;
		}
		set
		{
			_ClientChangelist = value;
			HasClientChangelist = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasTestType)
		{
			hash ^= TestType.GetHashCode();
		}
		if (HasDuration)
		{
			hash ^= Duration.GetHashCode();
		}
		if (HasClientChangelist)
		{
			hash ^= ClientChangelist.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AppStart other))
		{
			return false;
		}
		if (HasTestType != other.HasTestType || (HasTestType && !TestType.Equals(other.TestType)))
		{
			return false;
		}
		if (HasDuration != other.HasDuration || (HasDuration && !Duration.Equals(other.Duration)))
		{
			return false;
		}
		if (HasClientChangelist != other.HasClientChangelist || (HasClientChangelist && !ClientChangelist.Equals(other.ClientChangelist)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static AppStart Deserialize(Stream stream, AppStart instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static AppStart DeserializeLengthDelimited(Stream stream)
	{
		AppStart instance = new AppStart();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static AppStart DeserializeLengthDelimited(Stream stream, AppStart instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static AppStart Deserialize(Stream stream, AppStart instance, long limit)
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
				instance.TestType = ProtocolParser.ReadString(stream);
				continue;
			case 21:
				instance.Duration = br.ReadSingle();
				continue;
			case 26:
				instance.ClientChangelist = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, AppStart instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasTestType)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.TestType));
		}
		if (instance.HasDuration)
		{
			stream.WriteByte(21);
			bw.Write(instance.Duration);
		}
		if (instance.HasClientChangelist)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ClientChangelist));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasTestType)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(TestType);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasDuration)
		{
			size++;
			size += 4;
		}
		if (HasClientChangelist)
		{
			size++;
			uint byteCount3 = (uint)Encoding.UTF8.GetByteCount(ClientChangelist);
			size += ProtocolParser.SizeOfUInt32(byteCount3) + byteCount3;
		}
		return size;
	}
}
