using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.SaveLoadSystem
{
    /// <summary>
    /// Saved instances are objects that should respawn when they are not destroyed.
    /// </summary>
    [AddComponentMenu("")]
    public class SavedInstance : MonoBehaviour
    {
        private SaveInstanceManager instanceManager;
        private SavableBehavior savable;

        // By default, when destroyed, the saved instance will wipe itself from existance.
        private bool removeData = true;

        public void Configure(SavableBehavior savable, SaveInstanceManager instanceManager)
        {
            this.savable = savable;
            this.instanceManager = instanceManager;
        }

        public void Destroy()
        {
            savable.ManualSaveLoad = true;
            removeData = false;
            SaveSystemPersistentManager.RemoveListener(savable);
            Destroy(this.gameObject);
        }

        private void OnDestroy()
        {
            if (SaveSystemPersistentManager.DeactivatedObjectExplicitly(this.gameObject))
            {
                if (removeData)
                {
                    SaveSystemPersistentManager.WipeSaveable(savable);
                    instanceManager.DestroyObject(this, savable);
                }
            }
        }
    }
}