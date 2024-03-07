using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Transform;
using BepInEx;
using Google.MiniJSON;
using Newtonsoft.Json;
using SimpleGUI.Submods.UnitClipboard;
using SimpleJSON;
using UnityEngine;

namespace SimpleGUI.Submods.SimpleMessages
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Messages : BaseUnityPlugin {


        public const string pluginGuid = "cody.worldbox.simple.messages";
        public const string pluginName = "SimpleMessages";
        public const string pluginVersion = "0.0.0.3";
        public static int windowInUse = 0;
        public static List<ModMessage> listOfMessages = new List<ModMessage>();
        public static int messageID;
        public float lastTimer;

        public static Messages self;

        public void Awake()
        {
            Debug.Log("Messages awake");
            self = this;
            CreateStartPhrases();
            CreateResponseDicts();
            Debug.Log("Messages awake end");
        }

        public static void ActorSay(Actor targetActor, string messageText, float duration = 3f)
        {
			if(actorsOnCooldown.Contains(targetActor)) {
                return;
			}
            ModMessage newMessage = new ModMessage
            {
                id = messageID // id starts at 0, random start addition to make sure no conflict with other mod menu ids
            };
            messageID++; // and increments upwards each time
            newMessage.assignedActor = targetActor;
            newMessage.duration = duration;
            newMessage.startTime = Time.realtimeSinceStartup;
            newMessage.MessageText = messageText;
            listOfMessages.Add(newMessage);

            timeAtLastMessage = Time.realtimeSinceStartup; // track time for auto reply

            replyActor = null; // reset to be sure
            replyString = null;

            //potential replier to the message being said right now
            Actor potentialReplier = UnitClipboard_Main.ClosestActorToTile(targetActor.currentTile, 5f, targetActor);
            if(potentialReplier != null && !actorsOnCooldown.Contains(potentialReplier /*do this part different*/)) {
                if(actorResponses.TryGetValue(messageText, out ResponseData response)) {
                    //make sure response has assigned actor asset id
                    if(response.actorAssetID != null) {
                        //and potential replier matches that id
                        if(response.actorAssetID.Contains(potentialReplier.asset.id)){
                            reply = response;
                            replyActor = potentialReplier;
                            actorReplyActorIsRespondingTo = targetActor;
                            stringToReplyTo = messageText;
                        }
                    }
                    //actorAssetId is null, use any actor nearby
                    else {
                        Debug.Log("Actor asset ID for reply not in list or list is null, choosing any available");
                        reply = response;
                        replyActor = potentialReplier;
                        actorReplyActorIsRespondingTo = targetActor;
                        stringToReplyTo = messageText;
                        /*
                        reply = null;
                        replyActor = null;
                        actorReplyActorIsRespondingTo = null;
                        replyString = null;
                        stringToReplyTo = null;*/
                    }
                }
                //if no replies, assign null
                else {
                    Debug.Log("No response found for:" + messageText);
                    reply = null;
                    replyActor = null;
                    actorReplyActorIsRespondingTo = null;
                    replyString = null;
                    stringToReplyTo = null;
                }
            }
            else {
                Debug.Log("No potential replier found");
            }

            //handle cooldown so actors dont say multiple things at once
            actorsOnCooldown.Add(targetActor);
            IEnumerator coroutine = WaitAndRemoveActorFromCooldown(duration, targetActor);
            self.StartCoroutine(coroutine);
        }

        public static IEnumerator WaitAndRemoveActorFromCooldown(float time, Actor target)
        {
            yield return new WaitForSeconds(time);
			if(actorsOnCooldown.Contains(target)) {
                actorsOnCooldown.Remove(target);
            }
        }

        public static List<Actor> actorsOnCooldown = new List<Actor>();

        public static void replyUpdate()
        {
            if(replyActor != null && reply != null) {
                if(timeAtLastMessage == 0f || timeAtLastMessage + 2.5f < Time.realtimeSinceStartup) {
                    //if original message has replies saved
                    if(reply.actorReply.Count > 0) {
                        //if response can be responded to by this actor type
                        if(reply.actorAssetID.Contains(replyActor.asset.id)) {
                            //response to say
                            replyString = reply.actorReply.GetRandom();
                            timeAtLastMessage = Time.realtimeSinceStartup; // auto reply after a time
                        }
                    }

                    //if response has action to execute
                    //target actor is origin speaker, replier is expected to be last initiator
                    if(reply.responseAction != null) {
                        //example: fighting, actor will execute attack against the person who was last speaking
                        reply.responseAction(replyActor, actorReplyActorIsRespondingTo);
                    }

                    //if reply string was found before, say it now
                    if(replyString != null) {
                        ActorSay(replyActor, replyString);
                    }
                }
            }
            else { // random message attempt every 5 seconds?
                if(timeAtLastMessage == 0f || timeAtLastMessage + 5f < Time.realtimeSinceStartup) {
                    //Actor target = MapBox.instance.units.GetRandom();
                }
            }
        }


        //change to list or dict of multiple conversations?
        public static Actor replyActor;
        public static Actor actorReplyActorIsRespondingTo;
        public static string replyString;
        public static string stringToReplyTo;
        public static ResponseData reply;

        public static float timeAtLastMessage;


        public void OnGUI()
        {
            if(listOfMessages.Count >= 1) {
                for(int i = 0; i < listOfMessages.Count; i++) {
                    ModMessage activeMessage = listOfMessages[i];
                    if(activeMessage.startTime + activeMessage.duration > Time.realtimeSinceStartup) {
                        Actor actor = activeMessage.assignedActor;
                        ActorData data = null;
                        if(actor != null)
                            data = actor.data;
                        if(data != null && data.alive) {
                            Vector2 textDimensions = GUI.skin.window.CalcSize(new GUIContent(activeMessage.MessageText));
                            if (Camera.main != null)
                            {
                                Vector3 position = Camera.main.WorldToScreenPoint(actor.gameObject.transform.position);
                                // adding a random number (3536) to make sure theres no conflict with window id in other mods
                                Rect window = GUILayout.Window(activeMessage.id + 3536,
                                    new Rect(position.x - (textDimensions.x / 2), Screen.height - position.y - (textDimensions.y * 2), textDimensions.x, textDimensions.y),
                                    ActorMessageDisplayWindow,
                                    activeMessage.TitleText);
                            }
                        }
                    }
                }
            }
        }
        public void Update()
        {
            if (GuiMain.useDebugHotkeys && Input.GetKeyDown(KeyCode.M))
            {
                Actor potentialStarter = UnitClipboard_Main.ClosestActorToTile(MapBox.instance.getMouseTilePos(), 5f);
                if (potentialStarter != null)
                {
                    string id = potentialStarter.asset.id;
                    Console.WriteLine("actor found with id:" + id);
                    if (Messages.actorStartPhrases.ContainsKey(id))
                    {
                        string phraseToSay = Messages.actorStartPhrases[id].starterPhrases.GetRandom();
                        Messages.ActorSay(potentialStarter, phraseToSay, 3);
                    }
                    else
                    {
                        Console.WriteLine("No conversation starter found for actor ID");
                    }
                }
                else
                {
                    Console.WriteLine("No actor found near cursor");
                }
            }
            Messages.replyUpdate();
        }

        //for refreshing changes mid-gameplay
        public static void Reload()
        {
            LoadStartersFromJson();
            LoadCustomsFromJson();
        }

        public static void LoadStartersFromJson()
        {
            string path1 = Application.streamingAssetsPath + "/messages";
            //load starters
            foreach (string f in Directory.GetFiles(path1 + "/starters"))
            {
                string fileName = RemoveInvalidChars(f.Replace(path1 + "/starters", "").Replace(".json", ""));
                if (actorStartPhrases.ContainsKey(fileName))
                {
                    actorStartPhrases[fileName] = JsonUtility.FromJson<StarterData>(File.ReadAllText(f));
                }
                else
                {
                    actorStartPhrases.Add(fileName, JsonUtility.FromJson<StarterData>(File.ReadAllText(f)));
                }
            }
            //load replies
            foreach (string f in Directory.GetFiles(path1 + "/replies"))
            {
                ResponseData response = JsonUtility.FromJson<ResponseData>(File.ReadAllText(f));
                if (actorResponses.ContainsKey(response.inputToRespondTo))
                {
                    actorResponses[response.inputToRespondTo] = response;
                }
                else
                {
                    actorResponses.Add(response.inputToRespondTo, response);
                }
            }
        }
        public static void LoadCustomsFromJson()
        {
            string path1 = Application.streamingAssetsPath + "/messages/custom/";
            if (File.Exists(path1 + "escortCommand.json"))
            {
                string[] lines = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(path1 + "escortCommand.json"));
                GuiMain.ActorControl.escortCommandLines = lines;
            }
        }

        //export pre-made message assets so there is reference to create new ones
        public static void SaveCurrent()
        {
            string path1 = Application.streamingAssetsPath + "/messages";
            //check for and create the paths if needed
            if(Directory.Exists(path1) == false)
            {
                Directory.CreateDirectory(path1);
            }
            if (Directory.Exists(path1 + "/starters") == false)
            {
                Directory.CreateDirectory(path1 + "/starters");
            }
            if (Directory.Exists(path1 + "/replies") == false)
            {
                Directory.CreateDirectory(path1 + "/replies");
            }
            if (Directory.Exists(path1 + "/custom") == false)
            {
                Directory.CreateDirectory(path1 + "/custom");
            }

            path1 = Application.streamingAssetsPath + "/messages/starters/";
            foreach (KeyValuePair<string, StarterData> starterData in actorStartPhrases)
            {
                string path2 = path1 + starterData.Key + ".json";
                string toSave = JsonUtility.ToJson(starterData.Value, true);
                File.WriteAllText(path2, toSave);
                Console.WriteLine(starterData);
            }

            path1 = Application.streamingAssetsPath + "/messages/replies/";
            foreach (KeyValuePair<string, ResponseData> responseData in actorResponses)
            {
                //cleanup the string to save as filename
                var invalidChars = Path.GetInvalidFileNameChars();
                string invalidCharsRemoved = new string(responseData.Key
                  .Where(x => !invalidChars.Contains(x))
                  .ToArray());
                //combine path with newly cleaned name
                string path2 = path1 + invalidCharsRemoved + ".json";
                string toSave = JsonUtility.ToJson(responseData.Value, true);
                File.WriteAllText(path2, toSave);
            }

            path1 = Application.streamingAssetsPath + "/messages/custom/";
            if (File.Exists(path1 + "escortCommand.json") == false)
            {
                string toSave = JsonConvert.SerializeObject(GuiMain.ActorControl.escortCommandLines);
                File.WriteAllText(path1 + "escortCommand.json", toSave);
            }
            if (File.Exists(path1 + "escortEncouragement.json") == false)
            {
                string toSave = JsonConvert.SerializeObject(GuiMain.ActorControl.escortEncourageLines);
                File.WriteAllText(path1 + "escortEncouragement.json", toSave);
            }
            if (File.Exists(path1 + "fear.json") == false)
            {
                string toSave = JsonConvert.SerializeObject(GuiMain.ActorControl.fearLines);
                File.WriteAllText(path1 + "fear.json", toSave);
            }
            if (File.Exists(path1 + "sorry.json") == false)
            {
                string toSave = JsonConvert.SerializeObject(GuiMain.ActorControl.sorryLines);

                File.WriteAllText(path1 + "sorry.json", toSave);
            }
        }

        public static string RemoveInvalidChars(string filename)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                filename = filename.Replace(c.ToString(), "");
            }
            return filename;
        }

        public void ActorMessageDisplayWindow(int windowID)
        {
            ModMessage activeMessage = listOfMessages[windowID - 3536]; // removing the conflict number
            if(activeMessage != null) {
                GUILayout.Label(activeMessage.MessageText);
            }
            GUI.DragWindow();
        }

        public void CreateStartPhrases(){
            actorStartPhrases.Add("unit_human", new StarterData(new string[] { "Hello.", "What do you want?", "Can I help you?" }));
            actorStartPhrases.Add("chicken", new StarterData(new string[] { "bu-bock", "bock!", "Where's my egg?", "Do I smell grease?", "Eat more beef" }));
            actorStartPhrases.Add("rooster", new StarterData(new string[] { "bu-bock", "bock!", "Where's my egg?", "Do I smell grease?", "Eat more beef" }));
            actorStartPhrases.Add("unit_elf", new StarterData(new string[] { "Greetings.", "Sshhh...", "I'm a vegan." }));
            actorStartPhrases.Add("unit_dwarf", new StarterData(new string[] { "Oy!", "Get back to work.", "Where did I leave my cup?", "Ooww, my head..", "You're big!" }));
            actorStartPhrases.Add("unit_orc", new StarterData(new string[] { "Grrr..", "Go away.", "Want to fight?", "I'll break your bones.", "You are small.", "Meat... *drools*" }));
            actorStartPhrases.Add("baby_human", new StarterData(new string[] { "That tickles!", "Want to play?", "Look over there!", "Where's my friend?", "I'm a big kid!" }));
            actorStartPhrases.Add("baby_elf", new StarterData(new string[] { "That tickles!", "Want to play?", "Look over there!", "Where's my friend?", "I'm a big kid!" }));
            actorStartPhrases.Add("baby_orc", new StarterData(new string[] { "That tickles!", "Want to play?", "Look over there!", "Where's my friend?", "I'm a big kid!" }));
            actorStartPhrases.Add("baby_dwarf", new StarterData(new string[] { "That tickles!", "Want to play?", "Look over there!", "Where's my friend?", "I'm a big kid!" }));
            actorStartPhrases.Add("dragon", new StarterData(new string[] { "Grrr...", "I'm tired.", "Where's my treasure?", "BURN!", "So small!", "RAH!" }));
            actorStartPhrases.Add("sheep", new StarterData(new string[] { "Ba.", "Baaaahh!", "Shh.. They can't know.", ".....", "Buuuhh.", "What are you doing?" }));
            actorStartPhrases.Add("cow", new StarterData(new string[] { "Mooo", "Mooooove!", "Shh.. They can't know.", ".....", "*Chews grass*" }));
            actorStartPhrases.Add("penguin", new StarterData(new string[] { "It might be a spy.", "What sort of noises does a penguin make?", "How did I get here?", "My feet don't have feelings..." }));
            actorStartPhrases.Add("turtle", new StarterData(new string[] { "Is that a bit of moss?", "How old am I?", "Where is my egg?", ".....", "Wow you're fast..." }));
            actorStartPhrases.Add("river_turtle", new StarterData(new string[] { "Is that a bit of moss?", "How old am I?", "Where is my egg?", ".....", "Wow you're fast..." }));
            actorStartPhrases.Add("crab", new StarterData(new string[] { "Click click.", "Let's dance!", "There's a rave going down later...", ".....", "Where's the big one?" }));
            actorStartPhrases.Add("fairy", new StarterData(new string[] { "Watch out for lightning..", "Do you hear them?", "Shh.. They're coming", ".....", "*Giggles*" }));
            actorStartPhrases.Add("enchanted_fairy", new StarterData(new string[] { "Watch out for lightning..", "Do you hear them?", "Shh.. They're coming", ".....", "*Giggles*" }));
        }

        public void CreateResponseDicts(){
            ResponseData hello1 = new ResponseData
            {
                inputToRespondTo = "Hello.",
                actorAssetID = new List<string> { "unit_human", "unit_elf", "unit_dwarf" },
                actorReply = new List<string> { "How are you?", "Greetings.", "Lovely weather.", "Hello!" }
            };
            actorResponses.Add(hello1.inputToRespondTo, hello1);

            ResponseData hello2 = new ResponseData
            {
                inputToRespondTo = "Hello!",
                actorAssetID = new List<string> { "unit_human", "unit_elf", "unit_dwarf" },
                actorReply = new List<string> { "How are you?", "Greetings.", "Can I help you?" }
            };
            actorResponses.Add(hello2.inputToRespondTo, hello2);

            ResponseData howAreYou = new ResponseData
            {
                inputToRespondTo = "How are you?",
                actorAssetID = new List<string> { "unit_human", "unit_elf", "unit_dwarf" },
                actorReply = new List<string> { "I'm doing well.", "A bit ill..", "Alright and you?", "I'm great!" }
            };
            actorResponses.Add(howAreYou.inputToRespondTo, howAreYou);

            ResponseData aBitIll = new ResponseData
            {
                inputToRespondTo = "A bit ill..",
                actorAssetID = new List<string> { "unit_human", "unit_elf", "unit_dwarf" },
                actorReply = new List<string> { "Sorry to hear that..", "Stay away from me, please.", "Who'd you catch it from?" }
            };
            actorResponses.Add(aBitIll.inputToRespondTo, aBitIll);

            ResponseData imDoingWell = new ResponseData
            {
                inputToRespondTo = "I'm doing well.",
                actorAssetID = new List<string> { "unit_human", "unit_elf", "unit_dwarf" },
                actorReply = new List<string> { "That's good.", "Glad to hear.", "Aye me too.", "Nice." }
            };
            actorResponses.Add(imDoingWell.inputToRespondTo, imDoingWell);

            ResponseData canIHelpYou = new ResponseData
            {
                inputToRespondTo = "Can I help you?",
                actorAssetID = new List<string> { "unit_human", "unit_elf", "unit_dwarf" },
                actorReply = new List<string> { "If you wouldn't mind...", "No, thank you.", "I can help too!" }
            };
            actorResponses.Add(canIHelpYou.inputToRespondTo, canIHelpYou);

            ResponseData ifYouWouldntMind = new ResponseData
            {
                inputToRespondTo = "If you wouldn't mind..",
                actorAssetID = new List<string> { "unit_human", "unit_elf", "unit_dwarf" },
                actorReply = new List<string> { "Okay, what do you need?", "I can help, let's go." }
            };
            actorResponses.Add(ifYouWouldntMind.inputToRespondTo, ifYouWouldntMind);

            ResponseData noThankYou = new ResponseData
            {
                inputToRespondTo = "No, thank you.",
                actorAssetID = new List<string> { "unit_human", "unit_elf", "unit_dwarf" },
                actorReply = new List<string> { "Okay.", "If you say so.", "Just ask if you need me!" }
            };
            actorResponses.Add(noThankYou.inputToRespondTo, noThankYou);

            ResponseData no = new ResponseData
            {
                inputToRespondTo = "No.",
                actorAssetID = new List<string> { "unit_human", "unit_elf", "unit_dwarf" },
                actorReply = new List<string> { "Okay.", "A little rude..", "I wasn't even asking you.", "Fine." }
            };
            actorResponses.Add(no.inputToRespondTo, no);

            ResponseData wheresMyFriend = new ResponseData
            {
                inputToRespondTo = "Where's my friend?",
                actorAssetID = new List<string> { "unit_human", "unit_elf", "unit_dwarf" },
                actorReply = new List<string> { "He's over there!", "I'm right here!", "Are you lost?", "I don't know." }
            };
            actorResponses.Add(wheresMyFriend.inputToRespondTo, wheresMyFriend);

            ResponseData example1 = new ResponseData
            {
                inputToRespondTo = "Howdy!",
                //actor types that can respond to this message
                actorAssetID = new List<string> { "unit_human" },
                actorReply = new List<string> { "Hows it going?", "Hello.", "Wanna fight?" }
                //no response action here
            };
            
            ResponseData example2 = new ResponseData
            {
                inputToRespondTo = "Wanna fight?",
                actorAssetID = new List<string> { "unit_human" },
                actorReply = new List<string> { "Bring it on!", "Let's go!" },
                //actor will respond with one of the above, and also attempt to attack whoever asked
                responseAction = startFight
            };

            actorResponses.Add(example1.inputToRespondTo, example1);
            actorResponses.Add(example2.inputToRespondTo, example2);
        }

        public static void startFight(Actor origin, Actor target)
        {
            bool didAttack = origin.tryToAttack(target);
            Debug.Log("attempting to attack from conversation ending: " + didAttack);
        }


        //key: statsID, value: list/array of phrases to start conversations
        public static Dictionary<string, StarterData> actorStartPhrases = new Dictionary<string, StarterData>();
        //key: inputToRespondTo, value: response data
        public static Dictionary<string, ResponseData> actorResponses = new Dictionary<string, ResponseData>();
    }

    [Serializable]
    public delegate void ResponseAction(Actor pActor = null, Actor pTarget = null);

    [Serializable]
    public class StarterData
    {
        //actor id is assigned by dict already
        public string[] starterPhrases;

        public StarterData(string[] starterPhrases)
        {
            this.starterPhrases = starterPhrases;
        }
    }

    [Serializable]
    public class ResponseData {
        //input to start these responses, cannot be null
        //maybe this should be key in dict and actor id is separate?
        public string inputToRespondTo;

        //list of actor ids that can respond with this
        public List<string> actorAssetID;

        //key: statsID, value: list/array of replies
        //if statsID is not in here, actor type will not verbally respond to input
        public List<string> actorReply = new List<string>();

        //possible action to execute with certain inputs
        //thems fightin words
        public ResponseAction responseAction;
    }
 
    [Serializable]
    public class ModMessage {
        public int id;
        public Actor assignedActor;
        public string TitleText = "";
        public string MessageText = "";
        public float duration = 3.0f;
        public float startTime;
    }
}