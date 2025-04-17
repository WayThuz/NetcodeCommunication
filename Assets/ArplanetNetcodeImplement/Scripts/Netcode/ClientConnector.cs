using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Wayne.Network.NetcodeImplement {
    public class ClientConnector : INetConnector {
        private string ip;
        private UnityTransport transport;

        public ClientConnector(string ip, UnityTransport transport) {
            this.ip = ip;
            this.transport = transport;
        }

        public void Connect() {
            if(transport == null) throw new NullReferenceException("UnityTransport is not assigned");
            Debug.LogWarning("Connect as client");      
            transport.ConnectionData.Address = ip;             
            var result = NetworkManager.Singleton.StartClient();
            if(!result) {
                throw new Exception("StartClient Failed");
            }
        }

        public void Disconnect() {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
