using System.Collections.Generic;
using System.IO;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public static class OfflineDataSerializer
{
	private abstract class OfflineDataSerializerBase : IOfflineDataSerializer
	{
		public void Serialize(OfflineDataCache.OfflineData data, BinaryWriter writer)
		{
			if (writer == null)
			{
				Debug.LogError("Could not Serialize OfflineData, writer was null");
				return;
			}
			writer.Write(data.UniqueFakeDeckId);
			List<long> fakeDeckIdList = OfflineDataCache.GetFakeDeckIds(data);
			writer.Write(fakeDeckIdList.Count);
			foreach (long deckId in fakeDeckIdList)
			{
				writer.Write(deckId);
			}
			List<DeckInfo> originalDeckList = ((data.OriginalDeckList == null) ? new List<DeckInfo>() : data.OriginalDeckList);
			List<DeckInfo> localDeckList = ((data.LocalDeckList == null) ? new List<DeckInfo>() : data.LocalDeckList);
			writer.Write(originalDeckList.Count);
			foreach (DeckInfo deckInfo in originalDeckList)
			{
				AppendProtoToFile(writer, deckInfo);
			}
			writer.Write(localDeckList.Count);
			foreach (DeckInfo deckInfo2 in localDeckList)
			{
				AppendProtoToFile(writer, deckInfo2);
			}
			List<DeckContents> originalDeckContentsList = ((data.OriginalDeckContents == null) ? new List<DeckContents>() : data.OriginalDeckContents);
			List<DeckContents> localDeckContentsList = ((data.LocalDeckContents == null) ? new List<DeckContents>() : data.LocalDeckContents);
			writer.Write(originalDeckContentsList.Count);
			foreach (DeckContents deckContents in originalDeckContentsList)
			{
				AppendProtoToFile(writer, deckContents);
			}
			writer.Write(localDeckContentsList.Count);
			foreach (DeckContents deckContents2 in localDeckContentsList)
			{
				AppendProtoToFile(writer, deckContents2);
			}
			List<FavoriteHero> favoriteHeroesList = ((data.FavoriteHeroes == null) ? new List<FavoriteHero>() : data.FavoriteHeroes);
			writer.Write(favoriteHeroesList.Count);
			foreach (FavoriteHero favoriteHero in favoriteHeroesList)
			{
				AppendProtoToFile(writer, favoriteHero);
			}
			writer.Write(data.m_hasChangedFavoriteHeroesOffline);
			AppendProtoToFile(writer, data.CardBacks);
			writer.Write(data.m_hasChangedCardBacksOffline);
			AppendProtoToFile(writer, data.Collection);
			AppendProtoToFile(writer, data.CosmeticCoins);
			writer.Write(data.m_hasChangedCoinsOffline);
		}

		public abstract OfflineDataCache.OfflineData Deserialize(BinaryReader reader);
	}

	private class OfflineDataSerializer_V0Deserializer : OfflineDataSerializerBase
	{
		public override OfflineDataCache.OfflineData Deserialize(BinaryReader reader)
		{
			if (reader == null)
			{
				Debug.LogError("Could not Deserialize v0 OfflineData, reader was null");
				return null;
			}
			OfflineDataCache.OfflineData data = new OfflineDataCache.OfflineData();
			data.UniqueFakeDeckId = reader.ReadInt32();
			int deckIdCount = reader.ReadInt32();
			data.FakeDeckIds = new List<long>();
			for (int i = 0; i < deckIdCount; i++)
			{
				data.FakeDeckIds.Add(reader.ReadInt64());
			}
			int originalDeckListCount = reader.ReadInt32();
			data.OriginalDeckList = new List<DeckInfo>();
			for (int j = 0; j < originalDeckListCount; j++)
			{
				DeckInfo deckInfo = ReadProtoFromFile<DeckInfo>(reader);
				data.OriginalDeckList.Add(deckInfo);
			}
			int localDeckListCount = reader.ReadInt32();
			data.LocalDeckList = new List<DeckInfo>();
			for (int k = 0; k < localDeckListCount; k++)
			{
				DeckInfo deckInfo2 = ReadProtoFromFile<DeckInfo>(reader);
				data.LocalDeckList.Add(deckInfo2);
			}
			int originalDeckContentsCount = reader.ReadInt32();
			data.OriginalDeckContents = new List<DeckContents>();
			for (int l = 0; l < originalDeckContentsCount; l++)
			{
				DeckContents deckContents = ReadProtoFromFile<DeckContents>(reader);
				data.OriginalDeckContents.Add(deckContents);
			}
			int localDeckContentsCount = reader.ReadInt32();
			data.LocalDeckContents = new List<DeckContents>();
			for (int m = 0; m < localDeckContentsCount; m++)
			{
				DeckContents deckContents2 = ReadProtoFromFile<DeckContents>(reader);
				data.LocalDeckContents.Add(deckContents2);
			}
			int favoriteHeroesCount = reader.ReadInt32();
			data.FavoriteHeroes = new List<FavoriteHero>();
			for (int n = 0; n < favoriteHeroesCount; n++)
			{
				FavoriteHero favoriteHero = ReadProtoFromFile<FavoriteHero>(reader);
				data.FavoriteHeroes.Add(favoriteHero);
			}
			data.m_hasChangedFavoriteHeroesOffline = reader.ReadBoolean();
			data.CardBacks = ReadProtoFromFile<CardBacks>(reader);
			data.m_hasChangedCardBacksOffline = reader.ReadBoolean();
			data.CosmeticCoins = new CosmeticCoins();
			return data;
		}
	}

	private class OfflineDataSerializer_V1Deserializer : OfflineDataSerializerBase
	{
		public override OfflineDataCache.OfflineData Deserialize(BinaryReader reader)
		{
			if (reader == null)
			{
				Debug.LogError("Could not Deserialize v10 OfflineData, reader was null");
				return null;
			}
			OfflineDataCache.OfflineData data = new OfflineDataCache.OfflineData();
			data.UniqueFakeDeckId = reader.ReadInt32();
			int deckIdCount = reader.ReadInt32();
			data.FakeDeckIds = new List<long>();
			for (int i = 0; i < deckIdCount; i++)
			{
				data.FakeDeckIds.Add(reader.ReadInt64());
			}
			int originalDeckListCount = reader.ReadInt32();
			data.OriginalDeckList = new List<DeckInfo>();
			for (int j = 0; j < originalDeckListCount; j++)
			{
				DeckInfo deckInfo = ReadProtoFromFile<DeckInfo>(reader);
				data.OriginalDeckList.Add(deckInfo);
			}
			int localDeckListCount = reader.ReadInt32();
			data.LocalDeckList = new List<DeckInfo>();
			for (int k = 0; k < localDeckListCount; k++)
			{
				DeckInfo deckInfo2 = ReadProtoFromFile<DeckInfo>(reader);
				data.LocalDeckList.Add(deckInfo2);
			}
			int originalDeckContentsCount = reader.ReadInt32();
			data.OriginalDeckContents = new List<DeckContents>();
			for (int l = 0; l < originalDeckContentsCount; l++)
			{
				DeckContents deckContents = ReadProtoFromFile<DeckContents>(reader);
				data.OriginalDeckContents.Add(deckContents);
			}
			int localDeckContentsCount = reader.ReadInt32();
			data.LocalDeckContents = new List<DeckContents>();
			for (int m = 0; m < localDeckContentsCount; m++)
			{
				DeckContents deckContents2 = ReadProtoFromFile<DeckContents>(reader);
				data.LocalDeckContents.Add(deckContents2);
			}
			int favoriteHeroesCount = reader.ReadInt32();
			data.FavoriteHeroes = new List<FavoriteHero>();
			for (int n = 0; n < favoriteHeroesCount; n++)
			{
				FavoriteHero favoriteHero = ReadProtoFromFile<FavoriteHero>(reader);
				data.FavoriteHeroes.Add(favoriteHero);
			}
			data.m_hasChangedFavoriteHeroesOffline = reader.ReadBoolean();
			data.CardBacks = ReadProtoFromFile<CardBacks>(reader);
			data.m_hasChangedCardBacksOffline = reader.ReadBoolean();
			data.Collection = ReadProtoFromFile<Collection>(reader);
			data.CosmeticCoins = new CosmeticCoins();
			return data;
		}
	}

	private class OfflineDataSerializer_V2Deserializer : OfflineDataSerializerBase
	{
		public override OfflineDataCache.OfflineData Deserialize(BinaryReader reader)
		{
			if (reader == null)
			{
				Debug.LogError("Could not Deserialize v10 OfflineData, reader was null");
				return null;
			}
			OfflineDataCache.OfflineData data = new OfflineDataCache.OfflineData();
			data.UniqueFakeDeckId = reader.ReadInt32();
			int deckIdCount = reader.ReadInt32();
			data.FakeDeckIds = new List<long>();
			for (int i = 0; i < deckIdCount; i++)
			{
				data.FakeDeckIds.Add(reader.ReadInt64());
			}
			int originalDeckListCount = reader.ReadInt32();
			data.OriginalDeckList = new List<DeckInfo>();
			for (int j = 0; j < originalDeckListCount; j++)
			{
				DeckInfo deckInfo = ReadProtoFromFile<DeckInfo>(reader);
				data.OriginalDeckList.Add(deckInfo);
			}
			int localDeckListCount = reader.ReadInt32();
			data.LocalDeckList = new List<DeckInfo>();
			for (int k = 0; k < localDeckListCount; k++)
			{
				DeckInfo deckInfo2 = ReadProtoFromFile<DeckInfo>(reader);
				data.LocalDeckList.Add(deckInfo2);
			}
			int originalDeckContentsCount = reader.ReadInt32();
			data.OriginalDeckContents = new List<DeckContents>();
			for (int l = 0; l < originalDeckContentsCount; l++)
			{
				DeckContents deckContents = ReadProtoFromFile<DeckContents>(reader);
				data.OriginalDeckContents.Add(deckContents);
			}
			int localDeckContentsCount = reader.ReadInt32();
			data.LocalDeckContents = new List<DeckContents>();
			for (int m = 0; m < localDeckContentsCount; m++)
			{
				DeckContents deckContents2 = ReadProtoFromFile<DeckContents>(reader);
				data.LocalDeckContents.Add(deckContents2);
			}
			int favoriteHeroesCount = reader.ReadInt32();
			data.FavoriteHeroes = new List<FavoriteHero>();
			for (int n = 0; n < favoriteHeroesCount; n++)
			{
				FavoriteHero favoriteHero = ReadProtoFromFile<FavoriteHero>(reader);
				data.FavoriteHeroes.Add(favoriteHero);
			}
			data.m_hasChangedFavoriteHeroesOffline = reader.ReadBoolean();
			data.CardBacks = ReadProtoFromFile<CardBacks>(reader);
			data.m_hasChangedCardBacksOffline = reader.ReadBoolean();
			data.Collection = ReadProtoFromFile<Collection>(reader);
			data.CosmeticCoins = ReadProtoFromFile<CosmeticCoins>(reader);
			data.m_hasChangedCoinsOffline = reader.ReadBoolean();
			return data;
		}
	}

	public static IOfflineDataSerializer GetSerializer(int serializerVersion)
	{
		return serializerVersion switch
		{
			0 => new OfflineDataSerializer_V0Deserializer(), 
			1 => new OfflineDataSerializer_V1Deserializer(), 
			2 => new OfflineDataSerializer_V2Deserializer(), 
			_ => null, 
		};
	}

	private static T ReadProtoFromFile<T>(BinaryReader reader) where T : IProtoBuf, new()
	{
		int protoLength = reader.ReadInt32();
		if (protoLength == 0)
		{
			return default(T);
		}
		return ProtobufUtil.ParseFrom<T>(reader.ReadBytes(protoLength), 0, protoLength);
	}

	private static void AppendProtoToFile(BinaryWriter writer, IProtoBuf packet)
	{
		if (packet == null)
		{
			writer.Write(0);
			return;
		}
		byte[] byteArray = ProtobufUtil.ToByteArray(packet);
		writer.Write(byteArray.Length);
		writer.Write(byteArray);
	}
}
