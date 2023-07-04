using System;
using BepInEx;
using BepInEx.Configuration;
using OpenAI_API;
using UnityEngine;

namespace SimpleGUI.Submods.SimpleAI {
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class SimpleAI : BaseUnityPlugin {
		public const string pluginGuid = "cody.worldbox.simple.ai";
		public const string pluginName = "SimpleAI (Using OpenAI)";
		public const string pluginVersion = "0.0.0.2";

		public static bool showHideInputWindow;
		public static Rect inputWindowRect = new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 100, 10, 10);

		public static bool showHideCustomPersonWindow;
		public static Rect customPersonWindowRect = new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 100, 10, 10);

		public static OpenAIAPI api;
		public static ConfigEntry<string> apiKey {
			get; set;
		}
		public static Engine selectedEngine = Engine.Ada;

		public string[] descriptor1List = { "grumpy", "wicked", "delirious", "sarcastic", "smelly", "happy", "wounded", "scarred" };

		public string[] descriptor2List = { "clever and helpful, yet rude", "young and stylish", "a little too ", "a bit ", "strangely ", "kinda ", "dressed nakedly", "breathing heavily", "really sweaty"};
		public string[] descriptor2Suffix = { "calm", "loud", "nervous", "excited", "giddy", "cautious", "touchy"};
		public string[] descriptor2SuffixPart2 = { "for your taste", "to be feeling safe", "compared to usual", "since the last time you met"};

		public void GenerateNewAI()
		{
			string currentName = aiPerson.pName;
			while(aiPerson.pName == currentName) {
				try {
					ActorAsset randomStats = AssetManager.actor_library.list.GetRandom();
					string nameTemplate = randomStats.nameTemplate;
					aiPerson = new AIPersonality
					{
						pName = NameGenerator.generateNameFromTemplate(AssetManager.nameGenerator.get(nameTemplate)),
						pDescriptor1 = descriptor1List.GetRandom(),
						pRace = randomStats.nameLocale,
						pDescriptor2 = descriptor2List.GetRandom(),
						pQuestion = "How can I help you?"
					};
					customName = aiPerson.pName;
					customDescriptor1 = aiPerson.pDescriptor1;
					customRace = aiPerson.pRace;
					customDescriptor2 = aiPerson.pDescriptor2;
					customQuestion = aiPerson.pQuestion;
				}
				catch(Exception e) {

				}
			}
			string suffix = "";
			string suffix2 = "";
			// add suffix to specific descriptors
			if(aiPerson.pDescriptor2 == "a bit " || aiPerson.pDescriptor2 == "a little too " || aiPerson.pDescriptor2 == "kinda " || aiPerson.pDescriptor2 == "strangely ") {
				
				suffix = descriptor2Suffix.GetRandom();
				aiPerson.pDescriptor2 = aiPerson.pDescriptor2 + suffix;
				if(Toolbox.randomBool() && Toolbox.randomBool()) {
					suffix2 = descriptor2SuffixPart2.GetRandom();
					aiPerson.pDescriptor2  = aiPerson.pDescriptor2  + " " + suffix2;
				}
			}
			customDescriptor2Suffix = suffix;
			customDescriptor2SuffixPt2 = suffix2;
		}

		public void Awake()
		{
			apiKey = Config.AddSetting("API", "Key", "", "Your personal API key. Important: Do not share with others.");
		}

		public void Start()
		{
			
		}

		public void Update()
		{
			inputWindowRect.height = 2f;
			inputWindowRect.width = 2f;
			customPersonWindowRect.height = 2f;
			customPersonWindowRect.width = 2f;
		}

		public bool firstOpen;
		public void OnGUI()
		{
			
			if (GUI.Button(new Rect(Screen.width - 120, 100, 120, 20), "SimpleAI"))
			{
				if(firstOpen == false) {
					GenerateNewAI();
					firstOpen = true;
				}
				showHideInputWindow = !showHideInputWindow;
				showHideCustomPersonWindow = !showHideCustomPersonWindow;
			}
			
			if(showHideInputWindow) {
				inputWindowRect = GUILayout.Window(139021, inputWindowRect, InputWindow, "OpenAI WorldBox", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
			}
			if(showHideCustomPersonWindow) {
				customPersonWindowRect = GUILayout.Window(139022, customPersonWindowRect, CustomPersonWindow, "OpenAI WorldBox", GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f));
			}
			SetWindowInUse(-1);
		}

		public string customName = "";
		public string customDescriptor1 = "";
		public string customRace = "";
		public string customDescriptor2 = "";
		public string customDescriptor2Suffix = "";
		public string customDescriptor2SuffixPt2 = "";
		public string customQuestion = "";

		public void CustomPersonWindow(int windowID)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Button("Name");
			customName = GUILayout.TextField(customName);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Button("Desc1");
			customDescriptor1 = GUILayout.TextField(customDescriptor1);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Button("Race");
			customRace = GUILayout.TextField(customRace);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Button("Desc2");
			customDescriptor2 = GUILayout.TextField(customDescriptor2);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Button("Desc2Suf");
			customDescriptor2Suffix = GUILayout.TextField(customDescriptor2Suffix);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Button("Desc2SufPt2");
			customDescriptor2SuffixPt2 = GUILayout.TextField(customDescriptor2SuffixPt2);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Button("Question");
			customQuestion = GUILayout.TextField(customQuestion);
			GUILayout.EndHorizontal();
			if(GUILayout.Button("Set personality")) {
				aiPerson = new AIPersonality
				{
					pName = customName,
					pDescriptor1 = customDescriptor1,
					pRace = customRace,
					pDescriptor2 = customDescriptor2,
					pQuestion = customQuestion
				};
				if(string.IsNullOrEmpty(customDescriptor2Suffix) == false) {
					aiPerson.pDescriptor2 = aiPerson.pDescriptor2 + " " + customDescriptor2Suffix;
				}
				if(string.IsNullOrEmpty(customDescriptor2SuffixPt2) == false) {
					aiPerson.pDescriptor2 = aiPerson.pDescriptor2 + " " + customDescriptor2SuffixPt2;
				}
			}

			GUI.DragWindow();
		}
		public string inputString = "";
		public void InputWindow(int windowID)
		{
			SetWindowInUse(windowID);
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Regen")) {
				ResetPrompt();
				GenerateNewAI();
			}
			if(GUILayout.Button("Reset")) {
				ResetPrompt();
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Ada")) {
				selectedEngine = Engine.Ada;
				ResetPrompt();
			}
			if(GUILayout.Button("Babbage")) {
				selectedEngine = Engine.Babbage;
				ResetPrompt();
			}
			if(GUILayout.Button("Curie")) {
				selectedEngine = Engine.Curie;
				ResetPrompt();
			}
			if(GUILayout.Button("Davinci")) {
				selectedEngine = Engine.Davinci;
				ResetPrompt();
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("Output (" + selectedEngine.EngineName + ")");
			if(displayString.IsNullOrWhiteSpace() == false) {
				GUILayout.Label(displayString);
			}
			else {
				GUILayout.Label(initialPrompt);
			}

			GUILayout.Label("Input");

			inputString = GUILayout.TextField(inputString);
			
			if(GUILayout.Button("Submit")) {
				SubmitTest(inputString, true);
			}

			GUI.DragWindow();
		}

		private static void ResetPrompt()
		{
			tlastprompt = "";
			lastResult = "";
			newFullPrompt = "";
			displayString = "";
		}

		public async void SubmitTest(string input, bool includeLastResult = false)
		{
			OpenAIAPI api = new OpenAIAPI(apiKey.Value, selectedEngine);

			if(includeLastResult) {
				if(string.IsNullOrEmpty(lastResult) == false) {
					string fullPrompt = newFullPrompt + "\nYou: " + input + "\n" + aiPerson.pName + ":";
					tlastprompt = fullPrompt;
					//Debug.Log("(db)full prompt2: " + fullPrompt);
					CompletionRequest newRequest = new CompletionRequest
					{
						Temperature = 1.0,
						MultipleStopSequences = new[] { "\n", " You:", " " + aiPerson.pName + ":" },
						MaxTokens = 150,
						TopP = 1,
						FrequencyPenalty = 0,
						PresencePenalty = 0.6,
						Prompt = fullPrompt
					};
					CompletionResult result = await api.Completions.CreateCompletionAsync(newRequest);
					lastResult = result.ToString();
					newFullPrompt = fullPrompt + result;
					displayString = newFullPrompt;
					//Debug.Log("(db)result2: " + result);

				}
				else {
					string fullPrompt = "The following is a conversation with a " + aiPerson.pDescriptor1 + " " + aiPerson.pRace + ". The " + aiPerson.pRace + " is " + aiPerson.pDescriptor2 + "\n\nYou: Hello, who are you?\n" + aiPerson.pName + ": I am " + aiPerson.pName + ", a " + aiPerson.pRace + ". How can I help you today?\nYou: " + input + "\n" + aiPerson.pName + ":";
					tlastprompt = fullPrompt;
					//Debug.Log("(db)full prompt: " + fullPrompt);
					CompletionRequest newRequest = new CompletionRequest
					{
						Temperature = 1.0,
						MultipleStopSequences = new[] { "\n", " You:", " " + aiPerson.pName + ":" },
						MaxTokens = 150,
						TopP = 1,
						FrequencyPenalty = 0,
						PresencePenalty = 0.6,
						Prompt = fullPrompt
					};
					CompletionResult result = await api.Completions.CreateCompletionAsync(newRequest);
					lastResult = result.ToString();
					newFullPrompt = fullPrompt + result;
					displayString = newFullPrompt;
					//Debug.Log("(db)result: " + result);
				}
			}

			inputString = "";
			//Debug.Log("@@@: " + result.ToString());
		}

		public static void SetWindowInUse(int windowID)
		{
			Event current = Event.current;
			bool inUse = current.type == EventType.MouseDown || current.type == EventType.MouseUp || current.type == EventType.MouseDrag || current.type == EventType.MouseMove;
			if(inUse) {
				windowInUse = windowID;
			}
		}

		public static int windowInUse = -1;
		public static AIPersonality aiPerson = new AIPersonality();

		public static string tlastprompt = "";
		public static string lastResult = "";
		public static string newFullPrompt = "";
		public static string displayString = "";
		public static string initialPrompt => "The following is a conversation with a " + aiPerson.pDescriptor1 + " " + aiPerson.pRace + ". The " + aiPerson.pRace + " is " + aiPerson.pDescriptor2 + ".\n\nYou: Hello, who are you?\n" + aiPerson.pName + ": I am " + aiPerson.pName + ", a " + aiPerson.pRace + ". " + aiPerson.pQuestion + "\nYou: ";
		// ^ needs work, or variance etc

		public class AIPersonality {
			public string pName = ""; //NameGenerator.generateNameFromTemplate(AssetManager.nameGenerator.get("evil_mage_name"));
			public string pRace = ""; //AssetManager.raceLibrary.list.GetRandom().id.Replace("_", " ");
			public string pDescriptor1 = "";
			public string pDescriptor2 = "";
			public string pQuestion = "";

		}
	}
}
