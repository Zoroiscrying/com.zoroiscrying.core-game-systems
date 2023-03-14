using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.CoreSystemBase
{
    public abstract class BaseSceneExclusiveManager<T> : MonoBehaviour where T : BaseSceneExclusiveManager<T>
    {
        private static T m_Instance = null;

        //public static bool isTemporaryInstance { private set; get; }
        
        private static bool m_isInitialized;
        
        private void OnEnable()
        {
            if (m_Instance != null)
            {
                Debug.LogError($"Another Persistent Manager of type {typeof(T).ToString()} " +
                               $"already existed in this scene. Trying to destroy this object");
                DestroyImmediate(this);
                return;
            }
            else
            {
                m_Instance = this as T;
            }
            
            if (!m_isInitialized) {
                //DontDestroyOnLoad(this.gameObject);
                m_isInitialized = true;
                m_Instance.InitManager();
            }
        }

        /// <summary>
        /// This function is called when the instance is used the first time
        /// Put all the initializations you need here, as you would do in Awake
        /// </summary>
        protected abstract void InitManager();
        
        /// Make sure the instance isn't referenced anymore when the user quit, just in case.
        private void OnApplicationQuit()
        {
            m_Instance = null;
        }
    }
}