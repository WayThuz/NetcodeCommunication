using UnityEngine;
using Wayne.Network.NetcodeImplement;

public class OnConnectExample : NetEventListenerBase
{
    protected override void OnConnect(NetcodeConfig config) {
        Debug.Log("OnConnectExample: OnConnect");
    }

    protected override void OnDisconnect(NetcodeConfig config) {
        Debug.Log("OnConnectExample: OnDisconnect");
    }
}
