using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using HarmonyLib;
using System.Linq;
using BepInEx.Configuration;

namespace GeneralPurposeOnlineServer
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class OnlineMain : BaseUnityPlugin
    {
        public static List<string> commands = new List<string> {
            "quit",
            "april fools",
            "quit",
            "random tile",
            "map",
            "lightning",
            "dragon",
            "help",
            "mouse",
            "setSeed",
            "loadMap"
        };
        public void SendNewTiletype(string tileType, int connection)
        {

        }
        // mitm
        public void MSendMessage(int hostId, int connectionId, int channelId, byte[] buffer, int size, out byte error)
        {

            NetworkTransport.Send(hostId, connectionId, channelId, buffer, size, out error);
            NetworkError networkError = (NetworkError)error;
            if (networkError != NetworkError.Ok)
            {
                Debug.LogError(string.Format("Error: {0}, hostId: {1}, connectionId: {2}, channelId: {3}", networkError, hostId, connectionId, channelId));
            }
        }
        public bool showHideOnline;
        public Rect onlineWindowRect;
        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 120, 100, 120, 30));
            if (GUILayout.Button("Online"))
            {
                showHideOnline = !showHideOnline;
            }
            if (showHideOnline)
            {
                onlineWindowRect = GUILayout.Window(50000, onlineWindowRect, new GUI.WindowFunction(onlineWindow), "Online", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }
            GUILayout.EndArea();


        }

        public void Update()
        {
            if ((!isServer && !isClient))
            {
                if (server.port < 1023)
                {
                    server.port = 1023;
                } // apparently these are reserved and app would need admin
                if (server.port > 65535)
                {
                    server.port = 65535;
                }
            }
            if (isServer)
            {
               
            }
            if (isClient)
            {
                client.SendActionsPacket();
            }
            NetworkReceivingStuff();
        }
        public void onlineWindow(int windowID)
        {
            if ((!isServer && !isClient))
            {
                if (!isTestBuild)
                {
                    if (GUILayout.Button("Start server"))
                    {
                        server.startServer();
                        isServer = true;
                        statePatch();
                    }
                }
                if (GUILayout.Button("Client connect"))
                {
                    client.Connect();
                }
                if (!isTestBuild)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Button("IP(client)");
                    client.host = GUILayout.TextField(client.host);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Button("Port");
                    try
                    {
                        server.port = int.Parse(GUILayout.TextField(server.port.ToString()));
                    }
                    catch (Exception error)
                    {
                        Debug.Log("server port error, setting to default (8000)");
                        server.port = 8000;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            if (isClient)
            {
                if (GUILayout.Button("Disconnect"))
                {
                    client.Disconnect();
                    isClient = false;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Button("Identifier");
                client.clientIdentifier = GUILayout.TextField(client.clientIdentifier);
                GUILayout.EndHorizontal();

                //message/command
                if (GUILayout.Button("Send message from client"))
                {
                    client.ClientSendMessage(messageToSend);
                    messageToSend = "";
                }
                messageToSend = GUILayout.TextField(messageToSend);

            }
            if (isServer)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Button("Active:");
                GUILayout.Button(activeClientConnections.Count.ToString() + "/" + server.maxConnections.ToString());
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Button("Identifier");
                server.serverIdentifier = GUILayout.TextField(server.serverIdentifier);
                GUILayout.EndHorizontal();
                //message/command
                if (GUILayout.Button("Send message from server"))
                {
                    foreach (int activeConnection in activeConnections)
                    {
                        server.SendSocketMessageServer(messageToSend, activeConnection);
                    }
                }
                messageToSend = GUILayout.TextField(messageToSend);
            }
            GUI.DragWindow();
        }
        public string messageToSend = "";
        public void Awake()
        {
            instance = this;
            NetworkTransport.Init();
            connectToIP = Config.AddSetting("Online", "Target IP", "defaultIP", "What address the client connects to");
            client.host = connectToIP.Value;
        }
        public bool hasInit;
        public List<int> activeClientConnections = new List<int>();
        void NetworkReceivingStuff()
        {
            int outHostId;
            int outConnectionId;
            int outChannelId;

            int receivedSize;
            byte error;
            byte[] buffer = new byte[1024];
            NetworkEventType evt = NetworkTransport.Receive(out outHostId, out outConnectionId, out outChannelId, buffer, buffer.Length, out receivedSize, out error);

            switch (evt)
            {
                case NetworkEventType.ConnectEvent:
                    {
                        OnConnect(outHostId, outConnectionId, (NetworkError)error);
                        if (outConnectionId != 0)
                        {
                            activeClientConnections.Add(outConnectionId);
                        }
                        break;
                    }
                case NetworkEventType.DisconnectEvent:
                    {
                        OnDisconnect(outHostId, outConnectionId, (NetworkError)error);
                        if (activeClientConnections.Contains(outConnectionId))
                        {
                            activeClientConnections.Remove(outConnectionId);
                        }
                        break;
                    }
                case NetworkEventType.DataEvent:
                    {
                        OnData(outHostId, outConnectionId, outChannelId, buffer, receivedSize, (NetworkError)error);
                        break;
                    }
                case NetworkEventType.BroadcastEvent:
                    {
                        OnBroadcast(outHostId, buffer, receivedSize, (NetworkError)error);
                        break;
                    }
                case NetworkEventType.Nothing:
                    break;

                default:
                    Debug.LogError("Unknown network message type received: " + evt);
                    break;
            }
        }
        public List<int> activeConnections = new List<int>(); // shows connections for both client and host
        void OnConnect(int hostId, int connectionId, NetworkError error)
        {
            // Debug.Log("OnConnect(hostId = " + hostId + ", connectionId = " + connectionId + ", error = " + error.ToString() + ")");
            if (!activeConnections.Contains(connectionId))
            {
                activeConnections.Add(connectionId);
            }
            if (isServer)
            {
                server.SendSocketMessageServer("setSeed(" + serverSeed.ToString() + ")", connectionId);

            }
        }
        public int serverSeed = 55;
        void OnDisconnect(int hostId, int connectionId, NetworkError error)
        {
            if (error == NetworkError.Timeout)
            {
                Debug.Log("Timed out, attempting auto-reconnect");
                //isClient = false; // should display message later
                this.client.Connect();
            }
            // Debug.Log("OnDisconnect(hostId = " + hostId + ", connectionId = " + connectionId + ", error = " + error.ToString() + ")");
            if (activeConnections.Contains(connectionId))
            {
                activeConnections.Remove(connectionId);
            }
        }
        Vector3 blankPos = Vector3.one;
        void OnBroadcast(int hostId, byte[] data, int size, NetworkError error)
        {
            // Debug.Log("OnBroadcast(hostId = " + hostId + ", data = " + data + ", size = " + size + ", error = " + error.ToString() + ")");
        }
        void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, NetworkError error)
        {
            // this is run when any data is received from connections, whether this instance is a client or server
            //Debug.Log("OnData(hostId = " + hostId + ", connectionId = "+ connectionId + ", channelId = " + channelId + ", data = "+ data + ", size = " + size + ", error = " + error.ToString() + ")");
            Stream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();
            object receivedData = formatter.Deserialize(stream);

            string message = "";
            if (receivedData.GetType() == data.GetType())
            {
                if (waitingForMapPacket) // send starting data
                {
                    waitingForMapPacket = false; 
                }
            }
            if (receivedData.GetType() == message.GetType())
            {
                message = receivedData as string;
            }
            if (message != null)
            {
                string identifier = "null";
                if (message.Contains(":"))
                {
                    string[] split = message.Split(new[] { ":" }, StringSplitOptions.None);
                    identifier = split[0];
                    message = split[1];
                }
                string originalMessage = message;
                if (identifier == "null")
                {
                    if (isClient && OnlineMain.instance.clientReference.clientIdentifier != null)
                    {
                        identifier = OnlineMain.instance.clientReference.clientIdentifier;
                    }
                    else
                    {
                        identifier = "connectionID " + connectionId.ToString();
                    }
                }
                if (isClient && message == "quit") // // isClient when message is received, meaning sent from a client or host
                {
                    Application.Quit(1);
                }
                Debug.Log("Incoming message received from " + identifier + ": " + message); // move this below to hide commands from client
                if (isServer) // isServer when message is received, meaning sent from a client
                {
                    if (message == "quit")
                    {
                        Debug.Log("Application.Quit not running, command received from client");
                    }
                }
                string command;
                string param;
                if (message.Contains('('))
                {
                    if (message.Contains(')'))
                    {
                        param = message.Split('(', ')')[1];
                        command = message.Replace("(" + param + ")", "");
                        ParseCommand(command, param, connectionId, identifier);
                    }
                }
                else
                {
                    command = message;
                    ParseCommand(command, "", connectionId, identifier);
                }

                // relay to all active connections so the host is the only one receiving the direct connections
                // lots of advantage to doing this way
                if (isServer && activeClientConnections.Count > 1)
                {
                    server.SendRelayedMessage(identifier + ":" + originalMessage, connectionId);
                }
            }
        }
        public bool waitingForMapPacket;

        public void ParseCommand(string command, string param, int connectionId /*sender*/, string identifier = null) // another mod could patch this method
        {
            if (commands.Contains(command) == false)
            {
                Debug.Log("Command not in dict");
            }
            try
            {
                if (command == "setSeed" && param != null)
                {
                    int.TryParse(param, out int newSeed);
                    clientSeed = newSeed;
                    statePatch();
                }
                if (command == "help" && isServer)
                {
                    DisplayHelp();
                }
                if (isClient && command == "setID")
                {
                    if (param != null)
                    {
                        if (param == "random")
                        {
                            param = UnityEngine.Random.Range(60000f, 6022220f).ToString();
                        }
                        client.ClientSendMessage(client.clientIdentifier + " changing to " + param);
                        client.clientIdentifier = param;
                    }
                    else
                    {
                        client.ClientSendMessage(client.clientIdentifier);
                    }
                }
                if (command == "ping")
                {
                    client.ClientSendMessage("pong");
                }
            }
            catch (Exception e)
            {
                Debug.Log("Command error: " + e.ToString());
            }
            // Debug.Log("finished: " + command + ", param: " + param);
        }

        public void DisplayHelp()
        {
            string outputMessage = "Commands; ";
            foreach (string commandInList in commands)
            {
                outputMessage += commandInList + ", ";
            }
            Debug.Log(outputMessage);
        }

        public static Vector3 StringToVector3(string sVector)
        {
            // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")"))
            {
                sVector = sVector.Substring(1, sVector.Length - 2);
            }

            // split the items
            string[] sArray = sVector.Split(',');

            // store as a Vector3
            Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

            return result;
        }

        public static void InitState_Prefix(ref int seed)
        {
            seed = clientSeed;
        }

        public void statePatch()
        {
            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo original = AccessTools.Method(typeof(UnityEngine.Random), "InitState");
            MethodInfo patch = AccessTools.Method(typeof(OnlineMain), "InitState_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("Pre patch: UnityEngine.Random.InitState");
        }

        public static ConfigEntry<string> connectToIP
        {
            get; set;
        }
        public static OnlineMain instance;
        public const string pluginGuid = "cody.general.online";
        public const string pluginName = "Bepinex Online";
        public const string pluginVersion = "0.0.0.1";

        public List<string> messageHistory = new List<string>();

        NetworkServer server = new NetworkServer();
        NetworkClient client = new NetworkClient();
        public NetworkClient clientReference => client;
        public NetworkServer serverReference => server;

        public bool isServer;
        public bool isClient;

        static int clientSeed = 55; // default seed

        public bool isTestBuild = false; // for publicTest, only allows connecting to one ip
        //eoc
    }

}