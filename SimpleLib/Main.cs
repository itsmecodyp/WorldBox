using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using SimpleMessages;
using System.Reflection;
using EpPathFinding.cs;

namespace SimpleLib
{
    // A collection of features and tools that other mods of mine use
    // Might be helpful for other modders
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.simple.lib";
        public const string pluginName = "SimpleLib";
        public const string pluginVersion = "0.0.0.2";

        public void Update()
        {
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
        }

        public void OnGUI()
        {

        } 
        /*
        public void Awake()
        {
            Test();
        }

        public async void Test()
		{
            OpenAIAPI api = new OpenAIAPI("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", Engine.Davinci);
            //var api = new OpenAI_API.OpenAIAPI(new APIAuthentication("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"), engine: Engine.Davinci);

            var result = await api.Completions.CreateCompletionAsync("Hello there ", temperature: 0.7);
            Debug.Log("@@@: " + result.ToString());
        }
        */
        public static void ActorSay(Actor targetActor, string messageText, float duration)
        {
            Messages.ActorSay(targetActor, messageText, duration);
}
        public static void ActorSay(Actor targetActor, string messageText, string titleText, float duration)
        {
            Messages.ActorSay(targetActor, messageText, titleText, duration);
        }

        public float lastTimer = 0f;

    }


}

namespace SimpleMessages
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Messages : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.simple.messages";
        public const string pluginName = "SimpleMessages";
        public const string pluginVersion = "0.0.0.1";
        public static int windowInUse = 0;
        public static List<ModMessage> listOfMessages = new List<ModMessage>();
        public static int messageID = 0;

        public static void ActorSay(Actor targetActor, string messageText, float duration = 3f)
        {
            ModMessage newMessage = new ModMessage();
            newMessage.id = messageID; // id starts at 0, random start addition to make sure no conflict with other mod menu ids
            messageID++; // and increments upwards each time
            newMessage.assignedActor = targetActor;
            newMessage.duration = duration;
            newMessage.startTime = Time.realtimeSinceStartup;
            newMessage.MessageText = messageText;
            listOfMessages.Add(newMessage);
        }

        public static void ActorSay(Actor targetActor, string titleText, string messageText, float duration = 3f)
        {
            ModMessage newMessage = new ModMessage();
            newMessage.id = messageID; // id starts at 0, random start addition to make sure no conflict with other mod menu ids
            messageID++; // and increments upwards each time
            newMessage.assignedActor = targetActor;
            newMessage.duration = 3f;
            newMessage.startTime = Time.realtimeSinceStartup;
            newMessage.MessageText = messageText;
            newMessage.TitleText = titleText;
            listOfMessages.Add(newMessage);
        }

        public void OnGUI()
        {
            if (listOfMessages.Count >= 1)
            {
                for (int i = 0; i < listOfMessages.Count; i++)
                {
                    ModMessage activeMessage = listOfMessages[i];
                    if (activeMessage.startTime + activeMessage.duration > Time.realtimeSinceStartup)
                    {
                        Actor actor = activeMessage.assignedActor;
                        ActorStatus data = null;
                        if (actor != null)
                        data = Reflection.GetField(actor.GetType(), actor, "data") as ActorStatus;
                        Vector3 screenPos = new Vector3();
                        if (data != null && data.alive)
                        {
                            SpriteRenderer spriteRendererBody = Reflection.GetField(actor.GetType(), actor, "spriteRenderer") as SpriteRenderer;
                            Vector3 pos = new Vector3();
                            pos.x = spriteRendererBody.transform.localPosition.x;
                            pos.y = spriteRendererBody.transform.localPosition.y;
                            pos.z = spriteRendererBody.transform.localPosition.z;
                            screenPos = Camera.main.WorldToScreenPoint(pos);
                            screenPos.y = (Screen.height - screenPos.y);
                            // adding a random number (3536) to make sure theres no conflict with window id in other mods
                            Rect window = GUILayout.Window(activeMessage.id + 3536, new Rect(new Vector2(screenPos.x - 100, screenPos.y - 115), new Vector2()), new GUI.WindowFunction(ActorMessageDisplayWindow), activeMessage.TitleText, new GUILayoutOption[] { GUILayout.MaxWidth(activeMessage.MessageText.Length), GUILayout.MinWidth(200f) });
                            //window.width = 5f;
                        }
                    }
                    else
                    {
                        //listOfMessages.Remove(activeMessage);
                        // i feel like removing them saves performance
                        // but it causes an error..
                    }

                }
            }
           
        }

        public void Update()
        {
           
        }

        public void ActorMessageDisplayWindow(int windowID)
        {
            ModMessage activeMessage = listOfMessages[windowID - 3536]; // removing the conflict number
            if (activeMessage != null)
            {
                GUILayout.Label(activeMessage.MessageText);
            }
            GUI.DragWindow();
        }
    }

    [Serializable]
    public class ModMessage
    {
        public int id = 0;
        public Actor assignedActor;
        public string TitleText = "";
        public string MessageText = "";
        public float duration = 3.0f;
        public float startTime;
    }

    public static class Reflection
    {
        // found on https://stackoverflow.com/questions/135443/how-do-i-use-reflection-to-invoke-a-private-method
        public static object CallMethod(this object o, string methodName, params object[] args)
        {
            var mi = o.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mi != null)
            {
                return mi.Invoke(o, args);
            }
            return null;
        }
        // found on: https://stackoverflow.com/questions/3303126/how-to-get-the-value-of-private-field-in-c/3303182
        public static object GetField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
        public static void SetField<T>(object originalObject, string fieldName, T newValue)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = originalObject.GetType().GetField(fieldName, bindFlags);
            field.SetValue(originalObject, newValue);
        }
    }

}

namespace SimpleTraits
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Traits : BaseUnityPlugin
    {
        public const string pluginGuid = "cody.worldbox.simple.traits";
        public const string pluginName = "SimpleTraits";
        public const string pluginVersion = "0.0.0.1";
    }
}
