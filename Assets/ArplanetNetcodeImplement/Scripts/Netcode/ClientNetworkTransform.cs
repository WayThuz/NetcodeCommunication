using Unity.Netcode.Components;
using UnityEngine;


namespace Wayne.Network.NetcodeImplement {
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
