請先下載以下 package
com.unity.netcode.gameobjects: 2.2.0 or newer (Netcode for GameObjects)
com.unity.transport: 2.4.0 or newer(Unity Transport)
TextMeshPro, 並且匯入 essentials

使用方法：
參考 ExampleScene(in the folder "Samples") 建立場景
1. 在初始場景中加入 Prefab "NetConnectManager"(其他場景不添加)

2.為避免 command 相衝突以及意外使用到，所有要傳遞給 client 的指令都需要繼承 IPatchCommand

3. 在使用連線傳遞／接收指令的場合
分別建立繼承 "CommandExecutor" 跟 "CommandSender" 的 script，並放置於場景(可以多個)
(需要 override 方法時，請閱讀完 CommandExecutorExample, CommandSenderExample & OnConnectEventBehaviour 的程式碼)

4. 為了避免重複的註冊程式碼跟方便管理，需要在連線/斷線時呼叫函數的 monobehaviour class 必須繼承 OnConnectEventBehaviour

5. 右鍵 > Create/Wayne Network/Create Netcode Config 建立 config，並參考 NetcodeConfig.cs 內的註解來設定 properties

備註：Menu > NetcodeImplment > Set Config 可以指定一個 config 到最上層，可以參考 SetConfigWindow.cs 來製作符合專案用途的 build script