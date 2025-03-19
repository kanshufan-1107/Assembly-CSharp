using System;
using System.Collections.Generic;
using System.ComponentModel;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using BobNetProto;
using Hearthstone.Core;

public class DebugConsole : IService
{
	private class CommandParamDecl
	{
		public enum ParamType
		{
			[Description("string")]
			STR,
			[Description("int32")]
			I32,
			[Description("float32")]
			F32,
			[Description("bool")]
			BOOL
		}

		public string Name;

		public ParamType Type;

		public CommandParamDecl(ParamType type, string name)
		{
			Type = type;
			Name = name;
		}
	}

	private delegate void ConsoleCallback(List<string> commandParams);

	private class ConsoleCallbackInfo
	{
		public bool DisplayInCommandList;

		public List<CommandParamDecl> ParamList;

		public ConsoleCallback Callback;

		public ConsoleCallbackInfo(bool displayInCmdList, ConsoleCallback callback, CommandParamDecl[] commandParams)
		{
			DisplayInCommandList = displayInCmdList;
			ParamList = new List<CommandParamDecl>(commandParams);
			Callback = callback;
		}

		public ConsoleCallbackInfo(bool displayInCmdList, ConsoleCallback callback, List<CommandParamDecl> commandParams)
			: this(displayInCmdList, callback, commandParams.ToArray())
		{
		}

		public int GetNumParams()
		{
			return ParamList.Count;
		}
	}

	private enum DebugConsoleResponseType
	{
		CONSOLE_OUTPUT,
		LOG_MESSAGE
	}

	private static Map<string, ConsoleCallbackInfo> s_serverConsoleCallbackMap;

