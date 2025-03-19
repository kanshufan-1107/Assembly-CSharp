using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class VersionFinished : IProtoBuf
{
	public bool HasCurrentVersion;

	private string _CurrentVersion;

	public bool HasLiveVersion;

	private string _LiveVersion;

	public string CurrentVersion
	{
		get
		{
			return _CurrentVersion;
		}
		set
		{
			_CurrentVersion = value;
			HasCurrentVersion = value != null;
		}
	}

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

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasCurrentVersion)
		{
			hash ^= CurrentVersion.GetHashCode();
		}
		if (HasLiveVersion)
		{
			hash ^= LiveVersion.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is VersionFinished other))
		{
			return false;
		}
		if (HasCurrentVersion != other.HasCurrentVersion || (HasCurrentVersion && !CurrentVersion.Equals(other.CurrentVersion)))
		{
			return false;
		}
		if (HasLiveVersion != other.HasLiveVersion || (HasLiveVersion && !LiveVersion.Equals(other.LiveVersion)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static VersionFinished Deserialize(Stream stream, VersionFinished instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static VersionFinished DeserializeLengthDelimited(Stream stream)
	{
		VersionFinished instance = new VersionFinished();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static VersionFinished DeserializeLengthDelimited(Stream stream, VersionFinished instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static VersionFinished Deserialize(Stream stream, VersionFinished instance, long limit)
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
				instance.CurrentVersion = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				instance.LiveVersion = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, VersionFinished instance)
	{
		if (instance.HasCurrentVersion)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.CurrentVersion));
		}
		if (instance.HasLiveVersion)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.LiveVersion));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasCurrentVersion)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(CurrentVersion);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasLiveVersion)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(LiveVersion);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		return size;
	}
}
