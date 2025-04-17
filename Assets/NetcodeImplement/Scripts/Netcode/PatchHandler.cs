using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wayne.Network.NetcodeImplement {

    public class PatchHandler : MonoBehaviour {
        /// <summary>
        /// 本地端最新的 Patch 版本
        /// </summary>
        protected ulong currentPatchVersion;
        protected ulong hostID = 0;

        public void Init() {
            NetworkManager.Singleton.OnConnectionEvent += OnConnectStatusChange;
            NetworkManager.Singleton.CustomMessagingManager.OnUnnamedMessage += OnReceivePatch;
        }

        protected virtual void OnConnectStatusChange(NetworkManager manager, ConnectionEventData data) { }

        protected virtual void SendPatch(Patch patch) {  
            using var writer = new FastBufferWriter(1100, Allocator.Temp);
            writer.WriteValueSafe(patch);
            NetworkManager.Singleton.CustomMessagingManager.SendUnnamedMessageToAll(writer);
        }

        protected virtual void SendPatch(ulong clientID, Patch patch) {
            using var writer = new FastBufferWriter(1100, Allocator.Temp);
            writer.WriteValueSafe(patch);
            NetworkManager.Singleton.CustomMessagingManager.SendUnnamedMessage(clientID, writer);
        }

        protected virtual void OnReceivePatch(ulong clientId, FastBufferReader reader) { }
    } 
}
