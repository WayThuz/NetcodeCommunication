using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace Wayne.Network.NetcodeImplement { 
    public interface IPatchCommand {
        void InitializeValue();

        string Serialize();

        void Deserialize(Patch patch, out bool isSuccess);
    }
}
