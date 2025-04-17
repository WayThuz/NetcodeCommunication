using UnityEngine;
using Wayne.Network.NetcodeImplement;

[System.Serializable]
public class DefaultPatchCommand : IPatchCommand
{
    public string elementID;
    public string command;

    public void InitializeValue() {
        elementID = "";
        command = "";
    }

    public string Serialize() {
        return $"{elementID},{command}";
    }

    public void Deserialize(Patch patch, out bool isSuccess) {
        var c = patch.command.Split(',');
        isSuccess = c.Length == 2;
        if(isSuccess) {
            elementID = c[0];
            command = c[1];
        }
    }

    public override string ToString() {
        return $"{elementID} {command}";
    }
}
