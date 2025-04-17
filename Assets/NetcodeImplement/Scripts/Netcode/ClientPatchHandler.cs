using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Wayne.Network.NetcodeImplement {
    public class ClientPatchHandler : PatchHandler {
        private bool isCurrentHardPatchExist = false;
        private List<IPatchCommandExecutor> patchCommandExecutors = new();
        private Queue<Patch> patchToExecute = new();

        public void RegisterExecutor<T>(T t) where T : IPatchCommandExecutor {
            if(!patchCommandExecutors.Contains(t)) {
                patchCommandExecutors.Add(t);
            }
        }

        public void DeregisterExecutor<T>(T t) where T : IPatchCommandExecutor {
            if(patchCommandExecutors.Contains(t)) {
                patchCommandExecutors.Remove(t);
            }
        }

        protected override void OnReceivePatch(ulong clientID, FastBufferReader reader) {
            void LetPatchWaiting(Patch patch) {
                patch.status = Patch.Status.Waiting;
                patchToExecute.Enqueue(patch);
                SendPatch(patch);
            }
            reader.ReadValueSafe(out Patch patch);
            if(patch.type == Patch.Type.Hard) {
                if(isCurrentHardPatchExist) {
                    LetPatchWaiting(patch);
                }
                else {
                    if(patchToExecute.Count > 0) {
                        LetPatchWaiting(patch);
                        patch = patchToExecute.Dequeue(); //取出最舊的 patch 並執行
                    }
                    ExeHardPatchCommand(patch);
                }
            }
            else { //不含有 command 的 patch 在被 client 收到後可以直接 acknowledge 並且傳回
                patch.status = Patch.Status.Acknowledge;
                SendPatch(patch);
            }
        }

        private void ExeHardPatchCommand(Patch patch) {
            Debug.Log($"Exe Hard Patch {patch}");
            isCurrentHardPatchExist = true;
            patchCommandExecutors.ForEach(executor => {
                if(executor != null && executor.Verify(patch)) {
                    executor.ExeCommand(patch);
                }
                else if (executor == null) {
                    Debug.LogWarning("Executor is null");
                }
                else {
                    Debug.LogWarning("Verify failed.");
                }
            });
            OnPatchCommandExe(patch);
        }

        protected void OnPatchCommandExe(Patch patch) {
            patch.status = Patch.Status.Acknowledge;
            SendPatch(patch);
            isCurrentHardPatchExist = false;
        }

        protected override void SendPatch(Patch patch) {
            patch.receivedClientOriginID = OriginID;
            base.SendPatch(hostID, patch);
        }

        public ulong OriginID { get; set; }
    }
}
