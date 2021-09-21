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
using Assets.SimpleZip;
using BepInEx.Configuration;

namespace WorldBoxOnline
{
    //  Tooltip.instance.show(this.powerButton.gameObject, "tip", null, null);
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
        public string CurrentMapAsString()
        {
            return MapBox.instance.saveManager.currentWorldToSavedMap().toJson();
        }
        public byte[] CurrentMapAsBytes()
        {
            return Zip.Compress(CurrentMapAsString());
        }
        public void loadMapFromString(string input)
        {
            SavedMap map = JsonUtility.FromJson<SavedMap>(input);
            map.worldLaws.check();
            MapBox.instance.saveManager.loadData(map);
        }
        public void loadMapFromBytes(byte[] input)
        {
            SavedMap map = JsonUtility.FromJson<SavedMap>(Zip.Decompress(input));
            map.worldLaws.check();
            MapBox.instance.saveManager.loadData(map);
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
                checkPower();
                if (server.sendMousePosition)
                {
                    server.SendMousePosition();
                    if (Input.GetMouseButtonDown(0)) // or mouse or whatever action key
                    {
                        foreach (int activeConnection in activeConnections)
                        {
                            server.SendSocketMessageServer("mouse0Down", activeConnection); // action named mouse for now
                        }
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        foreach (int activeConnection in activeConnections)
                        {
                            server.SendSocketMessageServer("mouse0Up", activeConnection);
                        }
                    }
                }
            }
            if (isClient)
            {
                checkPower();
                client.SendActionsPacket();
                if (client.sendMousePosition)
                {
                    client.SendMouseToHost();
                    if (Input.GetMouseButtonDown(0)) 
                    {
                        client.AddAction("mouse0Down");
                    }
                    else if (Input.GetMouseButtonUp(0)) 
                    {
                        client.AddAction("mouse0Up");
                    }
                }
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
               
                if (GUILayout.Button("send mouse pos:" + client.sendMousePosition.ToString()))
                {
                    client.sendMousePosition = !client.sendMousePosition;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Button("Identifier");
                client.clientIdentifier = GUILayout.TextField(client.clientIdentifier);
                GUILayout.EndHorizontal();
                //message/command
                if (GUILayout.Button("Send message from client"))
                {
                    client.AddAction(messageToSend);
                    messageToSend = "";
                }
                messageToSend = GUILayout.TextField(messageToSend);
            }
            if (isServer)
            {
                if (GUILayout.Button("send mouse pos:" + server.sendMousePosition.ToString()))
                {
                    server.sendMousePosition = !server.sendMousePosition;
                }
                //if (GUILayout.Button("Shutdown server"))

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
            Debug.Log("OnConnect(hostId = " + hostId + ", connectionId = "
                + connectionId + ", error = " + error.ToString() + ")");
            if (!activeConnections.Contains(connectionId))
            {
                activeConnections.Add(connectionId);
            }
            if (isServer)
            {
                if (serverActivePower != null)
                {
                    server.SendSocketMessageServer("setPower(" + serverActivePower.name + ")", connectionId);
                }
                server.SendSocketMessageServer("setSeed(" + serverSeed.ToString() + ")", connectionId);

            }
        }
        public int serverSeed = 55;
        public GodPower serverActivePower;
        void OnDisconnect(int hostId, int connectionId, NetworkError error)
        {
            if (error == NetworkError.Timeout)
            {
                Debug.Log("Timed out, attempting auto-reconnect");
                //isClient = false; // should display message later
                this.client.Connect();
            }
            Debug.Log("OnDisconnect(hostId = " + hostId + ", connectionId = "
                + connectionId + ", error = " + error.ToString() + ")");
            if (activeConnections.Contains(connectionId))
            {
                activeConnections.Remove(connectionId);
            }
        }
        Vector3 blankPos = Vector3.one;
        void OnBroadcast(int hostId, byte[] data, int size, NetworkError error)
        {
            Debug.Log("OnBroadcast(hostId = " + hostId + ", data = "
                + data + ", size = " + size + ", error = " + error.ToString() + ")");
        }
        void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, NetworkError error)
        {
            //Debug.Log("OnData(hostId = " + hostId + ", connectionId = "+ connectionId + ", channelId = " + channelId + ", data = "+ data + ", size = " + size + ", error = " + error.ToString() + ")");
            Stream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();
            object receivedData = formatter.Deserialize(stream);

            string message = "";
            if (receivedData.GetType() == data.GetType())
            {
                if (waitingForMapPacket)
                {
                    loadMapFromBytes(receivedData as byte[]);
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
                    identifier = message.Split(new[] { ":" }, StringSplitOptions.None)[0];
                    message = message.Split(new[] { ":" }, StringSplitOptions.None)[1];
                }
                string originalMessage = message;
                if (identifier == "null")
                {
                    identifier = "connectionID " + connectionId.ToString();
                }
                if (isClient && message == "quit") // // isClient when message is received, meaning sent from a client or host
                {
                    Application.Quit(1);
                }
                //Tooltip.instance.show(base.gameObject, "normal", "From: " + identifier, message);
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
                        ParseCommand(command, param, connectionId);
                    }
                }
                else
                {
                    command = message;
                    ParseCommand(command, "", connectionId);
                }
                if (isServer && activeClientConnections.Count > 1)
                {
                    server.SendRelayedMessage(originalMessage, connectionId);
                }
            }
        }
        public bool waitingForMapPacket;
        public string currentPower = "";
        public string currentTile = "";
        public void checkPower() // check if a GodPower is selected and if so, get ready to sync its action
        {
            if (MapBox.instance.selectedButtons.isPowerSelected())
            {
                PowerButton selectedButton = Reflection.GetField(MapBox.instance.selectedButtons.GetType(), MapBox.instance.selectedButtons, "selectedButton") as PowerButton;
                GodPower godPower = Reflection.GetField(selectedButton.GetType(), selectedButton, "godPower") as GodPower;
                if (currentPower != godPower.id)
                {
                    currentPower = godPower.id;
                    if (TileType.get(godPower.tileType) != null) // set tiles for drawing
                    {
                        currentTile = godPower.tileType;
                    }
                    if (isClient)
                    {
                        client.AddAction("setPower", currentPower);
                    }
                }
              
            }
        }
        public Dictionary<int, string> connectionsPowerEquipped = new Dictionary<int, string>();
        public Dictionary<int, string> connectionsClickStatus = new Dictionary<int, string>();

        public void ParseCommand(string command, string param, int connectionId /*sender*/)
        {
            if (commands.Contains(command) == false)
            {
                //Debug.Log("Command not in dict");
            }
            try
            {
                #region WorldBox
                if (command == "setSeed" && param != null)
                {
                    int.TryParse(param, out int newSeed);
                    clientSeed = newSeed;
                    statePatch();
                }
                if (command == "loadMap")
                {
                    waitingForMapPacket = true;
                    // split up giant map string and send it in parts?
                }
                if (command == "setAction" && param != null)
                {
                    currentAction = param;
                }
                if (command == "april fools")
                {
                    SaveManager.loadMapFromResources("mapTemplates/maphash");
                }
                if (command == "tileTest")
                {
                    WorldTile targetTile = MapBox.instance.tilesList.GetRandom();
                    foreach (Actor creature in MapBox.instance.units) // not backwards compatible from 8.0 update
                    {
                        creature.cancelAllBeh();
                        creature.moveTo(targetTile);
                    }
                }
                if (command == "map")
                {
                    int.TryParse(param, out int newSize);
                    MapBox.instance.setMapSize(newSize, newSize);
                    MapBox.instance.CallMethod("GenerateMap", new object[] { "custom" });
                    MapBox.instance.finishMakingWorld();
                }
                if (command == "earth")
                {
                    MapBox.instance.CallMethod("GenerateMap", new object[] { "earth" });
                    MapBox.instance.finishMakingWorld();
                }
                if (command == "help")
                {
                    DisplayHelp();
                }
                if (command == "mouse")
                {
                    Vector3 mousePos = StringToVector3(param); // parse remainder which was Vector3.ToString() converted before
                    WorldTile mouseTile = MapBox.instance.GetTile((int)mousePos.x, (int)mousePos.y);
                    PixelFlashEffects flashEffects = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "flashEffects") as PixelFlashEffects;
                    MapBox.instance.CallMethod("highlightFrom", new object[] { mouseTile, "circ_5" });
                    lastReceivedMousePos.x = (int)mousePos.x;
                    lastReceivedMousePos.y = (int)mousePos.y;
                    if (connectionsClickStatus.ContainsKey(connectionId) && connectionsClickStatus[connectionId] == "down") /*&& connectionsPowerEquipped[connectionId].Contains("tile")*/
                    {
                            UsePower(MapBox.instance.GetTile(lastReceivedMousePos.x, lastReceivedMousePos.y), connectionsPowerEquipped[connectionId]);
                    }
                }
                if (command == "mouse0Down")
                {
                    if (connectionsClickStatus.ContainsKey(connectionId) == false)
                    {
                        connectionsClickStatus.Add(connectionId, "down");
                    }
                    else
                    {
                        connectionsClickStatus[connectionId] = "down";
                    }
                }
                if (command == "mouse0Up")
                {
                    if (connectionsClickStatus.ContainsKey(connectionId) == false)
                    {
                        connectionsClickStatus.Add(connectionId, "up");
                    }
                    else
                    {
                        connectionsClickStatus[connectionId] = "up";
                    }
                    UsePower(MapBox.instance.GetTile(lastReceivedMousePos.x, lastReceivedMousePos.y), connectionsPowerEquipped[connectionId]);
                }
                if (command == "setPower")
                {
                    if (connectionsPowerEquipped.ContainsKey(connectionId) == false)
                    {
                        connectionsPowerEquipped.Add(connectionId, param);
                    }
                    else
                    {
                        connectionsPowerEquipped[connectionId] = param;
                    }
                    if (isServer)
                    {
                        server.SendRelayedMessage(command, connectionId); // for some reason everything else is relayed properly except this
                    }
                }
                #endregion WorldBox
                if (isClient && command == "setID")
                {
                    if (param != null)
                    {
                        if (param == "random")
                        {
                            param = UnityEngine.Random.Range(60000f, 6022220f).ToString();
                        }
                        client.ClientSendReply(client.clientIdentifier + " changing to " + param);
                        client.clientIdentifier = param;
                    }
                    else
                    {
                        client.ClientSendReply(client.clientIdentifier);
                    }
                }
                if (command == "game")
                {
                    client.ClientSendReply("Product:" + Application.productName + "; \n Installer:" + Application.installerName);
                }
                if (command == "mouse0Down")
                {
                    isMouseDown = true;
                }
                if (command == "mouse0Up")
                {
                    isMouseDown = false;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Command error: " + e.ToString());
            }
            Debug.Log("finished: " + command + ", param: " + param);
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
        public static bool GetSpritePixelColorUnderMousePointer_Prefix(MonoBehaviour mono, out Vector2Int pVector)
        {
            pVector = new Vector2Int(-1, -1);
            if (OnlineMain.instance.isMouseDown)
            {
                pVector = OnlineMain.instance.lastReceivedMousePos;
            }
            return true;
        }
        public void statePatch()
        {
            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo original = AccessTools.Method(typeof(UnityEngine.Random), "InitState");
            MethodInfo patch = AccessTools.Method(typeof(OnlineMain), "InitState_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
            Debug.Log("Pre patch: UnityEngine.Random.InitState");
        }
        
        public void UsePower(WorldTile targetTile, string power)
        {
            GodPower chosenPower = AssetManager.powers.get(power);
            if (chosenPower != null)
            {
                // this needs turned into a switch statement.. lol
                if (chosenPower.actionType == PowerActionType.SpawnActor)
                {
                    Sfx.play("spawn", true, -1f, -1f);
                    if (chosenPower.spawnSound != "")
                    {
                        Sfx.play(chosenPower.spawnSound, true, -1f, -1f);
                    }
                    if (chosenPower.showSpawnEffect != string.Empty)
                    {
                        MapBox.instance.stackEffects.CallMethod("startSpawnEffect", new object[] { targetTile, chosenPower.showSpawnEffect });
                    }
                    string pStatsID;
                    if (chosenPower.actorStatsId.Contains(","))
                    {
                        pStatsID = Toolbox.getRandom<string>(chosenPower.actorStatsId.Split(new char[]
                        {
                    ','
                        }));
                    }
                    else
                    {
                        pStatsID = chosenPower.actorStatsId;
                    }
                    Actor actor = MapBox.instance.createNewUnit(pStatsID, targetTile, "", chosenPower.actorSpawnHeight, null);
                    if (actor.stats.unit)
                    {
                        ActorStatus data = Reflection.GetField(actor.GetType(), actor, "data") as ActorStatus;
                        data.age = 18;
                        actor.CallMethod("setKingdom", new object[] { MapBox.instance.kingdoms.dict_hidden["nomads_" + actor.stats.race] });
                        return;
                    }
                }
                else if (chosenPower.actionType == PowerActionType.Special)
                {
                    if (chosenPower.id == "cloudRain" || chosenPower.id == "cloudAcid" || chosenPower.id == "cloudLava" || chosenPower.id == "cloudSnow")
                    {
                        MapBox.instance.cloudController.CallMethod("spawnCloud", new object[] { targetTile.posV3, chosenPower.id });
                    }
                }
                else if (chosenPower.actionType == PowerActionType.Tile)
                {
                    MapBox.instance.drawWithBrush(targetTile.pos, Brush.get("circ_5"), 1, "tile", chosenPower.tileType, chosenPower, true);
                }
                else if (chosenPower.actionType == PowerActionType.Draw)
                {
                    MapBox.instance.drawWithBrush(targetTile.pos, Brush.get("circ_5"), 1, chosenPower.id, null, chosenPower, true);
                }
                else if (chosenPower.id == "lightning")
                {
                    MapBox.spawnLightning(targetTile, 0.25f);
                }
                else if (chosenPower.id == "magnet")
                {
                    MapBox.instance.actions.CallMethod("magnetAction", new object[] { false, targetTile });
                }
                else if (chosenPower.id == "divineLight")
                {
                    MapBox.instance.drawWithBrush(targetTile.pos, Brush.get("circ_5"), 1, "divineLight", null, chosenPower, true);
                    MapBox.instance.fxDivineLight.CallMethod("playOn", new object[] { targetTile });
                }
                else if (chosenPower.id == "temperaturePlus")
                {
                    Sfx.play("temperatureU", true, -1f, -1f);
                    MapBox.instance.drawWithBrush(targetTile.pos, Brush.get("circ_5"), 1, "temperaturePlus", null, chosenPower, true);
                }
                else if (chosenPower.id == "temperatureMinus")
                {
                    Sfx.play("temperatureD", true, -1f, -1f);
                    MapBox.instance.drawWithBrush(targetTile.pos, Brush.get("circ_5"), 1, "temperatureMinus", null, chosenPower, true);
                }
                else if (chosenPower.id == "heatray")
                {
                    Sfx.play("temperatureU", true, -1f, -1f);
                    MapBox.instance.heatRayFx.CallMethod("play", new object[] { targetTile.pos, 10 });
                }
                else if (chosenPower.id == "rainfallPlus")
                {
                    MapBox.instance.drawWithBrush(targetTile.pos, Brush.get("circ_5"), 1, "rainfall", null, chosenPower, true);
                }
                else if (chosenPower.id == "rainfallMinus")
                {
                    MapBox.instance.drawWithBrush(targetTile.pos, Brush.get("circ_5"), 1, "rainfall", null, chosenPower, true);
                }
                else
                {
                    MapBox.instance.drawWithBrush(targetTile.pos, Brush.get("circ_5"), 1, "draw", null, chosenPower, true);
                }
            }

        }
        public static ConfigEntry<string> connectToIP
        {
            get; set;
        }
        public static OnlineMain instance;
        public const string pluginGuid = "cody.worldbox.online";
        public const string pluginName = "WorldBox Online";
        public const string pluginVersion = "0.0.0.1";

        public List<string> messageHistory = new List<string>();

        NetworkServer server = new NetworkServer();
        NetworkClient client = new NetworkClient();
        public NetworkClient clientReference => client;
        public NetworkServer serverReference => server;

        public bool isServer;
        public bool isClient;

        public string currentAction = "";
        public bool isMouseDown;
        public Vector2Int lastReceivedMousePos;
        static int clientSeed = 55; // default seed

        public bool isTestBuild = false; // for publicTest
        //eoc
    }

    public class NetworkServer : MonoBehaviour
    {
        GlobalConfig gconfig;
        public int port = 8000;
        public HostTopology topology;
        public ConnectionConfig config;
        public int connectionId;
        public int hostId;
        public byte channelId;
        public byte _channelUnreliable;
        public int maxConnections = 10;
        public string serverIdentifier = "host";
        public WorldTile lastMouseTile;

        void Start()
        {
          
        }
        void Update()
        {
            int recHostId;
            int recConnectionId;
            int recChannelId;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            byte error;
            NetworkEventType networkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);
            NetworkError networkError = (NetworkError)error;
            if (networkError != NetworkError.Ok)
            {
                Debug.LogError(string.Format("Error recieving event: {0} with recHostId: {1}, recConnectionId: {2}, recChannelId: {3}", networkError, recHostId, recConnectionId, recChannelId));
            }
            switch (networkEvent)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    Debug.Log(string.Format("incoming connection event received with connectionId: {0}, recHostId: {1}, recChannelId: {2}", recConnectionId, recHostId, recChannelId));
                    //do something on connection, ie request something
                    break;
                case NetworkEventType.DataEvent:
                    Stream stream = new MemoryStream(recBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(stream) as string;
                    Debug.Log("server: incoming message event received: " + message);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("remote client " + recConnectionId + " disconnected");
                    break;
            }
        }
        public void startServer()
        {
            gconfig = new GlobalConfig();
            gconfig.ReactorModel = ReactorModel.FixRateReactor;
            gconfig.ThreadAwakeTimeout = 10;
            config = new ConnectionConfig();
            channelId = config.AddChannel(QosType.ReliableSequenced);
            _channelUnreliable = config.AddChannel(QosType.UnreliableSequenced);

            topology = new HostTopology(config, maxConnections);
            NetworkTransport.Init(gconfig);
            hostId = NetworkTransport.AddHost(topology, port);
            Debug.Log("Server started on port" + port + " with id of " + hostId);
        }
        public bool sendMousePosition;
        public GodPower lastActiveGodPower;
        public void SendMousePosition()
        {
            
            if (sendMousePosition && MapBox.instance.getMouseTilePos() != null && lastMouseTile != MapBox.instance.getMouseTilePos())
            {
                MapBox.instance.CallMethod("highlightFrom", new object[] { MapBox.instance.getMouseTilePos(), "circ_5" });
                if (OnlineMain.instance.isMouseDown)
                {
                    OnlineMain.instance.UsePower(MapBox.instance.getMouseTilePos(), OnlineMain.instance.currentPower);
                }
                lastMouseTile = MapBox.instance.getMouseTilePos();
                byte error;
                byte[] buffer = new byte[1024];
                Stream stream = new MemoryStream(buffer);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, "mouse(" + lastMouseTile.x.ToString() + "," + lastMouseTile.y.ToString() + ",0)");
                int bufferSize = 1024;
                foreach (int connectionId in OnlineMain.instance.activeConnections)
                {
                    NetworkTransport.Send(hostId, connectionId, channelId, buffer, bufferSize, out error);
                    NetworkError networkError = (NetworkError)error;
                    if (networkError != NetworkError.Ok)
                    {
                        Debug.LogError("Server:" + string.Format("client: Error: {0}, hostId: {1}, connectionId: {2}, channelId: {3}", networkError, hostId, connectionId, channelId));
                    }

                }
            }
        }
       
