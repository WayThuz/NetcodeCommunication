using Wayne.Network.NetcodeImplement;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConnectStatusDisplay : MonoBehaviour {
    [SerializeField] TMP_Text deviceCharacterText;
    [SerializeField] TMP_Text connectionCountText;
    [SerializeField] TMP_Text connectionStatusText;
    private bool canUpdateStatus = false;

    void Start() {
        var config = NetConnectManager.Instance.Config;
        if(config.character == NetcodeConfig.Character.Client) {
            connectionCountText.text = "";
        }
        canUpdateStatus = true;
        NetworkManager.Singleton.OnConnectionEvent += OnConnectStatusChange; 
    }

    void Update() {
        if(!canUpdateStatus) return;
        connectionCountText.text = $"Current connected player: {NetStatusManager.ConnectClientCount}";
        deviceCharacterText.text = $"My Character: {NetConnectManager.Instance.Config.character}, my id: {NetworkManager.Singleton.LocalClientId}";
    }

    protected void OnConnectStatusChange(NetworkManager manager, ConnectionEventData data) {
        connectionStatusText.text = $"Client ID {data.ClientId} change connect status to {data.EventType}";
    }
}
