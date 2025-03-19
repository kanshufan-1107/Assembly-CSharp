using System.IO;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class UpdateError : IProtoBuf
{
	public bool HasErrorCode;

	private uint _ErrorCode;

	public bool HasElapsedSeconds;

	private float _ElapsedSeconds;

	public uint ErrorCode
	{
		get
		{
			return _ErrorCode;
		}
		set
		{
			_ErrorCode = value;
			HasErrorCode = true;
		}
	}

	public float ElapsedSeconds
	{
		get
		{
			return _ElapsedSeconds;
		}
		set
		{
			_ElapsedSeconds = value;
			HasElapsedSeconds = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasErrorCode)
		{
			hash ^= ErrorCode.GetHashCode();
		}
		if (HasElapsedSeconds)
		{
			hash ^= ElapsedSeconds.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is UpdateError other))
		{
			return false;
		}
		if (HasErrorCode != other.HasErrorCode || (HasErrorCode && !ErrorCode.Equals(other.ErrorCode)))
		{
			return false;
		}
		if (HasElapsedSeconds != other.HasElapsedSeconds || (HasElapsedSeconds && !ElapsedSeconds.Equals(other.ElapsedSeconds)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static UpdateError Deserialize(Stream stream, UpdateError instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static UpdateError DeserializeLengthDelimited(Stream stream)
	{
		UpdateError instance = new UpdateError();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static UpdateError DeserializeLengthDelimited(Stream stream, UpdateError instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static UpdateError Deserialize(Stream stream, UpdateError instance, long limit)
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
			case 8:
				instance.ErrorCode = ProtocolParser.ReadUInt32(stream);
				continue;
			case 37:
				instance.ElapsedSeconds = br.ReadSingle();
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

	public static void Serialize(Stream stream, UpdateError instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasErrorCode)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.ErrorCode);
		}
		if (instance.HasElapsedSeconds)
		{
			stream.WriteByte(37);
			bw.Write(instance.ElapsedSeconds);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasErrorCode)
		{
			size++;
			size += ProtocolParser.SizeOfUInt32(ErrorCode);
		}
		if (HasElapsedSeconds)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
