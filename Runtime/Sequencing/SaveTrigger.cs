using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class SaveTrigger : MonoBehaviour {
        [SerializeField] private SaveType _type;
        [SerializeField] private string _specificSaveName;

        public void Activate() {
            var ssm = SaveStateManager.Instance;
            if (!ssm) {
                Debug.LogError("SaveTrigger requires SaveStateManager.");
                return;
            }
            
            if (_type == SaveType.AutoSave) {
                SaveStateManager.Instance.AutoSave();
            } else if (_type == SaveType.QuickSave) {
                SaveStateManager.Instance.QuickSave();
            } else if (_type == SaveType.CurrentSave) {
                SaveStateManager.Instance.SaveAll(PersistenceManager.Instance.LoadedStateData.StateName);
            } else {
                SaveStateManager.Instance.SaveAll(_specificSaveName);
            }
        }
        
        private enum SaveType {
            AutoSave, QuickSave, CurrentSave, SpecificSave,
        }
    }
}