public void SendSocketMessageServer(string input, int connectionId)
        {
    string command;
    string param;
    if (input.Contains('('))
    {
        if (input.Contains(')'))
        {
            param = input.Split('(', ')')[1];
            command = input.Replace("(" + param + ")", "");
            if (command != "loadMap")
            {
                OnlineMain.instance.ParseCommand(command, param, connectionId);
            }
        }
    }
    else
    {
        command = input;
        OnlineMain.instance.ParseCommand(command, "", connectionId);


    }
    byte error;
            byte[] buffer = new byte[1024];
            Stream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            // identify connections by pc username
            if (serverIdentifier == null)
            {
                serverIdentifier = connectionId.ToString();
            }
            formatter.Serialize(stream, serverIdentifier + ":" + input);
            int bufferSize = 1024;
            NetworkTransport.Send(hostId, connectionId, channelId, buffer, bufferSize, out error);
            NetworkError networkError = (NetworkError)error;
            if (networkError != NetworkError.Ok)
            {
                Debug.LogError("Server:" + string.Format("client: Error: {0}, hostId: {1}, connectionId: {2}, channelId: {3}", networkError, hostId, connectionId, channelId));
            }
            else
            {
                Debug.Log("Message sent: '" + input + "'");
            }
            // needs to also execute the same action // but AFTER the message is sent, so it happens semi-simultaneously
            
            /*
            // and at the end.. make sure to send the 
            if (input == "loadMap")
            {
                SendMapAsBytes(connectionId); // doesntwork
            } 
            */

        }
        public void SendMapAsBytes(int connectionId)
        {
            byte[] input = OnlineMain.instance.CurrentMapAsBytes();
            byte error;
            byte[] buffer = new byte[1024];
            Stream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            // identify connections by pc username
            formatter.Serialize(stream, input);
            int bufferSize = 1024;

            NetworkTransport.Send(hostId, connectionId, channelId, buffer, bufferSize, out error);
            NetworkError networkError = (NetworkError)error;
            if (networkError != NetworkError.Ok)
            {
                Debug.LogError("Server:" + string.Format("client: Error: {0}, hostId: {1}, connectionId: {2}, channelId: {3}", networkError, hostId, connectionId, channelId));
            }
            else
            {
            }

        }
    
        public void SendRelayedMessage(string input, int originalConnection)
        {
            byte error;
            byte[] buffer = new byte[1024];
            Stream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            // identify connections by pc username
            formatter.Serialize(stream, input);
            int bufferSize = 1024;
            foreach (int connectionId in OnlineMain.instance.activeClientConnections)
            {
                if (connectionId != originalConnection)
                {
                    NetworkTransport.Send(hostId, connectionId, channelId, buffer, bufferSize, out error);
                    NetworkError networkError = (NetworkError)error;
                    if (networkError != NetworkError.Ok)
                    {
                        Debug.LogError("Server:" + string.Format("client: Error: {0}, hostId: {1}, connectionId: {2}, channelId: {3}", networkError, hostId, connectionId, channelId));
                    }
                    else
                    {
                        Debug.Log("Message sent: '" + input + "'");
                    }
                }
              
            }
        }
    }
   
    public class NetworkClient : MonoBehaviour
    {
        GlobalConfig gconfig;
        //public string host = "71.217.107.137";
        public string host = "127.0.0.1";
        public int port = 8000;
        private int hostId;
        private int connectionId;
        private ConnectionConfig config;
        private HostTopology hostTopology;
        private byte channelId;
        private byte _channelUnreliable;
        public WorldTile lastMouseTile;
        public Dictionary<string, string> actionListForPacket = new Dictionary<string, string>();

        public void SendActionsPacket() // to allow for assured formation support if nothing else, should help a lot
        {
            // one action at a time, several packets for now
            // possibly ICBM style command parsing, one line/packet, many actions
            if (actionListForPacket.Count >= 1)
            {
                ClientSendMessage(actionListForPacket.Last().Key + "(" + actionListForPacket.Last().Value + ")");
                // ex: actionListForPacket.Add("map", "4,5")
                // ex: map(4,5)
                actionListForPacket.Remove(actionListForPacket.Last().Key);
            }
            /*
            foreach (KeyValuePair<string, string> command in actionListForPacket)
            {
              
            }
            */
        }
        public void AddAction(string actionName) // for action list above
        {
            actionListForPacket.Add(actionName, null);
        }
        public void AddAction(string actionName, string param) // for action list above
        {
            actionListForPacket.Add(actionName, param);

        }
        void Start()
        {
        }
        public void Disconnect()
        {
            byte error;
            NetworkTransport.Disconnect(hostId, connectionId, out error);
            NetworkError networkError = (NetworkError)error;
            if (networkError != NetworkError.Ok)
            {
                Debug.LogError("client disconnect error:" + string.Format("{0}", networkError));
            }
            else
            {
                Debug.Log("Disconnected");
            }
        }
        public void Connect()
        {
            
            byte error;
            
            gconfig = new GlobalConfig();
            gconfig.ReactorModel = ReactorModel.FixRateReactor;
            gconfig.ThreadAwakeTimeout = 1;

            config = new ConnectionConfig();
            channelId = config.AddChannel(QosType.ReliableSequenced);
            _channelUnreliable = config.AddChannel(QosType.UnreliableSequenced);
            hostTopology = new HostTopology(config, 1);

            NetworkTransport.Init(gconfig);
            hostId = NetworkTransport.AddHost(hostTopology);
            connectionId = NetworkTransport.Connect(hostId, host, port, 0, out error);
           NetworkError networkError = (NetworkError)error;
            if (networkError != NetworkError.Ok)
            {
                Debug.LogError("client:" + string.Format("Unable to connect to {0}:{1}, Error: {2}", host, port, networkError));
            }
            else
            {// should be host but hiding ip
                Debug.Log(string.Format("Connected to {0}:{1} with hostId: {2}, connectionId: {3}, channelId: {4},", "cody", port, hostId, connectionId, channelId));
                OnlineMain.instance.isClient = true;
            }

        }
        void Update()
        {
            int recHostId;
            int recConnectionId;
            int recChannelId;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            byte error;
            NetworkEventType networkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);
            NetworkError networkError = (NetworkError)error;
            if (networkError != NetworkError.Ok)
            {
                Debug.LogError(string.Format("Error recieving event: {0} with recHostId: {1}, recConnectionId: {2}, recChannelId: {3}", networkError, recHostId, recConnectionId, recChannelId));
            }
            switch (networkEvent)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    if (recConnectionId == connectionId)
                        print("success with connection");
                    else
                        print("got connection");
                    Debug.Log(string.Format("incoming connection event received with connectionId: {0}, recHostId: {1}, recChannelId: {2}", recConnectionId, recHostId, recChannelId));
                    break;
                case NetworkEventType.DataEvent:
                    Stream stream = new MemoryStream(recBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(stream) as string;
                    Debug.Log("incoming message event received: " + message);

                  
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("remote client " + recConnectionId + " disconnected");
                    break;
            }
        }
        public bool sendMousePosition;
        public void SendMouseToHost()
        {

            if (sendMousePosition == true && 
                MapBox.instance.getMouseTilePos() != null && 
                lastMouseTile != MapBox.instance.getMouseTilePos()) // this part needs updated less, maybe a radius check?
            {

                lastMouseTile = MapBox.instance.getMouseTilePos();
                actionListForPacket.Add("mouse", lastMouseTile.x.ToString() + "," + lastMouseTile.y.ToString() + ",0");
            }
        }

        public string clientIdentifier = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None)[1];

        public string addIdentifier(string input)
        {
            if (clientIdentifier == null)
            {
                clientIdentifier = connectionId.ToString();
            }
            return input = clientIdentifier + ":" + input;
        }
        public void ClientSendMessage(string input)
        {
            addIdentifier(input);
            byte error;
            byte[] buffer = new byte[1024];
            Stream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, input);
            int bufferSize = 1024;
            NetworkTransport.Send(hostId, connectionId, channelId, buffer, bufferSize, out error);
            NetworkError networkError = (NetworkError)error;
            if (networkError != NetworkError.Ok)
            {
                Debug.LogError("client:"+string.Format("client: Error: {0}, hostId: {1}, connectionId: {2}, channelId: {3}", networkError, hostId, connectionId, channelId));
            }
            else
            {
                Debug.Log("Message sent: '" + input + "'");
            }
        }
        public void ClientSendReply(string input)
        {
            byte error;
            byte[] buffer = new byte[1024];
            Stream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            // identify connections by pc username
            if (clientIdentifier == null)
            {
                clientIdentifier = connectionId.ToString();
            }
            formatter.Serialize(stream, clientIdentifier + ":" + input);
            int bufferSize = 1024;
            NetworkTransport.Send(hostId, connectionId, channelId, buffer, bufferSize, out error);
            NetworkError networkError = (NetworkError)error;
            if (networkError != NetworkError.Ok)
            {
                Debug.LogError("client:" + string.Format("client: Error: {0}, hostId: {1}, connectionId: {2}, channelId: {3}", networkError, hostId, connectionId, channelId));
            }
        }
        /*
        public void SendCreatureList()
        {
            byte error;
            byte[] buffer = new byte[1024];
            Stream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            // identify connections by pc username
            if (clientIdentifier == null)
            {
                clientIdentifier = connectionId.ToString();
            }
            ScriptableObject newObject = ScriptableObject.CreateInstance(MapBox.instance.creaturesList.GetType());
            newObject =
            formatter.Serialize(stream, newObject);
            int bufferSize = 1024;
            NetworkTransport.Send(hostId, connectionId, channelId, buffer, bufferSize, out error);
            NetworkError networkError = (NetworkError)error;
            if (networkError != NetworkError.Ok)
            {
                Debug.LogError("client:" + string.Format("client: Error: {0}, hostId: {1}, connectionId: {2}, channelId: {3}", networkError, hostId, connectionId, channelId));
            }
            else
            {
                Debug.Log("Message sent!");
            }
        }
        */
    }


}
