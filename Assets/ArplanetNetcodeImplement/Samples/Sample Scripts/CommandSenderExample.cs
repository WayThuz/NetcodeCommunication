using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Wayne.Network.NetcodeImplement;

public class CommandSenderExample : CommandSender
{
    [SerializeField] Button sendCommandButton;

    void Start() {
        if(NetConnectManager.Instance.Config.character == NetcodeConfig.Character.Client) {
            sendCommandButton.gameObject.SetActive(false);
        }
        else {
            sendCommandButton.onClick.AddListener(SendCommand);
        }
    }

    /// <summary>
    /// Host 端：送出 command 給 client 執行
    /// </summary>
    protected override void SendCommand() {
        Debug.Log("Send Command");
        DefaultPatchCommand c = new() {
            elementID = "CommandExecutorExample",
            command = "sphere"
        };
        HostPatchHandler.SendHardPatch(c);
    }
}
