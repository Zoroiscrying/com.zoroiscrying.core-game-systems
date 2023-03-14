using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.SaveLoadSystem
{
    /// <summary>
    /// Attach this to the root of an object that you want to save
    /// </summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(-9001)]
    [AddComponentMenu("Saving/SavableBehavior")]
    public class SavableBehavior : MonoBehaviour
    {
        [Header("Save configuration")]
        [SerializeField, Tooltip("Will never allow the object to load data more then once." +
                                 "this is useful for persistent game objects.")]
        private bool loadOnce = false;
        
        [SerializeField, Tooltip("Save and Load will not be called by the Save System." +
                                 "this is useful for displaying data from a different save file")]
        private bool manualSaveLoad;
        
        [SerializeField, Tooltip("It will scan other objects for ISavable components")]
        private List<GameObject> externalListeners = new List<GameObject>();
        
        [SerializeField, HideInInspector]
        private List<CachedSavableComponent> cachedSavableComponents = new List<CachedSavableComponent>();

        private List<string> _savableComponentIDs = new List<string>();
        private List<ISavable> _savableComponentObjects = new List<ISavable>();

        
        [SerializeField] private string saveIdentification;
        
        public string SaveIdentification
        {
            get
            {
                return saveIdentification;
            }
            set
            {
                saveIdentification = value;
                hasIdentification = !string.IsNullOrEmpty(saveIdentification);
            }
        }
        
        private bool hasLoaded;
        private bool hasStateReset;
        private bool hasIdentification;
        
        /// <summary>
        /// Means of storing all savable components for the ISavable component.
        /// </summary>
        [System.Serializable]
        public class CachedSavableComponent
        {
            public string identifier;
            public MonoBehaviour monoBehaviour;
        }

        public bool ManualSaveLoad
        {
            get => manualSaveLoad;
            set => manualSaveLoad = value;
        }
        
#if UNITY_EDITOR
        
        /// <summary>
        /// Used to check if you are duplicating an object. If so, it assigns a new identification
        /// </summary>
        private static Dictionary<string, SavableBehavior> saveIdentificationCache = new Dictionary<string, SavableBehavior>();
        
        /// <summary>
        /// Used to prevent duplicating GUIDS when you copy a scene.
        /// </summary>
        [HideInInspector] [SerializeField] private string sceneName;

        /// <summary>
        /// Set the identifier of a cached savable component
        /// </summary>
        /// <param name="index"></param>
        /// <param name="identifier"></param>
        private void SetIdentification(int index, string identifier)
        {
            cachedSavableComponents[index].identifier = identifier;
        }
        
        /// <summary>
        /// Validate the global Savable Behaviors as well as each Savable Behavior's cachedISavableComponents' ids.
        /// </summary>
        public void OnValidate()
        {
            // This is a editor-only operation
            if (Application.isPlaying)
                return;
            
            // Check if this object is a prefab.
            bool isPrefab;

#if UNITY_2018_3_OR_NEWER 
            isPrefab = UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this.gameObject);
#else
            isPrefab = this.gameObject.scene.name == null;
#endif

            // Set a new save identification if it is not a prefab at the moment.
            if (!isPrefab)
            {
                ValidateHierarchy.Add(this);

                bool isDuplicate = false;
                SavableBehavior savable = null;

                // update the scene name of the object
                if (sceneName != gameObject.scene.name)
                {
                    UnityEditor.Undo.RecordObject(this, "Updated Object Scene ID");

                    if (SaveSettings.Get().resetSavableIdOnNewScene)
                    {
                        saveIdentification = "";
                    }

                    sceneName = gameObject.scene.name;
                }

                
                if (SaveSettings.Get().resetSavableIdOnDuplicate)
                {
                    // Does the object have a valid save id? If not, we give a new one.
                    if (!string.IsNullOrEmpty(saveIdentification))
                    {
                        isDuplicate = saveIdentificationCache.TryGetValue(saveIdentification, out savable);

                        // This is the first appeared object with this identification
                        if (!isDuplicate)
                        {
                            if (saveIdentification != "")
                            {
                                saveIdentificationCache.Add(saveIdentification, this);
                            }
                        }
                        else // This object's id has already existed
                        {
                            // The cached savable is destroyed or something, update it to this one
                            if (savable == null)
                            {
                                saveIdentificationCache.Remove(saveIdentification);
                                saveIdentificationCache.Add(saveIdentification, this);
                            }
                            else // Update this object's id
                            {
                                if (savable.gameObject != this.gameObject)
                                {
                                    UnityEditor.Undo.RecordObject(this, "Updated Object Scene ID");
                                    saveIdentification = "";
                                }
                            }
                        }
                    }
                    // end if
                }

                // It's time to update the id of this object
                if (string.IsNullOrEmpty(saveIdentification))
                {
                    UnityEditor.Undo.RecordObject(this, "ClearedSaveIdentification");

                    int guidLength = SaveSettings.Get().gameObjectGuidLength;

#if NET_4_6
                    saveIdentification = $"{gameObject.scene.name}-{gameObject.name}-{System.Guid.NewGuid().ToString().Substring(0, guidLength)}";
#else
                    saveIdentification = string.Format("{0}-{1}-{2}", gameObject.scene.name, gameObject.name, System.Guid.NewGuid().ToString().Substring(0, guidLength));
#endif
                    saveIdentificationCache.Add(saveIdentification, this);
                    
                    // The scene has been changed
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(this.gameObject.scene);
                }
            }
            else // This object is a prefab, we don't want to store the prefab, only the instantiated prefabs
            {
                saveIdentification = string.Empty;
                sceneName = string.Empty;
            }

            // Get the Savables in the child gameobjects (and this gameobject).
            List<ISavable> obtainSavables = new List<ISavable>();
            obtainSavables.AddRange(GetComponentsInChildren<ISavable>(true).ToList());
            // Add external listeners to savable list too.
            for (int i = 0; i < externalListeners.Count; i++)
            {
                if (externalListeners[i] != null)
                    obtainSavables.AddRange(externalListeners[i].GetComponentsInChildren<ISavable>(true).ToList());
            }

            // Clear the cached Savable components list of all the null monobehaviors
            for (int i = cachedSavableComponents.Count - 1; i >= 0; i--)
            {
                if (cachedSavableComponents[i].monoBehaviour == null)
                {
                    cachedSavableComponents.RemoveAt(i);
                }
            }

            // Compare the editor-runtime obtained savables and update the cached savable components
            if (obtainSavables.Count != cachedSavableComponents.Count)
            {
                // Should remove the cached savables
                if (cachedSavableComponents.Count > obtainSavables.Count)
                {
                    for (int i = cachedSavableComponents.Count - 1; i >= obtainSavables.Count; i--)
                    {
                        cachedSavableComponents.RemoveAt(i);
                    }
                }

                // Clear the savables again
                int saveableComponentCount = cachedSavableComponents.Count;
                for (int i = saveableComponentCount - 1; i >= 0; i--)
                {
                    if (cachedSavableComponents[i] == null)
                    {
                        cachedSavableComponents.RemoveAt(i);
                    }
                }
                
                ISavable[] cachedSaveables = new ISavable[cachedSavableComponents.Count];
                for (int i = 0; i < cachedSaveables.Length; i++)
                {
                    cachedSaveables[i] = cachedSavableComponents[i].monoBehaviour as ISavable;
                }

                // Check the missing elements of the cached ISavables compared to the obtained savables 
                ISavable[] missingElements = obtainSavables.Except(cachedSaveables).ToArray();

                // Add new ISavable components to the cached list
                for (int i = 0; i < missingElements.Length; i++)
                {
                    CachedSavableComponent newSaveableComponent = new CachedSavableComponent()
                    {
                        monoBehaviour = missingElements[i] as MonoBehaviour
                    };

                    string typeString = newSaveableComponent.monoBehaviour.GetType().Name.ToString();

                    var identifier = "";
                    
                    // Create a new identifier for the savable component
                    while (!IsIdentifierUnique(identifier))
                    {
                        int guidLength = SaveSettings.Get().componentGuidLength;
                        string guidString = System.Guid.NewGuid().ToString().Substring(0, guidLength);
                        identifier = string.Format("{0}-{1}", typeString, guidString);
                    }

                    newSaveableComponent.identifier = identifier;

                    cachedSavableComponents.Add(newSaveableComponent);
                }

                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(this.gameObject.scene);
            }
        }
        
        /// <summary>
        /// Check if the identifier is unique in the 'CachedSavableComponent' list.
        /// </summary>
        /// <param name="identifier">The identifier of the ISavable component</param>
        /// <returns></returns>
        private bool IsIdentifierUnique(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            for (int i = 0; i < cachedSavableComponents.Count; i++)
            {
                if (cachedSavableComponents[i].identifier == identifier)
                {
                    return false;
                }
            }

            return true;
        }
        
        public void Refresh()
        {
            OnValidate();
        }
        
#endif
        
        /// <summary>
        /// Gets and adds savable components in Children. This is only required when you want to
        /// create gameobjects dynamically through C#. Keep in mind that changing the component add order
        /// will change the way it gets loaded.
        /// </summary>
        public void ScanAddSavableComponents()
        {
            ISavable[] saveables = GetComponentsInChildren<ISavable>();

            for (int i = 0; i < saveables.Length; i++)
            {
                string mono = (saveables[i] as MonoBehaviour).name;

                AddSavableComponent(string.Format("Dyn-{0}-{1}", mono, i.ToString()), saveables[i]);
            }

            // Load it again, to ensure all ISaveable interfaces are updated.
            SaveSystemPersistentManager.ReloadListener(this);
        }
        
        /// <summary>
        /// Useful if you want to dynamically add a savable component. To ensure it 
        /// gets registered.
        /// </summary>
        /// <param name="identifier">The identifier for the component, this is the adress the data will be loaded from </param>
        /// <param name="iSaveable">The interface reference on the component. </param>
        /// <param name="reloadData">Do you want to reload the data on all the components? 
        /// Only call this if you add one component. Else call SaveMaster.ReloadListener(saveable). </param>
        public void AddSavableComponent(string identifier, ISavable iSaveable, bool reloadData = false)
        {
            _savableComponentIDs.Add(string.Format("{0}-{1}", saveIdentification, identifier));
            _savableComponentObjects.Add(iSaveable);

            if (reloadData)
            {
                // Load it again, to ensure all ISaveable interfaces are updated.
                SaveSystemPersistentManager.ReloadListener(this);
            }
        }
        
        private void Awake()
        {
            // Store the component identifiers into a dictionary for performant retrieval.
            for (int i = 0; i < cachedSavableComponents.Count; i++)
            {
                _savableComponentIDs.Add(string.Format("{0}-{1}", saveIdentification, cachedSavableComponents[i].identifier));
                _savableComponentObjects.Add(cachedSavableComponents[i].monoBehaviour as ISavable);
            }

            if (!manualSaveLoad)
            {
                SaveSystemPersistentManager.AddListener(this);
            }
        }
        
        private void OnDestroy()
        {
            if (!manualSaveLoad)
            {
                SaveSystemPersistentManager.RemoveListener(this);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ValidateHierarchy.Remove(this);
                saveIdentificationCache.Remove(saveIdentification);
            }
#endif
        }
        
        /// <summary>
        /// Removes all save data related to this component.
        /// This is useful for dynamic saved objects. So they get erased
        /// from the save file permanently.
        /// </summary>
        public void WipeData(GameSaveData saveGame)
        {
            int componentCount = _savableComponentIDs.Count;

            for (int i = componentCount - 1; i >= 0; i--)
            {
                saveGame.Remove(_savableComponentIDs[i]);
            }

            // Ensures it doesn't try to save upon destruction.
            manualSaveLoad = true;
            SaveSystemPersistentManager.RemoveListener(this, false);
        }
        
        /// <summary>
        /// Used to reset the savable, as if it was never saved or loaded.
        /// </summary>
        public void ResetState()
        {
            // Since the game uses a new save game, reset loadOnce and hasLoaded
            loadOnce = false;
            hasLoaded = false;
            hasStateReset = true;
        }
        
        /// <summary>
        /// Save the data into GameSaveData, this function is called by the Save System
        /// </summary>
        /// <param name="saveGame"></param>
        public void OnSaveRequest(GameSaveData saveGame)
        {
            if (!hasIdentification)
            {
                return;
            }

            int componentCount = _savableComponentIDs.Count;

            for (int i = componentCount - 1; i >= 0; i--)
            {
                ISavable getSaveable = _savableComponentObjects[i];
                string getIdentification = _savableComponentIDs[i];

                if (getSaveable == null)
                {
                    Debug.Log(string.Format("Failed to save component: {0}. Component is potentially destroyed.", getIdentification));
                    _savableComponentIDs.RemoveAt(i);
                    _savableComponentObjects.RemoveAt(i);
                }
                else
                {
                    // The Savable's state hasn't been reset and the ISavable can be saved
                    if (!hasStateReset && !getSaveable.OnSaveCondition())
                    {
                        continue;
                    }
                    
                    // Save the data to the GameSaveData file
                    string dataString = getSaveable.OnSave();

                    if (!string.IsNullOrEmpty(dataString))
                    {
                        saveGame.Set(getIdentification, dataString, this.gameObject.scene.name);
                    }
                }
            }
            hasStateReset = false;
        }

        /// <summary>
        /// Load the data from the 'GameSaveData', the cached component list's id and behaviors are stored
        /// The ISavable behaviors take care of the data save and load of these behaviors via ids in the cached list.
        /// </summary>
        /// <param name="saveGame"></param>
        public void OnLoadRequest(GameSaveData saveGame)
        {
            if (loadOnce && hasLoaded)
            {
                return;
            }
            else
            {
                // Ensure it only loads once with the loadOnce
                // Parameter
                hasLoaded = true;
                hasIdentification = !string.IsNullOrEmpty(saveIdentification);
            }

            if (!hasIdentification)
            {
                Debug.Log("No identification!");
                return;
            }

            int componentCount = _savableComponentIDs.Count;

            for (int i = componentCount - 1; i >= 0; i--)
            {
                ISavable getSaveable = _savableComponentObjects[i];
                string getIdentification = _savableComponentIDs[i];

                if (getSaveable == null)
                {
                    Debug.Log(string.Format("Failed to load component: {0}. Component is potentially destroyed.",
                        getIdentification));
                    _savableComponentIDs.RemoveAt(i);
                    _savableComponentObjects.RemoveAt(i);
                }
                else
                {
                    string getData = saveGame.Get(_savableComponentIDs[i]);

                    if (!string.IsNullOrEmpty(getData))
                    {
                        getSaveable.OnLoad(getData);
                    }
                }
            }
        }
    }
}