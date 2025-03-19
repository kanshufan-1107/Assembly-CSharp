using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class UncaughtException : IProtoBuf
{
	public bool HasStackTrace;

	private string _StackTrace;

	public bool HasAndroidModel;

	private string _AndroidModel;

	public bool HasAndroidSdkVersion;

	private uint _AndroidSdkVersion;

	public string StackTrace
	{
		get
		{
			return _StackTrace;
		}
		set
		{
			_StackTrace = value;
			HasStackTrace = value != null;
		}
	}

	public string AndroidModel
	{
		get
		{
			return _AndroidModel;
		}
		set
		{
			_AndroidModel = value;
			HasAndroidModel = value != null;
		}
	}

	public uint AndroidSdkVersion
	{
		get
		{
			return _AndroidSdkVersion;
		}
		set
		{
			_AndroidSdkVersion = value;
			HasAndroidSdkVersion = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasStackTrace)
		{
			hash ^= StackTrace.GetHashCode();
		}
		if (HasAndroidModel)
		{
			hash ^= AndroidModel.GetHashCode();
		}
		if (HasAndroidSdkVersion)
		{
			hash ^= AndroidSdkVersion.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is UncaughtException other))
		{
			return false;
		}
		if (HasStackTrace != other.HasStackTrace || (HasStackTrace && !StackTrace.Equals(other.StackTrace)))
		{
			return false;
		}
		if (HasAndroidModel != other.HasAndroidModel || (HasAndroidModel && !AndroidModel.Equals(other.AndroidModel)))
		{
			return false;
		}
		if (HasAndroidSdkVersion != other.HasAndroidSdkVersion || (HasAndroidSdkVersion && !AndroidSdkVersion.Equals(other.AndroidSdkVersion)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static UncaughtException Deserialize(Stream stream, UncaughtException instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static UncaughtException DeserializeLengthDelimited(Stream stream)
	{
		UncaughtException instance = new UncaughtException();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static UncaughtException DeserializeLengthDelimited(Stream stream, UncaughtException instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static UncaughtException Deserialize(Stream stream, UncaughtException instance, long limit)
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
				instance.StackTrace = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				instance.AndroidModel = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.AndroidSdkVersion = ProtocolParser.ReadUInt32(stream);
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

	public static void Serialize(Stream stream, UncaughtException instance)
	{
		if (instance.HasStackTrace)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.StackTrace));
		}
		if (instance.HasAndroidModel)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AndroidModel));
		}
		if (instance.HasAndroidSdkVersion)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.AndroidSdkVersion);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasStackTrace)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(StackTrace);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasAndroidModel)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(AndroidModel);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasAndroidSdkVersion)
		{
			size++;
			size += ProtocolParser.SizeOfUInt32(AndroidSdkVersion);
		}
		return size;
	}
}
