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

        public string addIdentifier(string input)
        {
            if (serverIdentifier == null)
            {
                serverIdentifier = connectionId.ToString();
            }
            return input = serverIdentifier + ":" + input;
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
                        OnlineMain.instance.ParseCommand(command, param, connectionId, serverIdentifier);
                    }
                }
            }
            else
            {
                command = input;
                OnlineMain.instance.ParseCommand(command, "", connectionId, serverIdentifier);
            }
            byte error;
            byte[] buffer = new byte[1024];
            Stream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
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
                Debug.Log("Message sent from server: '" + input + "'");
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
                        Debug.Log("Relayed message re-sent from "+ connectionId +": '" + input + "'");
                    }
                }

            }
        }
    }

}
