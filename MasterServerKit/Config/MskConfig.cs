using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MskConfig", menuName = "Master Server kit/Create Config", order = int.MaxValue)]
public class MskConfig : ScriptableObject
{
    [Header("Config")]
    public string version = "";


    [Header("Master Server Config")]
    public string masterServerIp = "127.0.0.1";
    public ushort masterServerPort = 5000;
    public int maxConnections = 100;


    [Header("Server Instance Config")]
    public string serverInstanceIp = "127.0.0.1";
    public ushort serverInstancePortStart = 25000;

    [Space]
    public ushort maxInstanceCount = 100;
    public ushort roomNumbersPerLobby = 100;

    [Space]
    public string serverInstancePath = "";
    public string instanceExeName = "";

    public bool useVersionInInstancePath = false;


    public static MskConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<MskConfig>("MskConfig");
            }

            return instance;
        }
    }

    private static MskConfig instance;

}
