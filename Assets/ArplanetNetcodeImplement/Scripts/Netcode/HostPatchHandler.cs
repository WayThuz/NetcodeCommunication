using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Wayne.Network.NetcodeImplement {
    public class HostPatchHandler : PatchHandler {
        private bool shouldForceALLACK = false;
        private const int countdownAmount = 100;

        private const float receiveTimeout = 5;
        private const int resendDelay = 1;
        private WaitForSeconds timeout;

        /// <summary>
        /// Client "OriginID" 對應到 Unreceived Patch List
        /// </summary>
        private Dictionary<ulong, List<Patch>> clientToUnreceivedHardPatch = new();

        /// <summary>
        /// Patch 對應到 Client "OriginID" List
        /// </summary>
        private Dictionary<Patch, List<ulong>> hardPatchToReceivedClient = new();

        void Awake() {
            timeout = new(receiveTimeout);
        }

        void Update() {
            //等候還在連線但沒有收到指令的用戶會等候(not include 斷線用戶 or 臨時加入的用戶)
            if(shouldForceALLACK && !IsAllACK(currentPatchVersion)) return;
            if(Time.frameCount % countdownAmount == 0) {
                SendSoftPatch(); 
            }
        }

        protected void SendSoftPatch() {
            PrepareSendNewPatch(Patch.Type.Soft, out var patch);
            base.SendPatch(patch);
        }

        public void SendHardPatch<T>(T command) where T : IPatchCommand {
            PrepareSendNewPatch(Patch.Type.Hard, out var patch);
            patch.command = command.Serialize();
            StartCoroutine(WaitClientReceivedHardPatch(patch));
            Debug.LogWarning($"Send HardPatch {patch} to all clients.");
            base.SendPatch(patch);
        }

        private void PrepareSendNewPatch(Patch.Type type, out Patch patch) {
            shouldForceALLACK = type == Patch.Type.Hard;
            currentPatchVersion++;
            patch = new() {
                type = type,
                receivedClientOriginID = 0,
                version = currentPatchVersion,
                status = Patch.Status.None,
                command = string.Empty
            };
        }

        private IEnumerator WaitClientReceivedHardPatch(Patch patch) {
            hardPatchToReceivedClient[patch] = new();
            yield return timeout;
            var clientsReceived = hardPatchToReceivedClient[patch];
            var clientsUnreceived = NetStatusManager.clientInfos.Where(c => !clientsReceived.Contains(c.originID)).Select(c => c.originID).ToList();
            patch.status = Patch.Status.Unreceived;
            foreach(var clientOriginID in clientsUnreceived) {
                OnHardPatchUnreceived(clientOriginID, patch);
            }
        }

        protected void OnHardPatchUnreceived(ulong originID, Patch patch) {
            AddToUnreceivedHardPatch(originID, patch);
            var networkID = NetStatusManager.GetCurrentNetworkID(originID);
            patch.status = Patch.Status.None;
            StartCoroutine(ResendPatch(networkID, patch));
        }

        protected override void OnReceivePatch(ulong clientID, FastBufferReader reader) {
            reader.ReadValueSafe(out Patch patch);
            var originID = patch.receivedClientOriginID; 
            if(originID == hostID) return;
            if(patch.status == Patch.Status.Acknowledge) {
                if(patch.type != Patch.Type.Hard) return;
                AddToReceivedClients(originID, patch);
                RemoveFromUnreceivedHardPatch(originID, patch);
            }
        }

        private void AddToReceivedClients(ulong originID, Patch patch) {
            if(originID == hostID) return;
            foreach(var pair in hardPatchToReceivedClient) {
                if(pair.Key.IsEqual(patch) && !pair.Value.Contains(originID)) {
                    pair.Value.Add(originID);
                    break;
                }
            }
        }

        private void AddToUnreceivedHardPatch(ulong originID, Patch patch) {
            if(originID == hostID) return;
            Debug.LogWarning($"Client with originID: {originID} does not receive patch: {patch}");
            if(!clientToUnreceivedHardPatch.ContainsKey(originID)) {
                clientToUnreceivedHardPatch[originID] = new();
            }
            clientToUnreceivedHardPatch[originID].Add(patch);
        }

        private void RemoveFromUnreceivedHardPatch(ulong originID, Patch patch) {
            if(originID == hostID) return;
            if(!clientToUnreceivedHardPatch.ContainsKey(originID)) return;
            var unreceivedPatches = clientToUnreceivedHardPatch[originID];
            for(int i = unreceivedPatches.Count - 1; i > -1; i--) {
                //不一定是 current version(可能是執行舊的)
                if(!unreceivedPatches[i].IsEqual(patch)) continue;
                unreceivedPatches.RemoveAt(i);
                break;
            }
        }

        private IEnumerator ResendPatch(ulong clientID, Patch patch) {
            yield return new WaitForSeconds(resendDelay);
            base.SendPatch(clientID, patch);
        }

        private bool IsAllACK(ulong version) {
            foreach(var pair in hardPatchToReceivedClient) {
                if(!pair.Key.IsEqual(version)) continue;
                return NetStatusManager.IsAllConnectedClientsInList(pair.Value);
            }
            Debug.LogError($"There's no hard patch with version: {version}.");
            return false;
        }
    }
}
