using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Wayne.Network.NetcodeImplement {
    public class HostConnector : INetConnector {
        protected UnityTransport transport;
        protected string ip;
        protected GameObject playerPref;

        public HostConnector(UnityTransport transport) {
            this.transport = transport;
        }

        public HostConnector(UnityTransport transport, string ip, GameObject playerPref) {
            this.transport = transport;
            this.ip = ip;
            if(playerPref && playerPref.TryGetComponent<NetworkObject>(out _)) {
                this.playerPref = playerPref;
            }
            else if(playerPref) {
                Debug.LogError($"Cannot assign playerPref {playerPref.name} because it has no NetworkObject Component.");
            }
            else {
                Debug.LogWarning("Cannot assign playerPref because it is null.");
            }
        }

        public void Connect() {
            if(transport == null) throw new NullReferenceException("UnityTransport is not assigned");
            transport.ConnectionData.Address = ip;
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientConnectedCallback += GeneratePlayer;
            NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
        }

        protected virtual void GeneratePlayer(ulong id) {
            Debug.Log("GeneratePlayer id: " + id);
            if(playerPref) {
                var g = GameObject.Instantiate(playerPref);
                g.GetComponent<NetworkObject>().SpawnWithOwnership(id);
            }
            ///Player 相關初始化改到 PlayerController 內執行
        }

        public void Disconnect() {
            NetworkManager.Singleton.OnClientConnectedCallback -= GeneratePlayer;
            NetworkManager.Singleton.Shutdown();
        }
    }
}
