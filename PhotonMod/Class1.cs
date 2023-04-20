using System.Reflection;
using BepInEx;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace PhotonMod {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Loader : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.photon.mod";
        public const string pluginName = "SimplePhoton";
        public const string pluginVersion = "0.0.0.0";
        public void Awake()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;

            Debug.Log("Loader awake");
            onlineObject.AddComponent(typeof(OnlineManager));
        }

        public GameObject onlineObject = new GameObject();
    }

    public class OnlineManager : MonoBehaviourPunCallbacks {
       

        public void Awake()
		{
            Debug.Log("OnlineManager awake");
		}

        public void Start()
		{
            Connect();
        }

        public void Connect()
        {
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if(PhotonNetwork.IsConnected) {
                RoomOptions roomOptions = new RoomOptions();
                roomOptions.IsVisible = true;
                roomOptions.MaxPlayers = 5;
                Debug.LogError("1");
                PhotonNetwork.JoinOrCreateRoom("Test", roomOptions, TypedLobby.Default);
            }
            else {
                // #Critical, we must first and foremost connect to Photon Online Server.
                AppSettings newSettings = new AppSettings();
                newSettings.AppIdChat = "c0f01036-236e-4446-9b6b-199cb43be934";
                //usually setup in editor and unchangable during runtime, with normal .dll an error is thrown, can modify that dll in editor and work around it
                newSettings.AppIdRealtime = "d4ec138d-d648-4786-ae01-e55b337d3fa4";
                newSettings.AppVersion = "0.1";
                newSettings.UseNameServer = true;
                newSettings.NetworkLogging = DebugLevel.ALL;
                newSettings.Protocol = ConnectionProtocol.Udp;
                newSettings.FixedRegion = "us";
                Debug.LogError("About to connect");
                PhotonNetwork.ConnectUsingSettings(newSettings);
                PhotonNetwork.GameVersion = "0.1";
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.LogError("OnConnectedToMaster() was called by PUN.");
            //PhotonNetwork.CreateRoom("MyMatch");
            //PhotonNetwork.JoinRandomRoom();
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.MaxPlayers = 5;
            Debug.LogError("2");
            Debug.LogError("About to join room");
            PhotonNetwork.JoinOrCreateRoom("Test", roomOptions, TypedLobby.Default);
        }

        public static Test onlineT = new Test();
    }

    public class Test : MonoBehaviourPunCallbacks {

        public void Awake()
		{
            Debug.Log("Test awake");
        }
    }
}
