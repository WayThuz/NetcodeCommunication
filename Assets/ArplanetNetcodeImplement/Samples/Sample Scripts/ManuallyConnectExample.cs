using Wayne.Network.NetcodeImplement;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManuallyConnectExample : MonoBehaviour
{
    [SerializeField] private Button connectButton;

    IEnumerator Start() {
        var wait = new WaitForEndOfFrame();
        while(NetConnectManager.Instance.Config == null) {
            yield return wait;
        }
        if(!connectButton) yield break;
        if(NetConnectManager.Instance.Config.directConnect) {
            connectButton.GetComponentInChildren<TMP_Text>().text = "Cannot Connect Manually";
            connectButton.onClick.RemoveAllListeners();
        }
        else {
            connectButton.onClick.AddListener(NetConnectManager.Instance.ConnectManually);
        }
    }

}
