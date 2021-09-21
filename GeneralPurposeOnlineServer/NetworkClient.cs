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
        public Dictionary<string, string> actionListForPacket = new Dictionary<string, string>();

        public void SendActionsPacket()
        {
            if (actionListForPacket.Count >= 1)
            {
                ClientSendMessage(actionListForPacket.Last().Key + "(" + actionListForPacket.Last().Value + ")");
                actionListForPacket.Remove(actionListForPacket.Last().Key);
                // ex: actionListForPacket.Add("map", "4,5")
                // ex: map(4,5)
            }
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
        // discontinued for now
        }

        public string clientIdentifier = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None)[1];

        public string addIdentifier(string input)
        {
            if (clientIdentifier == null)
            {
                clientIdentifier = connectionId.ToString();
            }
            return clientIdentifier + ": " + input;
        }
        public void ClientSendMessage(string input)
        {
            input = addIdentifier(input);
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
                Debug.LogError("client:" + string.Format("client: Error: {0}, hostId: {1}, connectionId: {2}, channelId: {3}", networkError, hostId, connectionId, channelId));
            }
            else
            {
                Debug.Log("Message sent from " + clientIdentifier +": '" + input + "'");
            }
        }
        public void ClientSendReply(string input)
        {
            input = addIdentifier(input);
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
                Debug.LogError("client:" + string.Format("client: Error: {0}, hostId: {1}, connectionId: {2}, channelId: {3}", networkError, hostId, connectionId, channelId));
            }
        }
    }

}
