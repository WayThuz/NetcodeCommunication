using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Wayne.Network.NetcodeImplement;
using System.IO;

/// <summary>
/// Set config Example, 可以結合進 project build function 內，減少設定 config 的步驟
/// </summary>
public class SetConfigWindow : EditorWindow
{
    private static NetcodeConfig netcodeConfig = null;

    [MenuItem("NetcodeImplement/Set Config")]
    private static void CreateWindow() {
        SetConfigWindow window = GetWindow<SetConfigWindow>("Build Window");
        window.Show();
    }

    private void OnGUI() {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Netcode Config: ", netcodeConfig, typeof(NetcodeConfig), false);
        GUI.enabled = true;

        if(GUILayout.Button("Set Netcode Config")) {
            SetNetCodeConfig();
        }
    }

    private void SetNetCodeConfig() {
        var path = EditorUtility.OpenFilePanel("Select netcode config", Path.Combine(Application.dataPath, "ArplanetNetcodeImplement", "NetConfigs"), "asset");
        if(path.StartsWith(Application.dataPath)) {
            var configPath = "Assets" + path[Application.dataPath.Length..];
            netcodeConfig = AssetDatabase.LoadAssetAtPath<NetcodeConfig>(configPath);
        }
        NetConnectManager.Instance.RearrangeConfig(netcodeConfig);
        AssetDatabase.SaveAssets();
    }
}
