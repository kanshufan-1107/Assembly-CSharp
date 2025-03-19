using System.IO;

namespace Blizzard.Telemetry.WTCG.Client;

public class AppPaused : IProtoBuf
{
	public bool HasPauseStatus;

	private bool _PauseStatus;

	public bool HasPauseTime;

	private float _PauseTime;

	public bool PauseStatus
	{
		get
		{
			return _PauseStatus;
		}
		set
		{
			_PauseStatus = value;
			HasPauseStatus = true;
		}
	}

	public float PauseTime
	{
		get
		{
			return _PauseTime;
		}
		set
		{
			_PauseTime = value;
			HasPauseTime = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasPauseStatus)
		{
			hash ^= PauseStatus.GetHashCode();
		}
		if (HasPauseTime)
		{
			hash ^= PauseTime.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AppPaused other))
		{
			return false;
		}
		if (HasPauseStatus != other.HasPauseStatus || (HasPauseStatus && !PauseStatus.Equals(other.PauseStatus)))
		{
			return false;
		}
		if (HasPauseTime != other.HasPauseTime || (HasPauseTime && !PauseTime.Equals(other.PauseTime)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static AppPaused Deserialize(Stream stream, AppPaused instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static AppPaused DeserializeLengthDelimited(Stream stream)
	{
		AppPaused instance = new AppPaused();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static AppPaused DeserializeLengthDelimited(Stream stream, AppPaused instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static AppPaused Deserialize(Stream stream, AppPaused instance, long limit)
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
				instance.PauseStatus = ProtocolParser.ReadBool(stream);
				continue;
			case 21:
				instance.PauseTime = br.ReadSingle();
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

	public static void Serialize(Stream stream, AppPaused instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasPauseStatus)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.PauseStatus);
		}
		if (instance.HasPauseTime)
		{
			stream.WriteByte(21);
			bw.Write(instance.PauseTime);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasPauseStatus)
		{
			size++;
			size++;
		}
		if (HasPauseTime)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
