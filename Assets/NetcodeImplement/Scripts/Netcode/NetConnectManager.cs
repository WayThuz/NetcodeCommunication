using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;


namespace Wayne.Network.NetcodeImplement {
    /// <summary>
    /// 僅負責連線、斷線重連行為的 Manager
    /// </summary>
    [RequireComponent(typeof(UnityTransport))]
    public class NetConnectManager : MonoBehaviour {
        private static NetConnectManager instance;
        public static NetConnectManager Instance {
            get {
                if(instance == null) {
                    instance = FindAnyObjectByType<NetConnectManager>();
                    if(instance == null) {
                        var o = new GameObject("NetConnectManager");
                        DontDestroyOnLoad(o);
                        instance = o.AddComponent<NetConnectManager>();
                    }
                }
                return instance;
            }
        }

        [SerializeField] private GameObject player = null;
        [SerializeField] private List<NetcodeConfig> netcodeConfigs = new();

        private NetStatusManager statusManager;
        private INetConnector connector;

        void Awake() {
            Init();
            if(Config.directConnect) {
                StartCoroutine(TryToConnect());
            }
        }

        private void Init() {
            Config = GetCompatibleConfig();
            string ip = Config.IP;
            if(Config.character == NetcodeConfig.Character.Host) {
                connector = new HostConnector(GetComponent<UnityTransport>(), ip, player);
            }
            else if(Config.character == NetcodeConfig.Character.Client) {
                connector = new ClientConnector(ip, GetComponent<UnityTransport>());
            }
            else {
                throw new NotImplementedException($"NetConnector of type {Config} is not implemented.");
            }
        }

        public void ConnectManually() {
            if(Config == null || Config.character == NetcodeConfig.Character.None || Config.directConnect) return;
            StartCoroutine(TryToConnect());
        }

        private IEnumerator TryToConnect() {
            yield return new WaitUntil(() => {
                var net = NetworkManager.Singleton;
                return net == null || !(net.ShutdownInProgress || net.IsListening || net.IsServer || net.IsHost || net.IsClient);
            });
            yield return new WaitForEndOfFrame();
            Connect();
        }

        #if DEVELOPMENT_TEST
        void Update() {
            if(Input.GetKeyUp(KeyCode.J)) {
                connector.Disconnect();
            }
        }
        #endif

        private void Connect() {
            Debug.Log("Connect");
            if(Config == null) {
                Debug.LogError("Config is null.");
                return;
            }
            NetworkManager.Singleton.OnConnectionEvent += OnLocalDeviceConnectionEvent;
            connector.Connect();
            StatusManager.StartStatusManage(Config);
        }

        private void OnLocalDeviceConnectionEvent(NetworkManager manager, ConnectionEventData data) {
            Debug.Log($"{data.ClientId} {data.EventType}");
            //NetEventPreregisterHandler.NetEvent 應當在自己連線狀態改變時才會執行
            if(Config.character == NetcodeConfig.Character.Host && data.ClientId != NetworkManager.Singleton.LocalClientId) return;
            if(data.EventType == ConnectionEvent.ClientConnected) {
                NetEventPreregisterHandler.InvokeNetEvent(NetEventType.Connect, Config);
            }
            else if(data.EventType == ConnectionEvent.ClientDisconnected) {
                NetEventPreregisterHandler.InvokeNetEvent(NetEventType.Disconnect, Config);
                NetworkManager.Singleton.OnConnectionEvent -= OnLocalDeviceConnectionEvent;
                Reconnect();
            }
        }

        private async void Reconnect() {
            Debug.Log("Wait for fully shutdown");
            while(NetworkManager.Singleton.ShutdownInProgress) {
                await Task.Delay(50);
            }
            Debug.Log("Start Reconnect");
            try {
                Connect();
            }
            catch(Exception e) {
                Debug.LogException(e);
                await Task.Delay(50);
                Reconnect();
            }
        }

        private NetcodeConfig GetCompatibleConfig() {
            for(int i = 0; i < netcodeConfigs.Count; i++) {
                if(netcodeConfigs[i].platform != Application.platform) continue;
                return netcodeConfigs[i];
            }
            return null;
        }

        public void RearrangeConfig(NetcodeConfig config) {
            if(netcodeConfigs.Contains(config)) {
                netcodeConfigs.Remove(config);
            }
            netcodeConfigs.Insert(0, config);
            Debug.Log($"Set netcode config as {config.name}");
        }

        void OnApplicationQuit() => connector.Disconnect();

        /// <summary>
        /// NetcodeManager.IsHost/IsClient/IsServer 在場景切換時會出現問題，無法正確判斷，故使用 config 來判斷
        /// </summary>
        public NetcodeConfig Config { get; private set; }

        private NetStatusManager StatusManager {
            get {
                if(!statusManager) {
                    statusManager = GetComponent<NetStatusManager>();
                    if(!statusManager) {
                        statusManager = gameObject.AddComponent<NetStatusManager>();
                    }
                }
                return statusManager;
            }
        }
    }
}