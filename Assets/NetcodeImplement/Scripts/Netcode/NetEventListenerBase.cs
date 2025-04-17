using UnityEngine;

namespace Wayne.Network.NetcodeImplement {
    /// <summary>
    /// 當自身連線狀態改變時，自動喚起對應事件。
    /// 方便場景、各種 Component 的初始化。
    /// </summary>
    public abstract class NetEventListenerBase : MonoBehaviour {

        protected bool isRegistered = false;
        protected NetEventPreregisterHandler.NetEvent onConnect;
        protected NetEventPreregisterHandler.NetEvent onDisconnect;

        //NetEventListenerBase 應當在自己連線狀態改變時才會執行
        //Host 端會因為 client 連線／斷線而呼叫 OnConnectEvent
        //如果 Host 需要在每個 Client 連線／斷線時都執行程式，需要自行寫 script 註冊到 OnConnectionEvent 
        protected virtual void OnEnable() {
            onConnect ??= new(OnConnect);
            onDisconnect ??= new(OnDisconnect);
            if(!isRegistered) {
                NetEventPreregisterHandler.Register(NetEventType.Connect, onConnect);
                NetEventPreregisterHandler.Register(NetEventType.Disconnect, onDisconnect);
                isRegistered = true;
            }
        }

        protected abstract void OnConnect(NetcodeConfig config);

        protected abstract void OnDisconnect(NetcodeConfig config);

        protected virtual void OnDestroy() {
            NetEventPreregisterHandler.Deregister(NetEventType.Connect, onConnect);
            NetEventPreregisterHandler.Deregister(NetEventType.Disconnect, onConnect);
        }
    }
}
