using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wayne.Network.NetcodeImplement {
    [CreateAssetMenu(fileName = "NetcodeConfig", menuName = "Wayne Network/Create Netcode Config")]
    public class NetcodeConfig : ScriptableObject {
        public enum Character {
            None,
            Client,
            Host,
        }

        public enum IPSource {
            /// <summary>
            /// 以 config 內的 manualIP 來連線
            /// </summary>
            Manual,

            /// <summary>
            /// 根據預設資料夾 (Application.persistentDataPath or Application.streamingAssetsPath) 內的 ip.txt 來決定連線 IP
            /// </summary>
            File,

            /// <summary>
            /// 使用預設 Local 路徑(127.0.0.1)
            /// </summary>
            Default
        }

        public RuntimePlatform platform;
        public Character character;

        public IPSource ipSource = IPSource.Manual;
        public string manualIp;

        public bool directConnect = false;

        public string IP {
            get {
                if(ipSource == IPSource.Default) return DefaultIP;
                if(ipSource == IPSource.Manual) return manualIp;
                string root = Application.platform == RuntimePlatform.Android ? Application.persistentDataPath : Application.streamingAssetsPath;
                string fullPath = Path.Combine(root, "ip.txt");
                string line = File.ReadAllText(fullPath);
                return line;
            }
        }

        public string DefaultIP => "127.0.0.1";
    }
}
