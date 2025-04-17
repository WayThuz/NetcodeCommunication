# A Host-Client Network System base on Unity Netcode
Can work with small multi-player games.(best for < 50 player)

# How to use?
Add Prefab "NetConnectManager" in your Unity start scene.
Implement CommandExecutor.cs and CommandSender.cs as you wish.(Please check CommandExecutorExample.cs and CommandSenderExample.cs)

# How do I start?
1. You can check ExampleScene inside Assets/NetcodeImplement/Samples to get more information about this system.
2. Adjust order of NetcodeConfigs in script NetConnectManager(GameObject: "NetConnectManager") of ExampleScene. (NetcodeConfig.cs will give you more information)
3. Build ExampleScene as Host/Client, Unity Editor can also perform as Host/Client

    Host: Editor
    Client: Application instance 0, Application instance 1, Application instance 2.......

    or

    Host: Host Application instance 0
    Client: Editor, Client Application instance 0, Client Application instance 1,  Client Application instance 2.......
    

4. Open Host app/editor
5. Open Client app/editor
