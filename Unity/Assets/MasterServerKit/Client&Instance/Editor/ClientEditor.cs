using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Msk
{
    [InitializeOnLoad]
    public static class ClientEditor
    {
        static ClientEditor()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                if (MasterServerKit.IsConnected)
                {
                    MasterServerKit.Client.Disconnect();
                }
            }
        }
    }
}