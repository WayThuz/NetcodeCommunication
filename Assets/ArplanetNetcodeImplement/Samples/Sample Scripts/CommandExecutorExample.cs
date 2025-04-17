using UnityEngine;
using Unity.Netcode;
using TMPro;
using Wayne.Network.NetcodeImplement;

public class CommandExecutorExample : CommandExecutor {

    [SerializeField] TMP_Text onCommandReceivedText;
    [SerializeField] private GameObject sphere;

    protected override void Init(NetcodeConfig config) {
        if(config.character == NetcodeConfig.Character.Host) {
            onCommandReceivedText.gameObject.SetActive(false);
        }
        base.Init(config);     
    }

    /// <summary>
    /// Client 端的 IPatchCommandExecutor 會對 Host 傳遞過來的 patch 做驗證(ClientPatchHandler.ExeHardPatchCommand)
    /// 驗證成功才能執行 command
    /// </summary>
    /// <param name="patch"></param>
    /// <returns></returns>
    public override bool Verify(Patch patch) {
        DefaultPatchCommand command = new();
        command.Deserialize(patch, out bool isSuccess);
        return isSuccess && command.elementID == "CommandExecutorExample";
    }

    /// <summary>
    /// IPatchCommandExecutor 在驗證完成後會執行 command
    /// </summary>
    /// <param name="patch"></param>
    public override void ExeCommand(Patch patch) {
        Debug.Log("ExeCommand");
        DefaultPatchCommand command = new();
        command.Deserialize(patch, out _);
        onCommandReceivedText.text = $"Exe command: {command.elementID}, {command.command}";
        if(command.command == "sphere") {
            var rand = Random.Range(-1.0f, 1.0f);
            Instantiate(sphere, rand*Vector3.one, Quaternion.identity);
        }
    }
}
