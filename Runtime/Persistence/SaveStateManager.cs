// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using UnityEngine;

namespace Infohazard.Sequencing {
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