using System.IO;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class ApkUpdate : IProtoBuf
{
	public bool HasInstalledVersion;

	private int _InstalledVersion;

	public bool HasAssetVersion;

	private int _AssetVersion;

	public bool HasAgentVersion;

	private int _AgentVersion;

	public int InstalledVersion
	{
		get
		{
			return _InstalledVersion;
		}
		set
		{
			_InstalledVersion = value;
			HasInstalledVersion = true;
		}
	}

	public int AssetVersion
	{
		get
		{
			return _AssetVersion;
		}
		set
		{
			_AssetVersion = value;
			HasAssetVersion = true;
		}
	}

	public int AgentVersion
	{
		get
		{
			return _AgentVersion;
		}
		set
		{
			_AgentVersion = value;
			HasAgentVersion = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasInstalledVersion)
		{
			hash ^= InstalledVersion.GetHashCode();
		}
		if (HasAssetVersion)
		{
			hash ^= AssetVersion.GetHashCode();
		}
		if (HasAgentVersion)
		{
			hash ^= AgentVersion.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ApkUpdate other))
		{
			return false;
		}
		if (HasInstalledVersion != other.HasInstalledVersion || (HasInstalledVersion && !InstalledVersion.Equals(other.InstalledVersion)))
		{
			return false;
		}
		if (HasAssetVersion != other.HasAssetVersion || (HasAssetVersion && !AssetVersion.Equals(other.AssetVersion)))
		{
			return false;
		}
		if (HasAgentVersion != other.HasAgentVersion || (HasAgentVersion && !AgentVersion.Equals(other.AgentVersion)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ApkUpdate Deserialize(Stream stream, ApkUpdate instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ApkUpdate DeserializeLengthDelimited(Stream stream)
	{
		ApkUpdate instance = new ApkUpdate();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ApkUpdate DeserializeLengthDelimited(Stream stream, ApkUpdate instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ApkUpdate Deserialize(Stream stream, ApkUpdate instance, long limit)
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
			case 8:
				instance.InstalledVersion = ProtocolParser.ReadZInt32(stream);
				continue;
			case 16:
				instance.AssetVersion = ProtocolParser.ReadZInt32(stream);
				continue;
			case 24:
				instance.AgentVersion = ProtocolParser.ReadZInt32(stream);
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

	public static void Serialize(Stream stream, ApkUpdate instance)
	{
		if (instance.HasInstalledVersion)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteZInt32(stream, instance.InstalledVersion);
		}
		if (instance.HasAssetVersion)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteZInt32(stream, instance.AssetVersion);
		}
		if (instance.HasAgentVersion)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteZInt32(stream, instance.AgentVersion);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasInstalledVersion)
		{
			size++;
			size += ProtocolParser.SizeOfZInt32(InstalledVersion);
		}
		if (HasAssetVersion)
		{
			size++;
			size += ProtocolParser.SizeOfZInt32(AssetVersion);
		}
		if (HasAgentVersion)
		{
			size++;
			size += ProtocolParser.SizeOfZInt32(AgentVersion);
		}
		return size;
	}
}
