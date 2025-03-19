using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class UpdateFinished : IProtoBuf
{
	public bool HasUpdatedVersion;

	private string _UpdatedVersion;

	public bool HasAvailableSpaceMB;

	private float _AvailableSpaceMB;

	public bool HasElapsedSeconds;

	private float _ElapsedSeconds;

	public string UpdatedVersion
	{
		get
		{
			return _UpdatedVersion;
		}
		set
		{
			_UpdatedVersion = value;
			HasUpdatedVersion = value != null;
		}
	}

	public float AvailableSpaceMB
	{
		get
		{
			return _AvailableSpaceMB;
		}
		set
		{
			_AvailableSpaceMB = value;
			HasAvailableSpaceMB = true;
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
		if (HasUpdatedVersion)
		{
			hash ^= UpdatedVersion.GetHashCode();
		}
		if (HasAvailableSpaceMB)
		{
			hash ^= AvailableSpaceMB.GetHashCode();
		}
		if (HasElapsedSeconds)
		{
			hash ^= ElapsedSeconds.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is UpdateFinished other))
		{
			return false;
		}
		if (HasUpdatedVersion != other.HasUpdatedVersion || (HasUpdatedVersion && !UpdatedVersion.Equals(other.UpdatedVersion)))
		{
			return false;
		}
		if (HasAvailableSpaceMB != other.HasAvailableSpaceMB || (HasAvailableSpaceMB && !AvailableSpaceMB.Equals(other.AvailableSpaceMB)))
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

	public static UpdateFinished Deserialize(Stream stream, UpdateFinished instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static UpdateFinished DeserializeLengthDelimited(Stream stream)
	{
		UpdateFinished instance = new UpdateFinished();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static UpdateFinished DeserializeLengthDelimited(Stream stream, UpdateFinished instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static UpdateFinished Deserialize(Stream stream, UpdateFinished instance, long limit)
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
			case 18:
				instance.UpdatedVersion = ProtocolParser.ReadString(stream);
				continue;
			case 29:
				instance.AvailableSpaceMB = br.ReadSingle();
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

	public static void Serialize(Stream stream, UpdateFinished instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasUpdatedVersion)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.UpdatedVersion));
		}
		if (instance.HasAvailableSpaceMB)
		{
			stream.WriteByte(29);
			bw.Write(instance.AvailableSpaceMB);
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
		if (HasUpdatedVersion)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(UpdatedVersion);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasAvailableSpaceMB)
		{
			size++;
			size += 4;
		}
		if (HasElapsedSeconds)
		{
			size++;
			size += 4;
		}
		return size;
	}
}
