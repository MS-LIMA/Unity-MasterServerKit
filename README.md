# Master Server Kit

## I. Abstract
When developing a dedicated multiplayer game, the master server is needed which orchestrates clients and rooms. Master server should manage player and room connection, and also list rooms around the world. 

This archive provides simple master server which meets basic features, such as creating rooms and share room lists over the network. Players are able to join master server and can create rooms in lobby and also join other player's room.

All processes are executed in one machine. **Master Server Kit** will execute unity's server build instances in the same machine and orchestrates them to the network. Master server will also use unity server build which enables fast production.

## II. Set Up
### 1. Master Server
#### a. Settings
In `MasterServerKit/Config/Resources`, there is a `MskConfig` file which has config variables inside. Some variables should be configured before build the master server.

- `Master Server Ip` : master server ip connects to
- `Master Server Port` : master server port connects to|
- `Max Connections` : maximum connections to master server|


#### b. Starting Master Server
First of all, create an empty mono script and declare `MasterServerKit.Master` above.

```csharp
using MasterServerKit.Master;
```
And simply call `MskMaster.Start();` to start master server.

```csharp
using MasterServerKit.Master;

public class MasterControl : MonoBehaviour
{
    private void Start()
    {
        MskMaster.Start();
    }
}
```

#### c. Building Master Server

Go to the build settings and change platform to the **Dedicated Server**. Click build and select the folder build files are located to.

<img src="https://github.com/MS-LIMA/Unity-MasterServerKit/blob/main/Screenshots/1.png"  width="400" height="400"/>


Start exe file. If there is a log which says `-> Master server started on "ip":"port"`, the master server has been successfully executed.

<img src="https://github.com/MS-LIMA/Unity-MasterServerKit/blob/main/Screenshots/1.png"  width="500" height="300"/>


### 2. Server Instance
#### a. Settings
In `MasterServerKit/Config/Resources`, there is a `MskConfig` file which has config variables inside. Some variables should be configured before build the server instance.
- `Server Instance Ip` : server instance ip connects to.
- `Server Instance Port Start` : server instance's first port connects to.
- `Max Instance Count` : maximum number of server instances.
- `Room Numberes Per Lobby` : room numbers of lobby.

`Server Instance Port Start` = `25000` and `Max Instance Count` = `100` means that first created server instance will have port 25000 and the last created server will have 25099. If maximum instance count is reached, the next spawn request will be denied. The port will be automatically enqeued when the server instance is terminated.

If server instance is created without given room name, master server will automatically assign room number from `0` to `Room Numberes Per Lobby`. If maximum room numbers per lobby reached, the next spawn reuqest will be denied. The room number will be automatically enqueued when the server instance is terminated.

- `Server Instance Path` : path of the server instance build.
- `Instance Exe Name` : executable server instance name.
- `Use Version in Instance Path` : is version name is used in the path?

If `Server Instance Path` = `C:\Users\MSLima\Desktop\TestMasterServer` and `Instance Exe Name` = `Build.exe`, then the master server will execute server instance in `C:/Users/MSLima/Desktop/TestMasterServer/Build.exe`. 

Clients will join lobby and request spawning server instance by their Master Server version which is declared in `MskConfig`. `Use Version in Instance Path` is used to execute the server instances for different versions.

If `Use Version in Instance Path` is set to true and client requested spawning server instance with version `0.0.1`, then master server will execute server instance in `C:/Users/MSLima/Desktop/TestMasterServer/0.0.1/Build.exe`.



#### b. Starting Server Instance
First of all, create an empty mono script and declare `MasterServerKit` above and inherit from `MskInstanceBehaviour`. `MskInstanceBehaviour` contains various server callbacks which is useful to control the server logic.

```csharp
using MasterServerKit;

public class InstanceControl : MskInstanceBehaviour
{
}
```

And simply call the method below to connect to the master server. Master server ip and port should be passed as arguments.
```csharp
private void Start()
{
    MskInstance.ConnectToMasterServer("127.0.0.1", 5000);
}
```

Write override `OnConnectedToMaster` and `OnConnectedToLobby`, and call `MskInstance.ConnectToLobby();` to connect to the lobby. Connection to lobby is required to execute server instance and register the room.

When connecting to lobby success, call `MskInstance.RegisterRoom();` to register current room to the master server.  `OnRoomRegistered()` will be invoked when registering room is success.

```csharp
    public override void OnConnectedToMaster()
    {
        MskInstance.ConnectToLobby();
    }

    public override void OnConnectedToLobby()
    {
        MskInstance.RegisterRoom();
    }

    public override void OnRoomRegistered()
    {
		Debug.Log("Registering room success!");
    }
```

Before starting the server instance, time to live variables can be set. These variable decides the server instance should be terminated.
- `MskInstance.TtlUntilFirstPlayer` : If room is registered, the server instance will be automatically shut down after this seconds when first player has not connected.

- `MskInstance.TtlEmptyRoom` : If room is empty, the server instance will be automatically shut down after this seconds when last player has left room.

#### c. Building Server Instance

Go to the build settings and change platform to the **Dedicated Server**. Click build and select the folder build files are located to.

Server instance always be started by client side APIs, should not be executed manually. 

### 3. Client
#### a. Settings
In `MasterServerKit/Config/Resources`, there is a `MskConfig` file which has config variables inside. Some variables should be configured before build the master server.

- `Version` : version assigned to the lobby which distinguished players.
- `Master Server Ip` : master server ip connects to
- `Master Server Port` : master server port connects to|

