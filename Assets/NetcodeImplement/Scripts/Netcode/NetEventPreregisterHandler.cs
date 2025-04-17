using System;
using System.Collections.Generic;

namespace Wayne.Network.NetcodeImplement {
    public enum NetEventType {
        Connect, 
        Disconnect
    }

    /// <summary>
    /// 透過此 Handler 進行 NetEventListenerBase 的註冊。
    /// </summary>
    public static class NetEventPreregisterHandler {
        
        public delegate void NetEvent(NetcodeConfig config);
        private static Dictionary<NetEventType, NetEvent> netEvents = new();

        public static void Register(NetEventType type, NetEvent action) {
            if(netEvents.ContainsKey(type)) {
                netEvents[type] += action;
            }
            else {
                netEvents.Add(type, action);
            }
        } 

        public static void Deregister(NetEventType type, NetEvent action) {
            if(netEvents.ContainsKey(type)) {
                netEvents[type] -= action;
            }
        }

        public static void InvokeNetEvent(NetEventType type, NetcodeConfig config) {
            if(!netEvents.ContainsKey(type)) return;
            netEvents[type]?.Invoke(config);
        }

        public static void Clear() {
           netEvents.Clear();
        }
    }
}

