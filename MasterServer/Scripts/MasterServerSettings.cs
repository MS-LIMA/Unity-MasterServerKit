using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "MasterServerSettings", menuName = "MasterServerSettings", order = 1)]
public class MasterServerSettings : ScriptableObject
{
    public string ip;
    public string port;

    public bool enableLogging;
}

