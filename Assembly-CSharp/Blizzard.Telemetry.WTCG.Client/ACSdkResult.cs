using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ACSdkResult : IProtoBuf
{
	public enum CommandType
	{
		SetupSDK,
		CallSDK
	}

	public bool HasReportId;

	private string _ReportId;

	public bool HasCommandType_;

	private CommandType _CommandType_;

	public bool HasScriptId;

	private string _ScriptId;

	public bool HasReturnSetExtraParams;

	private bool _ReturnSetExtraParams;

	public bool HasReturnSetupSDK;

	private int _ReturnSetupSDK;

	public bool HasReturnCallSDK;

	private int _ReturnCallSDK;

	private List<string> _Messages = new List<string>();

	public string ReportId
	{
		get
		{
			return _ReportId;
		}
		set
		{
			_ReportId = value;
			HasReportId = value != null;
		}
	}

	public CommandType CommandType_
	{
		get
		{
			return _CommandType_;
		}
		set
		{
			_CommandType_ = value;
			HasCommandType_ = true;
		}
	}

	public string ScriptId
	{
		get
		{
			return _ScriptId;
		}
		set
		{
			_ScriptId = value;
			HasScriptId = value != null;
		}
	}

	public bool ReturnSetExtraParams
	{
		get
		{
			return _ReturnSetExtraParams;
		}
		set
		{
			_ReturnSetExtraParams = value;
			HasReturnSetExtraParams = true;
		}
	}

	public int ReturnSetupSDK
	{
		get
		{
			return _ReturnSetupSDK;
		}
		set
		{
			_ReturnSetupSDK = value;
			HasReturnSetupSDK = true;
		}
	}

	public int ReturnCallSDK
	{
		get
		{
			return _ReturnCallSDK;
		}
		set
		{
			_ReturnCallSDK = value;
			HasReturnCallSDK = true;
		}
	}

	public List<string> Messages
	{
		get
		{
			return _Messages;
		}
		set
		{
			_Messages = value;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasReportId)
		{
			hash ^= ReportId.GetHashCode();
		}
		if (HasCommandType_)
		{
			hash ^= CommandType_.GetHashCode();
		}
		if (HasScriptId)
		{
			hash ^= ScriptId.GetHashCode();
		}
		if (HasReturnSetExtraParams)
		{
			hash ^= ReturnSetExtraParams.GetHashCode();
		}
		if (HasReturnSetupSDK)
		{
			hash ^= ReturnSetupSDK.GetHashCode();
		}
		if (HasReturnCallSDK)
		{
			hash ^= ReturnCallSDK.GetHashCode();
		}
		foreach (string i in Messages)
		{
			hash ^= i.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ACSdkResult other))
		{
			return false;
		}
		if (HasReportId != other.HasReportId || (HasReportId && !ReportId.Equals(other.ReportId)))
		{
			return false;
		}
		if (HasCommandType_ != other.HasCommandType_ || (HasCommandType_ && !CommandType_.Equals(other.CommandType_)))
		{
			return false;
		}
		if (HasScriptId != other.HasScriptId || (HasScriptId && !ScriptId.Equals(other.ScriptId)))
		{
			return false;
		}
		if (HasReturnSetExtraParams != other.HasReturnSetExtraParams || (HasReturnSetExtraParams && !ReturnSetExtraParams.Equals(other.ReturnSetExtraParams)))
		{
			return false;
		}
		if (HasReturnSetupSDK != other.HasReturnSetupSDK || (HasReturnSetupSDK && !ReturnSetupSDK.Equals(other.ReturnSetupSDK)))
		{
			return false;
		}
		if (HasReturnCallSDK != other.HasReturnCallSDK || (HasReturnCallSDK && !ReturnCallSDK.Equals(other.ReturnCallSDK)))
		{
			return false;
		}
		if (Messages.Count != other.Messages.Count)
		{
			return false;
		}
		for (int i = 0; i < Messages.Count; i++)
		{
			if (!Messages[i].Equals(other.Messages[i]))
			{
				return false;
			}
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ACSdkResult Deserialize(Stream stream, ACSdkResult instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ACSdkResult DeserializeLengthDelimited(Stream stream)
	{
		ACSdkResult instance = new ACSdkResult();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ACSdkResult DeserializeLengthDelimited(Stream stream, ACSdkResult instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ACSdkResult Deserialize(Stream stream, ACSdkResult instance, long limit)
	{
		instance.CommandType_ = CommandType.SetupSDK;
		if (instance.Messages == null)
		{
			instance.Messages = new List<string>();
		}
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
				instance.ReportId = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.CommandType_ = (CommandType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				instance.ScriptId = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				instance.ReturnSetExtraParams = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				instance.ReturnSetupSDK = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				instance.ReturnCallSDK = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 90:
				instance.Messages.Add(ProtocolParser.ReadString(stream));
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

	public static void Serialize(Stream stream, ACSdkResult instance)
	{
		if (instance.HasReportId)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ReportId));
		}
		if (instance.HasCommandType_)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CommandType_);
		}
		if (instance.HasScriptId)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ScriptId));
		}
		if (instance.HasReturnSetExtraParams)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.ReturnSetExtraParams);
		}
		if (instance.HasReturnSetupSDK)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ReturnSetupSDK);
		}
		if (instance.HasReturnCallSDK)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ReturnCallSDK);
		}
		if (instance.Messages.Count <= 0)
		{
			return;
		}
		foreach (string i11 in instance.Messages)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(i11));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasReportId)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(ReportId);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasCommandType_)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CommandType_);
		}
		if (HasScriptId)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(ScriptId);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasReturnSetExtraParams)
		{
			size++;
			size++;
		}
		if (HasReturnSetupSDK)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ReturnSetupSDK);
		}
		if (HasReturnCallSDK)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ReturnCallSDK);
		}
		if (Messages.Count > 0)
		{
			foreach (string i11 in Messages)
			{
				size++;
				uint byteCount11 = (uint)Encoding.UTF8.GetByteCount(i11);
				size += ProtocolParser.SizeOfUInt32(byteCount11) + byteCount11;
			}
		}
		return size;
	}
}
