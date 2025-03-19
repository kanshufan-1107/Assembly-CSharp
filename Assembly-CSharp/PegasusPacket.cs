using System;
using System.IO;
using Blizzard.GameService.SDK.Client.Integration;

public class PegasusPacket : PacketFormat
{
	private const int TYPE_BYTES = 4;

	private const int SIZE_BYTES = 4;

	public int Size;

	public int Type;

	public int Context;

	public object Body;

	private bool sizeRead;

	private bool typeRead;

	public PegasusPacket()
	{
	}

	public PegasusPacket(int type, int context, object body)
	{
		Type = type;
		Context = context;
		Size = -1;
		Body = body;
	}

	public PegasusPacket(int type, int context, int size, object body)
	{
		Type = type;
		Context = context;
		Size = size;
		Body = body;
	}

	public override bool IsLoaded()
	{
		return Body != null;
	}

	public override int Decode(byte[] bytes, int offset, int available)
	{
		int bytesRead = 0;
		if (!typeRead)
		{
			if (available < 4)
			{
				return bytesRead;
			}
			Type = BitConverter.ToInt32(bytes, offset);
			typeRead = true;
			available -= 4;
			bytesRead += 4;
			offset += 4;
		}
		if (!sizeRead)
		{
			if (available < 4)
			{
				return bytesRead;
			}
			Size = BitConverter.ToInt32(bytes, offset);
			sizeRead = true;
			available -= 4;
			bytesRead += 4;
			offset += 4;
		}
		if (Body == null)
		{
			if (available < Size)
			{
				return bytesRead;
			}
			byte[] bodyBytes = new byte[Size];
			Array.Copy(bytes, offset, bodyBytes, 0, Size);
			Body = bodyBytes;
			bytesRead += Size;
		}
		return bytesRead;
	}

	public override byte[] Encode()
	{
		if (Body is IProtoBuf)
		{
			IProtoBuf protoBuf = (IProtoBuf)Body;
			Size = (int)protoBuf.GetSerializedSize();
			byte[] result = new byte[Size + 4 + 4];
			Array.Copy(BitConverter.GetBytes(Type), 0, result, 0, 4);
			Array.Copy(BitConverter.GetBytes(Size), 0, result, 4, 4);
			protoBuf.Serialize(new MemoryStream(result, 8, Size));
			return result;
		}
		return null;
	}

	public override string ToString()
	{
		return "PegasusPacket Type: " + Type;
	}

	public override bool IsFatalOnError()
	{
		return Type == 168;
	}
}
