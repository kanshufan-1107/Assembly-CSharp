using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone;
using Hearthstone.Core;
using PegasusUtil;
using UnityEngine;

public class HttpCheater
{
	private bool m_isReady;

	private string m_address;

	private int m_port;

	private static HttpCheater s_instance;

	private string m_baseUrl => $"http://{m_address}:{m_port}";

	public static HttpCheater Get()
	{
		if (s_instance == null)
		{
			s_instance = new HttpCheater();
			Network.Get().RegisterNetHandler(LocateCheatServerResponse.PacketID.ID, s_instance.OnLocateCheatServerResponse);
		}
		return s_instance;
	}

	public void OnLocateCheatServerResponse()
	{
		LocateCheatServerResponse response = Network.Get().GetLocateCheatServerResponse();
		Initialize(response.Address, response.Port);
	}

	public void Initialize(string address, int port)
	{
		m_address = address;
		m_port = port;
		m_isReady = true;
	}

	private IEnumerator LocateServerCoroutine(int timeoutMilliseconds)
	{
		if (!m_isReady && !HearthstoneApplication.IsPublic())
		{
			Network.Get().SendLocateCheatServerRequest();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!m_isReady && stopwatch.ElapsedMilliseconds <= timeoutMilliseconds)
			{
				yield return null;
			}
		}
	}

	private Coroutine LocateServer(int timeoutMilliseconds = 5000)
	{
		return Processor.RunCoroutine(LocateServerCoroutine(timeoutMilliseconds));
	}

	public void RunSetResourceCommand(string[] args)
	{
		if (!CheatResourceParser.TryParse(args, out var resource, out var errMsg))
		{
			UIStatus.Get().AddError(errMsg);
		}
		else if (resource is TutorialCheatResource tutorialResource)
		{
			UpdateTutorial(tutorialResource.Progress);
		}
		else if (resource is HeroCheatResource heroResource)
		{
			UpdateHero(heroResource.ClassName, heroResource.Level, heroResource.Wins, heroResource.Gametype);
		}
		else if (resource is ArenaCheatResource arenaResource)
		{
			UpdateArenaRecord(arenaResource.Win, arenaResource.Loss);
		}
	}

	public void RunSkipResourceCommand(string[] args)
	{
		if (!CheatResourceParser.TryParse(args, out var resource, out var errMsg))
		{
			UIStatus.Get().AddError(errMsg);
		}
		else if (resource is TutorialCheatResource)
		{
			UpdateTutorial(null);
		}
	}

	public void RunUnlockResourceCommand(string[] args)
	{
		if (!CheatResourceParser.TryParse(args, out var resource, out var errMsg))
		{
			UIStatus.Get().AddError(errMsg);
		}
		else if (resource is HeroCheatResource heroResource)
		{
			UnlockHero(heroResource.ClassName, heroResource.Premium);
		}
	}

	public void RunAddResourceCommand(string[] args)
	{
		if (!CheatResourceParser.TryParse(args, out var resource, out var errMsg))
		{
			UIStatus.Get().AddError(errMsg);
		}
		else if (resource is GoldCheatResource goldResource)
		{
			UpdateGold(goldResource.Amount);
		}
		else if (resource is DustCheatResource dustResource)
		{
			UpdateDust(dustResource.Amount);
		}
		else if (resource is FullCardCollectionCheatResource)
		{
			GrantCardCollection();
		}
		else if (resource is ArenaTicketCheatResource arenaTicketResource)
		{
			GrantArenaTicket(arenaTicketResource.TicketCount);
		}
		else if (resource is PackCheatResource packResource)
		{
			GrantBoosterPack(packResource.PackCount, packResource.TypeID);
		}
	}

	public void RunRemoveResourceCommand(string[] args)
	{
		if (!CheatResourceParser.TryParse(args, out var resource, out var errMsg))
		{
			UIStatus.Get().AddError(errMsg);
		}
		else if (resource is GoldCheatResource goldResource)
		{
			if (goldResource.Amount.HasValue)
			{
				UpdateGold(-goldResource.Amount);
			}
			else
			{
				RemoveAllGold();
			}
		}
		else if (resource is DustCheatResource dustResource)
		{
			if (dustResource.Amount.HasValue)
			{
				UpdateDust(-dustResource.Amount);
			}
			else
			{
				RemoveAllDust();
			}
		}
		else if (resource is HeroCheatResource heroResource)
		{
			RemoveHero(heroResource.ClassName);
		}
		else if (resource is FullCardCollectionCheatResource)
		{
			RemoveCardCollection();
		}
		else if (resource is ArenaTicketCheatResource arenaResource)
		{
			RemoveArenaTicket(arenaResource.TicketCount);
		}
		else if (resource is PackCheatResource packResource)
		{
			RemoveBoosterPack(packResource.PackCount, packResource.TypeID);
		}
		else if (resource is AllAdventureOwnershipCheatResource)
		{
			Processor.RunCoroutine(RemoveResourceCoroutine("adventureownership"));
		}
	}

	public Coroutine GrantCardCollection()
	{
		return Processor.RunCoroutine(GrantCardCollectionCoroutine());
	}

	public Coroutine RemoveCardCollection()
	{
		return Processor.RunCoroutine(RemoveCardCollectionCoroutine());
	}

	public Coroutine UpdateGold(int? deltaAmount)
	{
		return Processor.RunCoroutine(UpdateGoldCoroutine(deltaAmount));
	}

	public Coroutine RemoveAllGold()
	{
		return Processor.RunCoroutine(RemoveAllGoldCoroutine());
	}

	public Coroutine UpdateDust(int? deltaAmount)
	{
		return Processor.RunCoroutine(UpdateDustCoroutine(deltaAmount));
	}

	public Coroutine RemoveAllDust()
	{
		return Processor.RunCoroutine(RemoveAllDustCoroutine());
	}

	public Coroutine UpdateTutorial(int? progressValue)
	{
		return Processor.RunCoroutine(UpdateTutorialCoroutine(progressValue));
	}

	public Coroutine UpdateHero(string className, int? heroLevel, int? wins, string gameType)
	{
		return Processor.RunCoroutine(UpdateHeroCoroutine(className, heroLevel, wins, gameType));
	}

	public Coroutine UnlockHero(string className, TAG_PREMIUM? premium)
	{
		return Processor.RunCoroutine(UnlockHeroCoroutine(className, premium));
	}

	public Coroutine RemoveHero(string className)
	{
		return Processor.RunCoroutine(RemoveHeroCoroutine(className));
	}

	public Coroutine GrantArenaTicket(int? ticketCount)
	{
		return Processor.RunCoroutine(GrantArenaTicketCoroutine(ticketCount));
	}

	public Coroutine RemoveArenaTicket(int? ticketCount)
	{
		return Processor.RunCoroutine(RemoveArenaTicketCoroutine(ticketCount));
	}

	public Coroutine UpdateArenaRecord(int? wins, int? losses)
	{
		return Processor.RunCoroutine(UpdateArenaRecordCoroutine(wins, losses));
	}

	public Coroutine GrantBoosterPack(int? packCount, int? typeID)
	{
		return Processor.RunCoroutine(GrantBoosterPackCoroutine(packCount, typeID));
	}

	public Coroutine RemoveBoosterPack(int? packCount, int? typeID)
	{
		return Processor.RunCoroutine(RemoveBoosterPackCoroutine(packCount, typeID));
	}

	private IEnumerator GrantCardCollectionCoroutine()
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		string url = $"{m_baseUrl}/cheat/cards?accountId={gameAccountId}";
		CheatRequest request = new CheatRequest();
		yield return request.SendGetRequest(url);
	}

	private IEnumerator RemoveCardCollectionCoroutine()
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		string url = $"{m_baseUrl}/cheat/cards?accountId={gameAccountId}";
		CheatRequest request = new CheatRequest();
		yield return request.SendDeleteRequest(url);
	}

	private IEnumerator UpdateGoldCoroutine(int? deltaAmount)
	{
		if (deltaAmount == 0)
		{
			yield break;
		}
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/gold?accountId={1}", m_baseUrl, gameAccountId);
		if (deltaAmount.HasValue)
		{
			urlBuilder.AppendFormat("&amount={0}", deltaAmount);
		}
		CheatRequest request = new CheatRequest();
		yield return request.SendGetRequest(urlBuilder.ToString());
	}

	public IEnumerator RemoveAllGoldCoroutine()
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		string url = $"{m_baseUrl}/cheat/gold?accountId={gameAccountId}";
		CheatRequest request = new CheatRequest();
		yield return request.SendDeleteRequest(url);
	}

	private IEnumerator UpdateDustCoroutine(int? deltaAmount)
	{
		if (deltaAmount == 0)
		{
			yield break;
		}
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/dust?accountId={1}", m_baseUrl, gameAccountId);
		if (deltaAmount.HasValue)
		{
			urlBuilder.AppendFormat("&amount={0}", deltaAmount);
		}
		CheatRequest request = new CheatRequest();
		yield return request.SendGetRequest(urlBuilder.ToString());
	}

	public IEnumerator RemoveAllDustCoroutine()
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		string url = $"{m_baseUrl}/cheat/dust?accountId={gameAccountId}";
		CheatRequest request = new CheatRequest();
		yield return request.SendDeleteRequest(url);
	}

	private IEnumerator UpdateTutorialCoroutine(int? progress)
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/tutorial?accountId={1}", m_baseUrl, gameAccountId);
		if (progress.HasValue)
		{
			urlBuilder.AppendFormat("&progress={0}", progress);
		}
		CheatRequest request = new CheatRequest();
		yield return request.SendGetRequest(urlBuilder.ToString());
		if (request.IsSuccessful)
		{
			HearthstoneApplication.Get().Reset();
		}
	}

	private IEnumerator UpdateHeroCoroutine(string className, int? heroLevel, int? wins, string gameType)
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/hero?accountId={1}", m_baseUrl, gameAccountId);
		if (!string.IsNullOrEmpty(className))
		{
			urlBuilder.AppendFormat("&class={0}", className);
		}
		if (heroLevel.HasValue)
		{
			urlBuilder.AppendFormat("&level={0}", heroLevel);
		}
		if (wins.HasValue)
		{
			urlBuilder.AppendFormat("&wins={0}", wins);
		}
		if (!string.IsNullOrEmpty(gameType))
		{
			urlBuilder.AppendFormat("&gametype={0}", gameType);
		}
		CheatRequest request = new CheatRequest();
		yield return request.SendGetRequest(urlBuilder.ToString());
	}

	private IEnumerator UnlockHeroCoroutine(string className, TAG_PREMIUM? premium)
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/hero?accountId={1}", m_baseUrl, gameAccountId);
		if (!string.IsNullOrEmpty(className))
		{
			urlBuilder.AppendFormat("&class={0}", className);
		}
		if (premium == TAG_PREMIUM.GOLDEN)
		{
			urlBuilder.AppendFormat("&wins=500");
		}
		int max_level = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().MaxHeroLevel;
		urlBuilder.AppendFormat("&level={0}", max_level);
		CheatRequest request = new CheatRequest();
		yield return request.SendGetRequest(urlBuilder.ToString());
	}

	private IEnumerator RemoveHeroCoroutine(string className)
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/hero?accountId={1}", m_baseUrl, gameAccountId);
		if (!string.IsNullOrEmpty(className))
		{
			urlBuilder.AppendFormat("&class={0}", className);
		}
		CheatRequest request = new CheatRequest();
		yield return request.SendDeleteRequest(urlBuilder.ToString());
	}

	private IEnumerator GrantArenaTicketCoroutine(int? ticketCount)
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/arenaticket?accountId={1}", m_baseUrl, gameAccountId);
		if (ticketCount.HasValue)
		{
			urlBuilder.AppendFormat("&ticketCount={0}", ticketCount);
		}
		CheatRequest request = new CheatRequest();
		yield return request.SendGetRequest(urlBuilder.ToString());
	}

	private IEnumerator RemoveArenaTicketCoroutine(int? ticketCount)
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/arenaticket?accountId={1}", m_baseUrl, gameAccountId);
		if (ticketCount.HasValue)
		{
			urlBuilder.AppendFormat("&ticketCount={0}", ticketCount);
		}
		CheatRequest request = new CheatRequest();
		yield return request.SendDeleteRequest(urlBuilder.ToString());
	}

	private IEnumerator UpdateArenaRecordCoroutine(int? wins, int? losses)
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/arena?accountId={1}", m_baseUrl, gameAccountId);
		if (wins.HasValue)
		{
			urlBuilder.AppendFormat("&win={0}", wins);
		}
		if (losses.HasValue)
		{
			urlBuilder.AppendFormat("&loss={0}", losses);
		}
		CheatRequest request = new CheatRequest();
		yield return request.SendGetRequest(urlBuilder.ToString());
		if (request.IsSuccessful && (bool)Object.FindObjectOfType<ArenaTrayDisplay>())
		{
			yield return new WaitForSeconds(1f);
			ArenaTrayDisplay.Get().UpdateTray();
		}
	}

	private IEnumerator GrantBoosterPackCoroutine(int? packCount, int? typeID)
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/pack?accountId={1}", m_baseUrl, gameAccountId);
		if (packCount.HasValue)
		{
			urlBuilder.AppendFormat("&count={0}", packCount);
		}
		if (typeID.HasValue)
		{
			urlBuilder.AppendFormat("&typeID={0}", typeID);
		}
		CheatRequest request = new CheatRequest();
		yield return request.SendGetRequest(urlBuilder.ToString());
	}

	private IEnumerator RemoveBoosterPackCoroutine(int? packCount, int? typeID)
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/pack?accountId={1}", m_baseUrl, gameAccountId);
		if (packCount.HasValue)
		{
			urlBuilder.AppendFormat("&count={0}", packCount);
		}
		if (typeID.HasValue)
		{
			urlBuilder.AppendFormat("&typeID={0}", typeID);
		}
		CheatRequest request = new CheatRequest();
		yield return request.SendDeleteRequest(urlBuilder.ToString());
	}

	private IEnumerator RemoveResourceCoroutine(string resourceName, params KeyValuePair<string, string>[] paramValuePairs)
	{
		yield return LocateServer();
		if (!m_isReady)
		{
			LogError("Failed to locate cheat server. Please ensure that the server has Config.Util.Cheat=true enabled.");
			yield break;
		}
		ulong gameAccountId = BattleNet.GetMyGameAccountId().Low;
		StringBuilder urlBuilder = new StringBuilder();
		urlBuilder.AppendFormat("{0}/cheat/{1}?accountId={2}", m_baseUrl, resourceName, gameAccountId);
		StringBuilder paramStringBuilder = new StringBuilder();
		for (int i = 0; i < paramValuePairs.Length; i++)
		{
			KeyValuePair<string, string> pair = paramValuePairs[i];
			paramStringBuilder.AppendFormat("&{0}={1}", pair.Key, pair.Value);
		}
		urlBuilder.Append(paramStringBuilder.ToString());
		CheatRequest request = new CheatRequest();
		yield return request.SendDeleteRequest(urlBuilder.ToString());
	}

	public static void LogError(string message)
	{
		if (!HearthstoneApplication.IsPublic())
		{
			UIStatus.Get().AddError(message);
			Debug.LogError(message);
		}
	}
}
