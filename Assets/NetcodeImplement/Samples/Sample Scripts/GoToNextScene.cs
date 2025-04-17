using Wayne.Network.NetcodeImplement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GoToNextScene : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private Button button;

    void Start() {
        if(NetConnectManager.Instance.Config.character == NetcodeConfig.Character.Host) {
            button.onClick.AddListener(LoadScene);
        }
        else {
            button.gameObject.SetActive(false);
        }
    }

    private void LoadScene() {
        Debug.Log("Go to scene" +  sceneName);
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
