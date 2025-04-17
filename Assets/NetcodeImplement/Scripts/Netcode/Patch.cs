using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Wayne.Network.NetcodeImplement {
    /// <summary>
    /// 確認各 client 連線狀態是否同步的 heartbeat
    /// </summary>
    [Serializable]
    public struct Patch : INetworkSerializable {

        public enum Status {
            None,
            Unreceived,
            Waiting,
            Acknowledge,
        }

        public enum Type {
            /// <summary>
            /// 不附帶指令，僅確認連線狀態
            /// </summary>
            Soft,

            /// <summary>
            /// 附帶指令讓 client script 解析跟執行
            /// </summary>
            Hard
        }

        public Type type;
        public Status status;
        public ulong receivedClientOriginID;
        public ulong version;
        public string command;
        

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
            serializer.SerializeValue(ref type);
            serializer.SerializeValue(ref status);
            serializer.SerializeValue(ref receivedClientOriginID);
            serializer.SerializeValue(ref version);
            serializer.SerializeValue(ref command);
        }

        public bool IsEqual(ulong version) => this.version == version;

        public bool IsEqual(Patch patch) => version == patch.version;
        
        public override string ToString() => $"Version {version}, Type: {type}, Status: {status}, Command: {command}";  
    }

} 