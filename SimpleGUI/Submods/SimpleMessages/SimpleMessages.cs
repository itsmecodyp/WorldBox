using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using SimpleGUI.Submods.UnitClipboard;
using UnityEngine;

namespace SimpleGUI.Submods.SimpleMessages
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.simple.lib";
        public const string pluginName = "SimpleLibrary";
        public const string pluginVersion = "0.0.0.3";

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.M)) {
                Actor potentialStarter = UnitClipboard_Main.ClosestActorToTile(MapBox.instance.getMouseTilePos(), 5f);
                if(potentialStarter != null) {
                    string id = potentialStarter.asset.id;
                    Debug.Log("actor found with id:" + id);
                    if(Messages.actorStartPhrases.TryGetValue(id, out var phrase)) {
                        string phraseToSay = phrase.GetRandom();
                        Messages.ActorSay(potentialStarter, phraseToSay, 3);
                    }
                    else {
                        Debug.Log("No conversation starter found for actor ID");
                    }
                }
                else {
                    Debug.Log("No actor found near cursor");
                }
            }

            //if(Input.GetKeyDown(KeyCode.L)) { foreach(Actor actor in MapBox.instance.units) { ActorSay(actor, "Hi, my name is " + actor.data.name, 3f); } }


            /*
            if (lastTimer + 5f < Time.realtimeSinceStartup)
            {
                if(MapBox.instance != null && MapBox.instance.units != null)
                foreach (Actor actor in MapBox.instance.units)
                {
                    ActorStatus data = Reflection.GetField(actor.GetType(), actor, "data") as ActorStatus;
                    string name = data.firstName;
                    if (data.favorite)
                    {
                        Messages.ActorSay(actor, name, 5f);
                    }
                }
                lastTimer = Time.realtimeSinceStartup;
            }
            */
            Messages.replyUpdate();
        }

        public float lastTimer;

    }

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Messages : BaseUnityPlugin {


        public const string pluginGuid = "cody.worldbox.simple.messages";
        public const string pluginName = "SimpleMessages";
        public const string pluginVersion = "0.0.0.3";
        public static int windowInUse = 0;
        public static List<ModMessage> listOfMessages = new List<ModMessage>();
        public static int messageID;

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
            //if(Input.GetKeyDown(KeyCode.L)) { foreach(Actor actor in MapBox.instance.units) { ActorSay(actor, "Hi, my name is " + actor.data.firstName); } }
            //if(Input.GetKeyDown(KeyCode.Alpha0)) { foreach(Actor actor in MapBox.instance.units) { ActorSay(actor, "0"); } }
            //if(Input.GetKeyDown(KeyCode.Alpha1)) { foreach(Actor actor in MapBox.instance.units) { ActorSay(actor, "11111"); } }
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
            string[] starterPhrases;
            //humans
            starterPhrases = new string[] { "Howdy!", "What's up?", "Good day to you.", "Hello!" };
            actorStartPhrases.Add("unit_human", starterPhrases);
        }

        public void CreateResponseDicts(){
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
        public static Dictionary<string, string[]> actorStartPhrases = new Dictionary<string, string[]>();
        //key: inputToRespondTo, value: response data
        public static Dictionary<string, ResponseData> actorResponses = new Dictionary<string, ResponseData>();
    }

    [Serializable]
    public delegate void ResponseAction(Actor pActor = null, Actor pTarget = null);

    public class ResponseData {
        public Actor originActor;
        public Actor respondingActor;

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

    //?
    public class StarterData {
        //list of actor ids that can use this starter
        public List<string> actorAssetID;
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