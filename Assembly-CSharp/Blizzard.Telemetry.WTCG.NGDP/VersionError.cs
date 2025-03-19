using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class VersionError : IProtoBuf
{
	public bool HasErrorCode;

	private uint _ErrorCode;

	public bool HasAgentState;

	private uint _AgentState;

	public bool HasLanguages;

	private string _Languages;

	public bool HasRegion;

	private string _Region;

	public bool HasBranch;

	private string _Branch;

	public bool HasAdditionalTags;

	private string _AdditionalTags;

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

	public uint AgentState
	{
		get
		{
			return _AgentState;
		}
		set
		{
			_AgentState = value;
			HasAgentState = true;
		}
	}

	public string Languages
	{
		get
		{
			return _Languages;
		}
		set
		{
			_Languages = value;
			HasLanguages = value != null;
		}
	}

	public string Region
	{
		get
		{
			return _Region;
		}
		set
		{
			_Region = value;
			HasRegion = value != null;
		}
	}

	public string Branch
	{
		get
		{
			return _Branch;
		}
		set
		{
			_Branch = value;
			HasBranch = value != null;
		}
	}

	public string AdditionalTags
	{
		get
		{
			return _AdditionalTags;
		}
		set
		{
			_AdditionalTags = value;
			HasAdditionalTags = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasErrorCode)
		{
			hash ^= ErrorCode.GetHashCode();
		}
		if (HasAgentState)
		{
			hash ^= AgentState.GetHashCode();
		}
		if (HasLanguages)
		{
			hash ^= Languages.GetHashCode();
		}
		if (HasRegion)
		{
			hash ^= Region.GetHashCode();
		}
		if (HasBranch)
		{
			hash ^= Branch.GetHashCode();
		}
		if (HasAdditionalTags)
		{
			hash ^= AdditionalTags.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is VersionError other))
		{
			return false;
		}
		if (HasErrorCode != other.HasErrorCode || (HasErrorCode && !ErrorCode.Equals(other.ErrorCode)))
		{
			return false;
		}
		if (HasAgentState != other.HasAgentState || (HasAgentState && !AgentState.Equals(other.AgentState)))
		{
			return false;
		}
		if (HasLanguages != other.HasLanguages || (HasLanguages && !Languages.Equals(other.Languages)))
		{
			return false;
		}
		if (HasRegion != other.HasRegion || (HasRegion && !Region.Equals(other.Region)))
		{
			return false;
		}
		if (HasBranch != other.HasBranch || (HasBranch && !Branch.Equals(other.Branch)))
		{
			return false;
		}
		if (HasAdditionalTags != other.HasAdditionalTags || (HasAdditionalTags && !AdditionalTags.Equals(other.AdditionalTags)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static VersionError Deserialize(Stream stream, VersionError instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static VersionError DeserializeLengthDelimited(Stream stream)
	{
		VersionError instance = new VersionError();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static VersionError DeserializeLengthDelimited(Stream stream, VersionError instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static VersionError Deserialize(Stream stream, VersionError instance, long limit)
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
				instance.ErrorCode = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				instance.AgentState = ProtocolParser.ReadUInt32(stream);
				continue;
			case 42:
				instance.Languages = ProtocolParser.ReadString(stream);
				continue;
			case 50:
				instance.Region = ProtocolParser.ReadString(stream);
				continue;
			case 58:
				instance.Branch = ProtocolParser.ReadString(stream);
				continue;
			case 66:
				instance.AdditionalTags = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, VersionError instance)
	{
		if (instance.HasErrorCode)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.ErrorCode);
		}
		if (instance.HasAgentState)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt32(stream, instance.AgentState);
		}
		if (instance.HasLanguages)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Languages));
		}
		if (instance.HasRegion)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Region));
		}
		if (instance.HasBranch)
		{
			stream.WriteByte(58);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Branch));
		}
		if (instance.HasAdditionalTags)
		{
			stream.WriteByte(66);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.AdditionalTags));
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
		if (HasAgentState)
		{
			size++;
			size += ProtocolParser.SizeOfUInt32(AgentState);
		}
		if (HasLanguages)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(Languages);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		if (HasRegion)
		{
			size++;
			uint byteCount6 = (uint)Encoding.UTF8.GetByteCount(Region);
			size += ProtocolParser.SizeOfUInt32(byteCount6) + byteCount6;
		}
		if (HasBranch)
		{
			size++;
			uint byteCount7 = (uint)Encoding.UTF8.GetByteCount(Branch);
			size += ProtocolParser.SizeOfUInt32(byteCount7) + byteCount7;
		}
		if (HasAdditionalTags)
		{
			size++;
			uint byteCount8 = (uint)Encoding.UTF8.GetByteCount(AdditionalTags);
			size += ProtocolParser.SizeOfUInt32(byteCount8) + byteCount8;
		}
		return size;
	}
}
