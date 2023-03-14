using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zoroiscrying.CoreGameSystems.SaveLoadSystem
{
    /// <summary>
    /// Container for all savable data in a game save data slot.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>
        /// Meta data is the relevant data of the 'GameSaveData'.
        /// </summary>
        [Serializable]
        public struct MetaData
        {
            public int gameVersion;
            public string creationDate;
            public string timePlayed;
        }
        
        [Serializable]
        public struct Data
        {
            public string guid;
            public string data;
            public string scene;
        }
        
        [NonSerialized] public TimeSpan timePlayed;
        [NonSerialized] public int gameVersion;
        [NonSerialized] public DateTime creationDate;
        
        [SerializeField] private MetaData metaData;
        
        [SerializeField] private List<Data> saveData = new List<Data>();
        
        /// <summary>
        /// Indexes are stored in dictionary for quick lookup, the key is the string guid of the data
        /// </summary>
        [NonSerialized]
        private Dictionary<string, int> _saveDataCache = new Dictionary<string, int>(StringComparer.Ordinal);
        
        [NonSerialized] private bool _loaded;
        
        /// <summary>
        /// Used to track which save guids are assigned to a specific scene (key);
        /// This makes it possible to wipe all data from a specific scene;
        /// </summary>
        [NonSerialized] private Dictionary<string, List<string>> _sceneObjectIds = new Dictionary<string, List<string>>();

        /// <summary>
        /// Update metadata when writing data onto the disk.
        /// </summary>
        public void OnWrite()
        {
            if (creationDate == default(DateTime))
            {
                creationDate = DateTime.Now;
            }

            metaData.creationDate = creationDate.ToString(CultureInfo.InvariantCulture);
            metaData.gameVersion = gameVersion;
            metaData.timePlayed = timePlayed.ToString();
        }
        
        /// <summary>
        /// 1. Initialize runtime metadata.
        /// 2. Clear the empty data on load.
        /// </summary>
        public void OnLoad()
        {
            gameVersion = metaData.gameVersion;

            DateTime.TryParse(metaData.creationDate, out creationDate);
            TimeSpan.TryParse(metaData.timePlayed, out timePlayed);

            if (saveData.Count > 0)
            {
                // Clear all empty data on load.
                int dataCount = saveData.Count;
                for (int i = dataCount - 1; i >= 0; i--)
                {
                    if (string.IsNullOrEmpty(saveData[i].data))
                        saveData.RemoveAt(i);
                }

                for (int i = 0; i < saveData.Count; i++)
                {
                    _saveDataCache.Add(saveData[i].guid, i);
                    AddSceneID(saveData[i].scene, saveData[i].guid);
                }
            }
        }
        
        /// <summary>
        /// Wipe the saved data in a specific scene
        /// </summary>
        /// <param name="sceneName">The string name of the scene</param>
        public void WipeSceneData(string sceneName)
        {
            List<string> value;
            if (_sceneObjectIds.TryGetValue(sceneName, out value))
            {
                int elementCount = value.Count;
                for (int i = elementCount - 1; i >= 0; i--)
                {
                    Remove(value[i]);
                    value.RemoveAt(i);
                }
            }
            else
            {
                Debug.Log("Scene is already wiped!");
            }
        }
        
        /// <summary>
        /// Wipe the saved data in a specific scene
        /// </summary>
        /// <param name="sceneIndex">The index of the built scene</param>
        public void WipeSceneData(int sceneIndex)
        {
            var scene = SceneManager.GetSceneByBuildIndex(sceneIndex);
            if (scene.IsValid())
            {
                WipeSceneData(scene.name);   
            }
            else
            {
                Debug.Log("Scene Index is not valid.");
            }
        }
        
        /// <summary>
        /// Remove the data associated with the specific guid.
        /// </summary>
        /// <param name="id"></param>
        public void Remove(string id)
        {
            if (_saveDataCache.TryGetValue(id, out var saveIndex))
            {
                // Zero out the string data, it will be wiped on next cache initialization.
                saveData[saveIndex] = new Data();
                _saveDataCache.Remove(id);
                _sceneObjectIds.Remove(id);
            }
        }
        
        /// <summary>
        /// Assign any data to the given ID. If data is already present within the ID, then it will be overwritten.
        /// </summary>
        /// <param name="id"> Save Identification </param>
        /// <param name="data"> Data in a string format </param>
        /// <param name="scene"> Data in a string format </param>
        public void Set(string id, string data, string scene)
        {
            if (_saveDataCache.TryGetValue(id, out var saveIndex))
            {
                saveData[saveIndex] = new Data() { guid = id, data = data, scene = scene };
            }
            else
            {
                Data newSaveData = new Data() { guid = id, data = data, scene = scene };

                saveData.Add(newSaveData);
                _saveDataCache.Add(id, saveData.Count - 1);
                AddSceneID(scene, id);
            }
        }
        
        /// <summary>
        /// Returns any data stored based on an string id identifier
        /// </summary>
        /// <param name="id"> Save Identification </param>
        /// <returns></returns>
        public string Get(string id)
        {
            if (_saveDataCache.TryGetValue(id, out var saveIndex))
            {
                return saveData[saveIndex].data;
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Adds the index to a list that is identifiable by scene
        /// Makes it easy to remove save data related to a scene name.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="id"></param>
        private void AddSceneID(string scene, string id)
        {
            if (_sceneObjectIds.TryGetValue(scene, out var sceneGuids))
            {
                sceneGuids.Add(id);
            }
            else
            {
                List<string> newSceneGuids = new List<string>();
                newSceneGuids.Add(id);
                _sceneObjectIds.Add(scene, newSceneGuids);
            }
        }
        
    }
}