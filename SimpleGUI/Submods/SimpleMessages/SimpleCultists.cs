using ai;
using ai.behaviours;
using SimplerGUI.Submods.SimpleMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplerGUI.Submods {
	public class SimpleCultists {
		//example use of SimpleMessages system
		
		public void init()
		{
			godOfChoice = NameGenerator.generateNameFromTemplate("orc_clan");
			AddPreachConversations();
			AddTasks();
			AddJobs();
			ActorTrait leaderTrait = new ActorTrait();
			leaderTrait.id = "cultLeader";
			AssetManager.traits.add(leaderTrait);
			/* use orc_clan names for now
			NameGeneratorAsset cultGodNameGenerator = new NameGeneratorAsset();
			{
				cultGodNameGenerator.id = "cult_god";
			}
			*/
		}

		public void AddJobs()
		{
			ActorJob leaderJob = new ActorJob();
			leaderJob.id = "cultLeader";
			leaderJob.addTask("cultPreach");
			leaderJob.addCondition(new CondEnoughFollowers(), true);
			leaderJob.addTask("cultEnd");
			AssetManager.job_actor.add(leaderJob);
		}

		public void AddTasks()
		{
			BehaviourTaskActor preach = new BehaviourTaskActor();
			preach.id = "cultPreach";
			preach.addBeh(new BehFindPreachTile());
			preach.addBeh(new BehGoToTileTarget());
			preach.addBeh(new BehPreach());
			preach.addBeh(new BehWait(3.5f));

			/*
			BehaviourTaskActor follow = new BehaviourTaskActor();
			preach.id = "cultFollow";
			preach.addBeh(new BehFindPreachTile());
			preach.addBeh(new BehGoToTileTarget());
			preach.addBeh(new BehPreach());
			*/

			BehaviourTaskActor cultEnd = new BehaviourTaskActor();
			preach.id = "cultEnd";
			preach.addBeh(new BehFindPreachTile()); // 

			AssetManager.tasks_actor.add(preach);
			AssetManager.tasks_actor.add(cultEnd);

		}

		public void AddPreachConversations()
		{
			string preachInput; // easier assignment of responses this way
			string temp; // mostly for emotion, maybe something else

			preachInput = "Have you heard of " + godOfChoice + "?";
			preachStarters.Add(preachInput);
			ResponseData preachResponseExample1 = new ResponseData {
				inputToRespondTo = preachInput,
				actorAssetID = null,
				actorReply = new List<string> { "Only rumours..", "Tell us more!", godOfChoice + "..?" },
				//responseAction = startFight
			};

			temp = emotions.GetRandom();
			preachInput = "Through " + godOfChoice + " we are more in tune with our " + temp + ".";
			preachStarters.Add(preachInput);
			ResponseData preachResponseExample2 = new ResponseData {
				inputToRespondTo = preachInput,
				actorAssetID = null,
				actorReply = new List<string> { "I feel my " + temp + " surging!" },
				//responseAction = recruitIntoCult();
			};

			preachStarters.Add("The key to salvation is " + godOfChoice + "'s " + emotions.GetRandom() + "!");
			preachStarters.Add("Ask yourself, what would " + godOfChoice + " do" + "?");
			preachStarters.Add(godOfChoice + " will bring a new world!");

			actorCultistResponses.Add(preachResponseExample1.inputToRespondTo, preachResponseExample1);
			actorCultistResponses.Add(preachResponseExample2.inputToRespondTo, preachResponseExample2);
		}

		//key: inputToRespondTo, value: response data
		public static Dictionary<string, ResponseData> actorCultistResponses = new Dictionary<string, ResponseData>();

		public class BehFindPreachTile : BehaviourActionActor {
			public override void create()
			{
				base.create();
			}

			public override BehResult execute(Actor pActor)
			{
				if(pActor.city == null) {
					//actor isnt in a city, cant preach to animals! (why not?)
					return BehResult.Stop;
				}
				WorldTile cityCenter = pActor.city.getTile(); 
				// is gettile center??
				//offset position a bit, and find random spot in a line (so they walk side to side randomly?)
				//vertical offset high enough to allow space for a crowd in the center?
				int verticalOffset = (int)Toolbox.randomFloat(3f, 5f);
				if(Toolbox.randomBool()) {
					//random chance to go negative/bottom side of center
					verticalOffset = -verticalOffset;
				}
				int horizontalOffset = (int)Toolbox.randomFloat(-5f, 5f);
				WorldTile offsetTile = MapBox.instance.GetTile(cityCenter.pos.x + horizontalOffset, cityCenter.pos.y + verticalOffset);
				if(offsetTile.zone.city == pActor.city) {
					//tile confirmed inside city
					pActor.beh_tile_target = offsetTile;
				}
				return BehResult.Continue;
			}
		}

		public class BehPreach : BehaviourActionActor {
			public override void create()
			{
				base.create();
			}

			public override BehResult execute(Actor pActor)
			{
				SimpleMessages.Messages.ActorSay(pActor, preachStarters.GetRandom(), 3f);
				return BehResult.Continue;
			}
		}

		//response patch for cultists
		//happens instantly, need a way to delay it
		public static void ActorSay_Postfix(Actor targetActor, string messageText, float duration)
		{
			//check if actor speaking is cult leader, could use some other check
			if(cultsDict.ContainsKey(targetActor)) {
				//cult leader detected, add reactions from nearby actors?
				Debug.Log("leader spoke, checking responses");
				//check if spoken message has responses to use
				if(actorCultistResponses.ContainsKey(messageText)) {
					Debug.Log("response found");
					//target city unit list
					List<Actor> targetActors = targetActor.city.units.getSimpleList();
					foreach(Actor actor in targetActors) {
						//random chance (30%)
						if(Toolbox.randomFloat(0, 1f) > 0.7f) {
							Debug.Log("random chance passed");
							//distance check to make sure responses arent from other side of city
							if(Toolbox.DistTile(targetActor.currentTile, actor.currentTile) < 10f) {
								//check if this targetActor is already in cult
								if(cultsDict[targetActor].Contains(actor)) {
									string reply = actorCultistResponses[messageText].actorReply.GetRandom();
									SimpleMessages.Messages.ActorSay(actor, reply, 3f);
								}
								//if theyre not, random chance to add to cult for next time
								else {
									if(actor != targetActor) {
										if(Toolbox.randomFloat(0, 1f) > 0.3f) {
											//prevent leader joining his own cult
											cultsDict[targetActor].Add(actor);
											Debug.Log(actor.getName() + "has joined " + targetActor.getName() + "'s cult!");
										}
									}
								}
							}
						}
					}
				}
				else {
					Debug.Log("response not found");
				}
			}
		}

		public class CondEnoughFollowers : BehaviourActorCondition {
			public override bool check(Actor pActor)
			{
				if(cultsDict.ContainsKey(pActor)) {
					int followerCount = cultsDict[pActor].Count;
					if(followerCount > 10) {
						return true;
					}
				}
				return false;
			}
		}

		public string godOfChoice;
		public List<string> emotions = new List<string>() { "anger", "bloodshed", "insanity", "rage", "fury", "wrath", "hate", "love", "joy" };
		public static List<string> preachStarters = new List<string>();
		public static Dictionary<Actor, List<Actor>> cultsDict = new Dictionary<Actor, List<Actor>>();


		//assign job using trait
		//doesnt work, nextJobActor only runs once, when actor is created
		public static void nextJobActor_postfix(ActorBase pActor, ref string __result)
		{
			if(pActor.hasTrait("cultLeader")) {
				__result = "cultLeader";
			}
		}
	}
}
