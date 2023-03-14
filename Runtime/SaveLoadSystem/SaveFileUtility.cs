using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.SaveLoadSystem
{
    /// <summary>
    /// Used to create, save and delete the 'GameSaveData's Json version on the disk.
    /// </summary>
    public class SaveFileUtility
    {
        // Saving with WebGL requires a separate DLL, which is included in the plugin.
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void SyncFiles();

    [DllImport("__Internal")]
    private static extern void WindowAlert(string message);
#endif
        
        private static string FileExtensionName => SaveSettings.Get().fileExtensionName;
        private static string GameFileName => SaveSettings.Get().fileName;
        private static bool DebugMode => SaveSettings.Get().showSaveFileUtilityLog;
        
        private static string DataPath =>
            $"{Application.persistentDataPath}/{SaveSettings.Get().fileFolderName}";
        
        private static void Log(string text)
        {
            if (DebugMode)
            {
                Debug.Log(text);
            }
        }
        
        private static Dictionary<int, string> _cachedSavePaths;
        
        /// <summary>
        /// Obtain saved game slot data files in the disk.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> ObtainSavePaths()
        {
            if (_cachedSavePaths != null)
            {
                return _cachedSavePaths;
            }

            Dictionary<int, string> newSavePaths = new Dictionary<int, string>();

            // Create a directory if it doesn't exist yet
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }

            string[] filePaths = Directory.GetFiles(DataPath);

            string[] savePaths = filePaths.Where(path => path.EndsWith(FileExtensionName)).ToArray();

            int pathCount = savePaths.Length;

            for (int i = 0; i < pathCount; i++)
            {
                Log($"Found save file at: {savePaths[i]}");

                string fileName = savePaths[i].Substring(DataPath.Length + GameFileName.Length + 1);

                if (int.TryParse(fileName.Substring(0, fileName.LastIndexOf(".", StringComparison.Ordinal)), out var getSlotNumber))
                {
                    newSavePaths.Add(getSlotNumber, savePaths[i]);
                }
            }

            _cachedSavePaths = newSavePaths;

            return newSavePaths;
        }
        
        /// <summary>
        /// Load the 'GameSaveData' from the local disk files.
        /// </summary>
        /// <param name="savePath"></param>
        /// <returns></returns>
        private static GameSaveData LoadSaveFromPath(string savePath)
        {
            string data = "";

            using (var reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
            {
                data = reader.ReadString();
            }

            if (string.IsNullOrEmpty(data))
            {
                Log($"Save file empty: {savePath}. It will be automatically removed");
                File.Delete(savePath);
                return null;
            }

            //todo::Enable XML / Database support
            GameSaveData getSave = JsonUtility.FromJson<GameSaveData>(data);

            if (getSave != null)
            {
                getSave.OnLoad();
                return getSave;
            }
            else
            {
                Log($"Save file corrupted: {savePath}");
                return null;
            }
        }
        
        /// <summary>
        /// Retrieve used slots in the disk.
        /// </summary>
        /// <returns>A list of slot indexes from 0.</returns>
        public static int[] GetUsedSlots()
        {
            int[] saves = new int[ObtainSavePaths().Count];

            int counter = 0;

            foreach (int item in ObtainSavePaths().Keys)
            {
                saves[counter] = item;
                counter++;
            }

            return saves;
        }
        
        /// <summary>
        /// Retrieve the number of used slots.
        /// </summary>
        /// <returns></returns>
        public static int GetSaveSlotCount()
        {
            return ObtainSavePaths().Count;
        }
        
        /// <summary>
        /// Load the 'GameSaveData' if the file exists in the disk.
        /// </summary>
        /// <param name="slot">The slot index</param>
        /// <param name="createIfEmpty">Create a file if the file does not exist.</param>
        /// <returns></returns>
        public static GameSaveData LoadSave(int slot, bool createIfEmpty = false)
        {
            if (slot < 0)
            {
                Debug.LogWarning("Attempted to load negative slot");
                return null;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
                SyncFiles();
#endif

            if (ObtainSavePaths().TryGetValue(slot, out var savePath))
            {
                GameSaveData saveGame = LoadSaveFromPath(savePath);

                if (saveGame == null)
                {
                    _cachedSavePaths.Remove(slot);
                    return null;
                }

                Log($"Successful load at slot (from cache): {slot}");
                return saveGame;
            }
            else
            {
                if (!createIfEmpty)
                {
                    Log($"Could not load game at slot {slot}");
                }
                else
                {

                    Log($"Creating save at slot {slot}");

                    GameSaveData saveGame = new GameSaveData();

                    WriteSave(saveGame, slot);

                    return saveGame;
                }

                return null;
            }
        }
        
        /// <summary>
        /// Write the 'GameSaveData' to a specific slot, create a file to store the data.
        /// </summary>
        /// <param name="gameSaveData">The data to be saved.</param>
        /// <param name="saveSlot">The slot of the game data.</param>
        public static void WriteSave(GameSaveData gameSaveData, int saveSlot)
        {
            string savePath = $"{DataPath}/{GameFileName}{saveSlot.ToString()}{FileExtensionName}";

            if (!_cachedSavePaths.ContainsKey(saveSlot))
            {
                _cachedSavePaths.Add(saveSlot, savePath);
            }

            Log($"Saving game slot {saveSlot.ToString()} to : {savePath}");

            gameSaveData.OnWrite();

            using (var writer = new BinaryWriter(File.Open(savePath, FileMode.Create)))
            {
                var jsonString = JsonUtility.ToJson(gameSaveData, SaveSettings.Get().useJsonPrettyPrint);

                writer.Write(jsonString);
            }

#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFiles();
#endif
        }
        
        /// <summary>
        /// Delete the saved game data file for a slot.
        /// </summary>
        /// <param name="slot">The slot index.</param>
        public static void DeleteSave(int slot)
        {
            string filePath = $"{DataPath}/{GameFileName}{slot}{FileExtensionName}";

            if (File.Exists(filePath))
            {
                Log($"Succesfully removed file at {filePath}");
                File.Delete(filePath);

                if (_cachedSavePaths.ContainsKey(slot))
                {
                    _cachedSavePaths.Remove(slot);
                }
            }
            else
            {
                Log($"Failed to remove file at {filePath}, no file exists.");
            }

#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFiles();
#endif
        }
        
        /// <summary>
        /// If the slot has data file.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsSlotUsed(int index)
        {
            return ObtainSavePaths().ContainsKey(index);
        }
        
        /// <summary>
        /// Get the unused slots.
        /// </summary>
        /// <returns></returns>
        public static int GetAvailableSaveSlot()
        {
            int slotCount = SaveSettings.Get().maxSaveSlotCount;

            for (int i = 0; i < slotCount; i++)
            {
                if (!ObtainSavePaths().ContainsKey(i))
                {
                    return i;
                }
            }

            return -1;
        }
        
    }
}