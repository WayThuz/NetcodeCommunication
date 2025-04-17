using UnityEngine;

namespace Wayne.Network.NetcodeImplement {
    public class CommandExecutor : NetEventListenerBase, IPatchCommandExecutor {
        private ClientPatchHandler clientPatchHandler;
        private bool isInited = false;

        /// <summary>
        /// 假如在連線狀態下重新回到場景，需要透過此方式初始化
        /// </summary>
        void Start() {
            if(!isInited) {
                Init(NetConnectManager.Instance.Config);
            }
        }


        /// <summary>
        /// 初次連線時需要透過此方式初始化
        /// </summary>
        protected override void OnConnect(NetcodeConfig config) {
            if(!isInited) {
                Init(config);
            }
        }

        protected virtual void Init(NetcodeConfig config) {
            if(config == null || !ClientPatchHandler) return;
            isInited = true;
            if(config.character == NetcodeConfig.Character.Client) {
                ClientPatchHandler.RegisterExecutor(this);
            }
            else {
                enabled = false;
            }
        }

        protected override void OnDisconnect(NetcodeConfig config) { }

        /// <summary>
        /// Client 端的 IPatchCommandExecutor 會對 Host 傳遞過來的 patch 做驗證(ClientPatchHandler.ExeHardPatchCommand)
        /// 例如可以驗證是否為需要的 command type
        /// 驗證成功才執行 command
        /// </summary>
        /// <param name="patch"></param>
        /// <returns></returns>
        public virtual bool Verify(Patch patch) => true;

        /// <summary>
        /// IPatchCommandExecutor 在驗證完成後會執行 command
        /// </summary>
        /// <param name="patch"></param>
        public virtual void ExeCommand(Patch patch) => Debug.Log("ExeCommand");

        protected ClientPatchHandler ClientPatchHandler {
            get { 
                if(clientPatchHandler == null) {
                    clientPatchHandler = FindAnyObjectByType<ClientPatchHandler>();
                }
                return clientPatchHandler; 
            }
            set {
                clientPatchHandler = value;
            }
        }

        protected override void OnDestroy() {
            ClientPatchHandler.DeregisterExecutor(this);
            base.OnDestroy();
        }
    }
}