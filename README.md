WORKING IN PROGRESS

# Master Server Kit

## I. Abstract
When developing a dedicated multiplayer game, the master server is needed in order to orchestrates clients and rooms to the network. Master server should manage players and room connections, and also list rooms around the world. 

This archive provides simple master server named **MasterServerKit(MSK)** which meets the basic features, such as creating rooms and sharing room lists over the network. Players are able to connect to the master server, create rooms in a lobby and also join other player's room.

It also provides multiple usefull methods, such as setting custom properties of the room and players, kicking players from the room, change the password of the room, sending messages to the players connected to the lobby and something else.

The rooms which are created by MSK is actually Unity's server instance, which means that MSK is designed for authorative game server model. MSK will orchestrates the unity server instance's ip and port so that other players can join the server instance using various realtime multiplay solutions, such as Mirror Network, NetCode and etc.

## II. Features

- **Room**
Create rooms
Join rooms, join random rooms
Set room's password, open, private, custom properties
Kick a player

- **Client**
Change player's nickname
Set player's custom properties
List rooms
Fetch players connected

And more..
<br>
## III. Setting Up

### 1. Master Server
#### a. Settings
There are two files in the builds directory. You can use the master server for Windows_x64 or Linux_x64. In the directory, there is a file named `config.json` which contains the configuration of the master server. Properly change the properties as the master server desired to be.

- **isHostDNS** : does the host use DNS instead of IPv4?
- **MasterHost** : address of the master server.
- **MasterPort** : port number of the master server.
- **MaxConnectionsToMaster** : maximum connections to the master server includes the players and room clients.
- **DispatchRate** : packet dispatching rate of the server.
- **PortStart** : starting port number of the unity server instance.
- **MaxInstanceCount** : maximum unity server instance count.
- **UseVersionInInstancePath** : MSK will uses the game version to execute the unity server instance.
- **ServerInstancePath** : path of the server instance.
- **InstanceFileName** : executable unity server instance file's name.

`PortStart` = `25000` and `MaxInstanceCount` = `100` mean that first created server instance will have port 25000 and the last created server will have 25099. If maximum instance count is reached, the next spawn request will be denied. The port will be automatically enqeued when the server instance is terminated.

If `ServerInstancePath` = `C:\Users\MSLima\Desktop\ServerInstance` and `InstanceFileName` = `Build.exe`, then the master server will execute a server instance in `C:/Users/MSLima/Desktop/ServerInstance/Build.exe`. 

If `UseVersionInInstancePath` is set to true and client requested spawning server instance with version `0.0.1`, then master server will execute server instance in `C:/Users/MSLima/Desktop/ServerInstance/0.0.1/Build.exe`.

<br>
#### b. Starting Master Server

Simply execute the file named below : 

- Windows : MasterServer.exe
- Linux : MasterServer

If you see the screen below, master server is successfully executed.
<img src="https://github.com/MS-LIMA/Unity-MasterServerKit/blob/main/Screenshots/1.png"  width="400" height="400"/>


<br>
### 2. Client & Server Instance
#### a. Settings
In `MasterServerKit/Config/Resources`, there is a `MskConfigClient` file which has config variables inside. Some variables should be configured before building the client or server instance.

- `Version` : the game's version.
- `DnsForHost` : does the host use DNS instead of IPv4?
- `Host` : address of the master server.
- `Port` : port number of the master server.

<br>
#### b. Starting Server Instance
First of all, create an empty mono script and declare `Msk` above and inherit from `MonoBehaviourMskInstanceCallbacks`. `MonoBehaviourMskInstanceCallbacks` contains various server callbacks which is useful to control the server logic.

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Msk;

public class InstanceConnectionControl : MonoBehaviourMskInstanceCallbacks
{
}

```

And simply call the method below to connect to the master server. Master server ip and port can be passed as arguments. Otherwise, it will use the default ip and port which are set in the `MskConfigClient`.

```csharp
    private void Start()
    {
        MasterServerKit.Instance.ConnectToMaster();
    }

    public override void OnConnectedToMaster()
    {
        // Invoked when connect to the master success.
    }

    public override void OnRoomRegistered()
    {
        // Invoked when the room is registerd to the master server.
    }
```

Note that the room will be provided to players when the room is registered to the master server, not the time when a connection has been establisehd.

<br>
#### c. Starting Client
Smiliar to the starting serverv instance, create an empty mono script and declare `Msk` above and inherit from `MonoBehaviourMskClientCallbacks` which provides server callbacks for the client.

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Msk;

public class ClientConnectionControl : MonoBehaviourMskClientCallbacks
{
}
```
Then, call the method below to connect to the master server. Same as server instance, it will use the default ip and port which are set in the `MskConfigClient` if the ip and port are not provided.

```csharp
    private void Start()
    {
        MasterServerKit.Client.ConnectToMaster();
    }

    public override void OnConnectedToMaster()
    {
        // Invoked when connect to the master success.
    }
```

Once the client has been connected to the master server, various server methods can be used to create game logics.

<br>
## IV. Examples
Examples are included in the UnityPackage. Please import the unity package into your unity project to explore the example situations.



