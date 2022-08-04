using System;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class SaveStateManager : PersistedSingleton<SaveStateManager, SaveStateManager.StateInfo> {
        [SerializeField] private int _autoSaveCount = 3;
        [SerializeField] private string _autoSaveName = "Autosave_{0}";
        [SerializeField] private string _quickSaveName = "Quicksave";

        public void QuickSave() {
            if (State == null) {
                Debug.LogError("Cannot quick save because AutoSaveManager state is not loaded.");
                return;
            }
            
            State.HasQuickSave = true;
            State.NotifyStateChanged();
            SaveAll(_quickSaveName);
        }

        public bool QuickLoad() {
            if (State == null) {
                Debug.LogError("Cannot quick load because AutoSaveManager state is not loaded.");
                return false;
            }

            if (!State.HasQuickSave) return false;
            
            PersistedSceneController.Instance.LoadState(_quickSaveName);
            return true;
        }

        public void AutoSave() {
            if (State == null) {
                Debug.LogError("Cannot auto save because AutoSaveManager state is not loaded.");
                return;
            }

            int nextAutoSave = State.NextAutoSave;
            State.NextAutoSave = (nextAutoSave + 1) % _autoSaveCount;
            State.LastAutoSave = nextAutoSave;
            State.NotifyStateChanged();
            SaveAll(string.Format(_autoSaveName, nextAutoSave));
        }

        public bool LoadLastAutoSave() {
            if (State == null) {
                Debug.LogError("Cannot auto load because AutoSaveManager state is not loaded.");
                return false;
            }

            if (State.LastAutoSave < 0) return false;
            
            PersistedSceneController.Instance.LoadState(string.Format(_autoSaveName, State.LastAutoSave));
            return true;
        }

        public void SaveAll(string stateName) {
            PersistenceManager.Instance.SaveStateDataAs(stateName);
            if (PersistedLevelRoot.Current) PersistedLevelRoot.Current.Save();
            PersistenceManager.Instance.SaveProfileData();
            PersistenceManager.Instance.SaveGlobalData();
        }
        
        [Serializable]
        public class StateInfo : PersistedData {
            public int LastAutoSave { get; set; } = -1;
            public int NextAutoSave { get; set; } = 0;
            public bool HasQuickSave { get; set; } = false;
        }
    }
}