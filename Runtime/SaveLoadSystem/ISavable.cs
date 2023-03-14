namespace Zoroiscrying.CoreGameSystems.SaveLoadSystem
{
    public interface ISavable
    {
        /// <summary>
        /// Called by a Savable component. SaveMaster (request save) 
        /// -> notify to all Savable -> return data to active save file with OnSave()
        /// </summary>
        /// <returns> Data for the save file </returns>
        string OnSave();

        /// <summary>
        /// Called by a Savable component. SaveMaster (request load) 
        /// -> notify to all Savable -> obtain data for this specific component with OnLoad()
        /// </summary>
        /// <param name="data"> Data that gets retrieved from the active save file </param>
        void OnLoad(string data);

        /// <summary>
        /// Returning true will allow the save to occur, else it will skip the save.
        /// This is useful when you want to call OnSave() only when something has actually changed.
        /// </summary>
        bool OnSaveCondition();
    }
    
    public interface ISavable<TD>
    {
        /// <summary>
        /// Called by a Savable component. SaveMaster (request save) 
        /// -> notify to all Savable -> return data to active save file with OnSave()
        /// </summary>
        /// <returns> Data for the save file </returns>
        TD OnSave();

        /// <summary>
        /// Called by a Savable component. SaveMaster (request load) 
        /// -> notify to all Savable -> obtain data for this specific component with OnLoad()
        /// </summary>
        /// <param name="data"> Data that gets retrieved from the active save file </param>
        void OnLoad(TD data);

        /// <summary>
        /// Returning true will allow the save to occur, else it will skip the save.
        /// This is useful when you want to call OnSave() only when something has actually changed.
        /// </summary>
        bool OnSaveCondition();
    }
}