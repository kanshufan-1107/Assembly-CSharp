using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class ApkInstallFailure : IProtoBuf
{
	public bool HasUpdatedVersion;

	private string _UpdatedVersion;

	public bool HasReason;

	private string _Reason;

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

	public string Reason
	{
		get
		{
			return _Reason;
		}
		set
		{
			_Reason = value;
			HasReason = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasUpdatedVersion)
		{
			hash ^= UpdatedVersion.GetHashCode();
		}
		if (HasReason)
		{
			hash ^= Reason.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ApkInstallFailure other))
		{
			return false;
		}
		if (HasUpdatedVersion != other.HasUpdatedVersion || (HasUpdatedVersion && !UpdatedVersion.Equals(other.UpdatedVersion)))
		{
			return false;
		}
		if (HasReason != other.HasReason || (HasReason && !Reason.Equals(other.Reason)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ApkInstallFailure Deserialize(Stream stream, ApkInstallFailure instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ApkInstallFailure DeserializeLengthDelimited(Stream stream)
	{
		ApkInstallFailure instance = new ApkInstallFailure();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ApkInstallFailure DeserializeLengthDelimited(Stream stream, ApkInstallFailure instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ApkInstallFailure Deserialize(Stream stream, ApkInstallFailure instance, long limit)
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
			case 18:
				instance.UpdatedVersion = ProtocolParser.ReadString(stream);
				continue;
			case 42:
				instance.Reason = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, ApkInstallFailure instance)
	{
		if (instance.HasUpdatedVersion)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.UpdatedVersion));
		}
		if (instance.HasReason)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Reason));
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
		if (HasReason)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(Reason);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		return size;
	}
}
