using System;
using Zoroiscrying.ScriptableObjectCore;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Zoroiscrying.CoreGameSystems.UISystem
{
    /// <summary>
    /// The panel controller takes the responsibility of handling how the data is presented and updated.
    /// If the data has been changed, the callback function would response as 'ReactToDataChange' function.
    /// This class will also handle the change of the data from other systems and player interactions.
    ///     For example, this controller may have additional events to listen to and react.
    /// </summary>
    public abstract class BaseUiPanelController<TD> : MonoBehaviour
    {
        [SerializeField] private BaseVariableSO<TD> dataSO;

        /// <summary>
        /// Auto register callback function to update UI.
        /// </summary>
        protected virtual void OnEnable()
        {
            dataSO.RegisterChangeValueListener(ReactToDataChange);
        }
        
        /// <summary>
        /// Auto register callback function to update UI.
        /// </summary>
        protected virtual void OnDisable()
        {
            dataSO.UnRegisterChangeValueListener(ReactToDataChange);
        }

        protected abstract void ReactToDataChange(TD data);
    }
}