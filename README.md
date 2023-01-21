# Master Server Kit

## I. Abstract
When developing a dedicated multiplayer game, the master server is needed which orchestrates clients and rooms. Master server should manage player and room connection, and also list rooms around the world. 

This archive provides simple master server which meets basic features, such as creating rooms and share room lists over the network. Players are able to join master server and can create rooms in lobby and also join other player's room.

All processes are executed in one machine. **Master Server Kit** will execute unity's server build instances in the same machine and orchestrates them to the network. Master server will also use unity server build which enables fast production.

## II. Master Server
#### 1. Settings
In `MasterServerKit/Config/Resources`, there is a `MskConfig` file which has config variables inside. Some variables should be configured before build the master server.
|  Variables |  explaination |
| ------------ | ------------ |
|Master Server Ip|Master server ip connect to|
|Master Server Port|Master server port connect to|
|Max Connections|Maximum connections to master server|


#### 2. Starting Master Server
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

#### 3. Building Master Server



