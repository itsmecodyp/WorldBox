using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using System.Net;
using Boerman.Networking;
using System.Linq;

namespace SimpleNetty {
   
   [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
   class Main : BaseUnityPlugin {
        public const string pluginGuid = "cody.worldbox.simple.netty";
        public const string pluginName = "SimpleNetty";
        public const string pluginVersion = "0.0.0.0";


        public void Awake()
		{
            Debug.Log("simple netty loaded!");

        }

        public static TcpServer existingServer;
        public static TcpClient client1;

        public void Update()
		{
			if(Input.GetKeyDown(KeyCode.N)) {
                TcpServer server = new TcpServer(
    new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2626));
                server.Start();

                server.Connected += (sender, e) => {
                    Console.WriteLine($"{e.TimeStamp}: {e.EndPoint} server acknowledges connection");
                };

                server.Received += (sender, e) => {
                    Console.WriteLine("server received dataString:" + e.Data);
                    ParseData(e.Data);
                };
                existingServer = server;
                Debug.Log("server started");
            }
            if(Input.GetKeyDown(KeyCode.M)) {
                if(existingServer != null) {
                    existingServer.Send("testmessage from server!");
				}
                if(client1 != null) {
                    client1.Send("testmessage from client!");
                }
            }
            if(Input.GetKeyDown(KeyCode.B)) {
                if(existingServer != null) {
                    existingServer.Send("test(p1)");
                }
                if(client1 != null) {
                    client1.Send("mooka");
                }
            }
        }

        public void ParseData(string dataString)
		{
            string command;
            if(dataString.Contains('(')) {
                if(dataString.Contains(')')) {
                    var param = dataString.Split('(', ')')[1];
                    command = dataString.Replace("(" + param + ")", "");
                    ParseCommand(command, param);
                }
            }
            else {
                command = dataString;
                ParseCommand(command, "");
            }
        }

        public void OnGUI()
		{
			if(GUILayout.Button("Connect")) {
                //ConnectClient(ipToConnectTo, 1000);
                //var ceras = new CerasSerializer();
                //var bytes = ceras.Serialize(ipToConnectTo);
                ConnectClient();
            }
            ipToConnectTo = GUILayout.TextField(ipToConnectTo);
        }

        public async void ConnectClient()
		{
            var client = new Boerman.Networking.TcpClient();
            await client.Open(new IPEndPoint(IPAddress.Parse(ipToConnectTo), 2626));

            client.Connected += (sender, e) => {
                Console.WriteLine($"{e.TimeStamp}: {e.EndPoint} client acknowledges connection");
            };

            client.Received += (sender, e) => {
                Console.WriteLine("client received dataString:" + e.Data);
                ParseData(e.Data);
            };

            client1 = client;
        }

        public string ipToConnectTo = "";

        public static void Send(string input)
		{
            if(existingServer != null) {
                existingServer.Send(input);
            }
            if(client1 != null) {
                client1.Send(input);
            }
        }

        public void setupPatches()
		{
            Harmony harmony = new Harmony(pluginName);
            MethodInfo original;
            MethodInfo patch;
            /*
            original = AccessTools.Method(typeof(PowerButtonSelector), "setPower");
            patch = AccessTools.Method(typeof(Main), "setPower_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));
            Debug.Log(pluginName + ": Harmony patch finished: " + patch.Name);
            */

        }

        public void ParseCommand(string command, string param)
        {
            if(string.IsNullOrEmpty(param) == false) {
                Console.WriteLine("command:" + command + "/param:" + param);
            }
            else {
                Console.WriteLine("command:" + command);
            }

            if(command == "setPower" && param != "") {
                //AssetManager.powers.get(param)
                Debug.Log("setting power to id: " + param);
                PowerButtonSelector.instance.setPower(PowerButton.get(param));
            }
           
        }



     

    }

}
