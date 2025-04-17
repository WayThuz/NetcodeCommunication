using UnityEngine;

namespace Wayne.Network.NetcodeImplement {
    public class CommandSender : MonoBehaviour {
        private HostPatchHandler hostPatchHandler;

        /// <summary>
        /// (1) Create Command command
        /// (2) hostPatchHandler.SendCommand(Command)
        /// </summary>
        protected virtual void SendCommand(){ }

        protected HostPatchHandler HostPatchHandler {
            get {
                if(hostPatchHandler == null) {
                    hostPatchHandler = FindAnyObjectByType<HostPatchHandler>();
                }
                return hostPatchHandler;
            }
            set {
                hostPatchHandler = value;
            }
        }
    }
}