	private static Map<string, ConsoleCallbackInfo> s_clientConsoleCallbackMap;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		Network net = serviceLocator.Get<Network>();
		if (net.ShouldBeConnectedToAurora_NONSTATIC())
		{
			Processor.QueueJob("InitializeDebugConsole", Job_InitializeAfterBGSInits(net), (IJobDependency[])null);
		}
		else
		{
			InitializeConsole(net);
		}
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(Network) };
	}

	public void Shutdown()
	{
	}

	private IEnumerator<IAsyncJobResult> Job_InitializeAfterBGSInits(Network net)
	{
		while (!BattleNet.IsInitialized())
		{
			yield return null;
		}
		InitializeConsole(net);
	}

	private void InitializeConsole(Network net)
	{
		InitConsoleCallbackMaps();
		net.RegisterNetHandler(DebugConsoleCommand.PacketID.ID, OnCommandReceived);
		net.RegisterNetHandler(DebugConsoleResponse.PacketID.ID, OnCommandResponseReceived);
	}

	private static List<CommandParamDecl> CreateParamDeclList(params CommandParamDecl[] paramDecls)
	{
		List<CommandParamDecl> paramList = new List<CommandParamDecl>();
		foreach (CommandParamDecl paramDecl in paramDecls)
		{
			paramList.Add(paramDecl);
		}
		return paramList;
	}

	private void InitConsoleCallbackMaps()
	{
		InitClientConsoleCallbackMap();
		InitServerConsoleCallbackMap();
	}

	private void InitServerConsoleCallbackMap()
	{
		if (s_serverConsoleCallbackMap == null)
		{
			s_serverConsoleCallbackMap = new Map<string, ConsoleCallbackInfo>();
			s_serverConsoleCallbackMap.Add("spawncard", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.STR, "cardGUID"), new CommandParamDecl(CommandParamDecl.ParamType.I32, "playerID"), new CommandParamDecl(CommandParamDecl.ParamType.STR, "zoneName"), new CommandParamDecl(CommandParamDecl.ParamType.I32, "premium"))));
			s_serverConsoleCallbackMap.Add("loadcard", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.STR, "cardGUID"))));
			s_serverConsoleCallbackMap.Add("drawcard", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "playerID"))));
			s_serverConsoleCallbackMap.Add("shuffle", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "playerID"))));
			s_serverConsoleCallbackMap.Add("cyclehand", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "playerID"))));
			s_serverConsoleCallbackMap.Add("nuke", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "playerID"))));
			s_serverConsoleCallbackMap.Add("damage", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "entityID"), new CommandParamDecl(CommandParamDecl.ParamType.I32, "damage"))));
			s_serverConsoleCallbackMap.Add("addmana", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "playerID"))));
			s_serverConsoleCallbackMap.Add("readymana", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "playerID"))));
			s_serverConsoleCallbackMap.Add("maxmana", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "playerID"))));
			s_serverConsoleCallbackMap.Add("nocosts", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList()));
			s_serverConsoleCallbackMap.Add("healhero", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "playerID"))));
			s_serverConsoleCallbackMap.Add("healentity", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "entityID"))));
			s_serverConsoleCallbackMap.Add("ready", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "entityID"))));
			s_serverConsoleCallbackMap.Add("exhaust", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "entityID"))));
			s_serverConsoleCallbackMap.Add("freeze", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "entityID"))));
			s_serverConsoleCallbackMap.Add("move", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList(new CommandParamDecl(CommandParamDecl.ParamType.I32, "entityID"), new CommandParamDecl(CommandParamDecl.ParamType.I32, "zoneID"))));
			s_serverConsoleCallbackMap.Add("tiegame", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList()));
			s_serverConsoleCallbackMap.Add("aiplaylastspawnedcard", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList()));
			s_serverConsoleCallbackMap.Add("forcestallingprevention", new ConsoleCallbackInfo(displayInCmdList: true, null, CreateParamDeclList()));
		}
	}

	private void InitClientConsoleCallbackMap()
	{
		if (s_clientConsoleCallbackMap == null)
		{
			s_clientConsoleCallbackMap = new Map<string, ConsoleCallbackInfo>();
		}
	}

	private void SendDebugConsoleResponse(DebugConsoleResponseType type, string message)
	{
		Network.Get().SendDebugConsoleResponse((int)type, message);
	}

	private void SendConsoleCmdToServer(string commandName, List<string> commandParams)
	{
		if (!s_serverConsoleCallbackMap.ContainsKey(commandName))
		{
			return;
		}
		string fullCommand = commandName;
		foreach (string param in commandParams)
		{
			fullCommand = fullCommand + " " + param;
		}
		if (!Network.Get().SendDebugConsoleCommand(fullCommand))
		{
			SendDebugConsoleResponse(DebugConsoleResponseType.CONSOLE_OUTPUT, $"Cannot send command '{commandName}'; not currently connected to a game server.");
		}
	}

	private void OnCommandReceived()
	{
		string[] commandWords = Network.Get().GetDebugConsoleCommand().Split(' ');
		if (commandWords.Length == 0)
		{
			Log.All.Print("Received empty command from debug console!");
			return;
		}
		string commandName = commandWords[0];
		List<string> commandParams = new List<string>();
		for (int idx = 1; idx < commandWords.Length; idx++)
		{
			commandParams.Add(commandWords[idx]);
		}
		if (s_serverConsoleCallbackMap.ContainsKey(commandName))
		{
			SendConsoleCmdToServer(commandName, commandParams);
			return;
		}
		if (!s_clientConsoleCallbackMap.ContainsKey(commandName))
		{
			SendDebugConsoleResponse(DebugConsoleResponseType.CONSOLE_OUTPUT, $"Unknown command '{commandName}'.");
			return;
		}
		ConsoleCallbackInfo callbackInfo = s_clientConsoleCallbackMap[commandName];
		if (callbackInfo.GetNumParams() != commandParams.Count)
		{
			SendDebugConsoleResponse(DebugConsoleResponseType.CONSOLE_OUTPUT, $"Invalid params for command '{commandName}'.");
			return;
		}
		Log.All.Print($"Processing command '{commandName}' from debug console.");
		callbackInfo.Callback(commandParams);
	}

	private void OnCommandResponseReceived()
	{
		Network.DebugConsoleResponse response = Network.Get().GetDebugConsoleResponse();
		if (response != null)
		{
			SendDebugConsoleResponse((DebugConsoleResponseType)response.Type, response.Response);
		}
		Log.All.Print("DebugConsoleResponse: {0}", string.IsNullOrEmpty(response.Response) ? "<empty>" : response.Response);
		if (!string.IsNullOrEmpty(response.Response))
		{
			UIStatus.Get().AddInfo(response.Response);
		}
	}
}
