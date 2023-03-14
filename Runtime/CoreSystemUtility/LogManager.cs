using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.CoreSystemUtility
{
    public static class LogManager
    {
        public static void Log(string msg)
        {
            Debug.Log(msg);
        }

        public static void LogWarning(string msg)
        {
            Debug.LogWarning(msg);
        }
    }
}