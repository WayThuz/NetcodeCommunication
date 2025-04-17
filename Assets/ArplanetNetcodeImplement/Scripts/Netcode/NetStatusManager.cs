using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using Unity.Collections;

namespace Wayne.Network.NetcodeImplement {
    /// <summary>
    /// 管理 & 檢查連線人數、連線狀態、場景載入狀態的 Manager
    /// </summary>
    public class NetStatusManager : MonoBehaviour {

        /// <summary>
        /// Editor debug 用
        /// </summary>
        [SerializeField] private List<ClientInfo> displayClientInfo = new();

        private PatchHandler patchHandler;
        private const string clientStatusName = "ClientStatus";

        /// <summary>
        /// 裝置第一次連線獲得的 network ID
        /// </summary>
        private ulong? originID = null;
        public static List<ClientInfo> clientInfos = new();

        public void StartStatusManage(NetcodeConfig config) {
            if(config.character == NetcodeConfig.Character.Host) {
                NetworkManager.Singleton.OnConnectionEvent += HostSideManageConnectionEvent;
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(clientStatusName, OnHostReceivedClientStatus);
                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneChange;
                if(!patchHandler) {
                    patchHandler = gameObject.AddComponent<HostPatchHandler>();
                }
            }
            else {
                NetworkManager.Singleton.OnConnectionEvent += ClientSideConnectionEvent; 
                if(!patchHandler) {
                    patchHandler = gameObject.AddComponent<ClientPatchHandler>();
                }
            }
            patchHandler.Init();
        }

        private void OnSceneChange(SceneEvent sceneEvent) {
            IEnumerator UpdateClientState() {
                clientInfos.ForEach(c => c.isLoadComplete = false);
                yield return new WaitForSeconds(5);
                clientInfos.ForEach(client => {
                    if(!client.isLoadComplete) {
                        Debug.Log($"Client ID {client.networkID} does not load current scene, force disconnecting.");
                        NetworkManager.Singleton.DisconnectClient(client.networkID);
                    }
                });
            }
            void OnClientLoadComplete(ulong id) {
                if(id != NetworkManager.Singleton.LocalClientId) {
                    for(int i = 0; i < clientInfos.Count; i++) {
                        if(clientInfos[i].networkID != id) continue;
                        clientInfos[i].isLoadComplete = true;
                    }
                }
            }
            Debug.Log($"ID: {sceneEvent.ClientId} on scene {sceneEvent.Scene.name} with event {sceneEvent.SceneEventType}");
            switch(sceneEvent.SceneEventType) {
                case SceneEventType.Load:
                    StartCoroutine(UpdateClientState());
                    break;
                case SceneEventType.LoadComplete:
                    OnClientLoadComplete(sceneEvent.ClientId);
                    break;
                default:
                    break;
            }
        }

        private void HostSideManageConnectionEvent(NetworkManager manager, ConnectionEventData data) {
            Debug.Log("HostSideManageEvent: " + data.EventType);
            var type = data.EventType;
            if(type != ConnectionEvent.ClientConnected && type != ConnectionEvent.ClientDisconnected) return;
            UpdateClientStatus(data.ClientId, data.EventType == ConnectionEvent.ClientConnected);
        }

        // client 自己 connect/disconnect 才執行
        private void ClientSideConnectionEvent(NetworkManager manager, ConnectionEventData data) {
            var type = data.EventType;
            if(type == ConnectionEvent.ClientConnected) {  
                if(originID == null) {
                    originID = data.ClientId;
                    (patchHandler as ClientPatchHandler).OriginID = (ulong)originID;
                }
                ClientInfo info = new() {
                    deviceID = SystemInfo.deviceUniqueIdentifier,
                    originID = (patchHandler as ClientPatchHandler).OriginID,
                    isConnected = type == ConnectionEvent.ClientConnected,
                };
                SendClientInfoToHost(info);
            }
        }

        private void SendClientInfoToHost(ClientInfo info) {
            using var writer = new FastBufferWriter(1100, Allocator.Temp);
            writer.WriteValueSafe(info);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(clientStatusName, 0, writer);
        }

        private void OnHostReceivedClientStatus(ulong clientId, FastBufferReader reader) {
            Debug.Log("Update Client list");
            reader.ReadValueSafe(out ClientInfo info);
            UpdateClientID(info.deviceID, clientId, info.originID); ;
            UpdateClientStatus(clientId, info.isConnected);
        }

        /// <param name="deviceID"></param>
        /// <param name="networkID">Client</param>
        /// <param name="originID">Client 本地上一次連線使用的 ID，Editor Host 才會拿來識別 ClientInfo</param>
        /// <param name="type"></param>
        private void UpdateClientID(string deviceID, ulong networkID, ulong originID) {
            bool isClientIDExist = false;
            for(int i = 0; i < clientInfos.Count; i++) {
                #if DEVELOPMENT_TEST
                if(clientInfos[i].originID != originID) continue; //Local 端多開測試用，等號成立表示為同一個 client, 只是重複連線而已
                #else
                if(clientInfos[i].deviceID != deviceID) continue; //實際多人連線時，利用 device ID 來檢核 client 身分
                #endif
                clientInfos[i].networkID = networkID;
                Debug.Log($"Find Client ID {networkID} with Device {deviceID}.");
                isClientIDExist = true;
                break;
            }
            if(!isClientIDExist) {
                clientInfos.Add(new() {
                    deviceID = deviceID,
                    originID = originID,
                    networkID = networkID,
                    isLoadComplete = true,
                    isConnected = true //default
                });
                Debug.Log($"Add Client ID {networkID} with Device {deviceID}.");
            }
        }

        private void UpdateClientStatus(ulong networkID, bool isConnect) {
            Debug.Log($"Update Connection Status to {isConnect} for networkID: {networkID}");
            for(int i = 0; i < clientInfos.Count; i++) {
                if(clientInfos[i].networkID != networkID) continue;
                clientInfos[i].isConnected = isConnect;
                break;
            }
            ConnectClientCount = clientInfos.Where(x => x.isConnected).Count();
            displayClientInfo = clientInfos;
        }

        private void OnApplicationQuit() {
            if(NetConnectManager.Instance.Config.character != NetcodeConfig.Character.Host) return;
            ConnectClientCount = 0;
        }

        public static ulong GetCurrentNetworkID(ulong originID) {
            foreach(var client in clientInfos) {
                if(client.originID != originID) continue;
                return client.networkID;
            }
            throw new Exception($"None of these clients has origin id {originID}");
        }

        public static bool IsAllConnectedClientsInList(List<ulong> listToExamine) {
            var checkTable = new Dictionary<ulong, int>();
            var connectedClients = clientInfos.Where(x => x.isConnected).Select(x =>　x.originID);
            foreach(var connectedID in connectedClients) {
                if(checkTable.ContainsKey(connectedID)) {
                    checkTable[connectedID]++;
                }
                else {
                    checkTable.Add(connectedID, 1);
                }
            }
            foreach(var id in listToExamine) {
                if(checkTable.ContainsKey(id)) {
                    checkTable[id]--;
                }
                else {
                    return false;
                }
            }
            return checkTable.Values.All(c => c == 0);
        }

        public static int ConnectClientCount { get; private set; } = 0;

        [Serializable]
        public class ClientInfo : INetworkSerializable {
            public string deviceID;
            public ulong originID; 
            public ulong networkID;
            public bool isConnected;
            public bool isLoadComplete;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
                serializer.SerializeValue(ref deviceID);
                serializer.SerializeValue(ref originID);
                serializer.SerializeValue(ref isConnected);
            }
        }
    }
}
   