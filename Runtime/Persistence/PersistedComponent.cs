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

using UnityEngine;

namespace Infohazard.Sequencing {
    public interface IPersistedComponent {
        public void Initialize(PersistedGameObject persistedGameObject, PersistedData parent, string id);
        public void PostLoad();
        public void WriteState();
    }
    
    public class PersistedComponent<T> : MonoBehaviour, IPersistedComponent where T : PersistedData, new() {
        protected T State { get; private set; }
        public bool Initialized => State != null;
        protected PersistedGameObject PersistedGameObject { get; private set; }

        public void Initialize(PersistedGameObject persistedGameObject, PersistedData parent, string id) {
            if (PersistenceManager.Instance.GetCustomData(parent, id, out T state)) {
                PersistedGameObject = persistedGameObject;
                State = state;
                if (state.Initialized) LoadState();
                else LoadDefaultState();
                State.Initialized = true;
            } else {
                State = null;
                Debug.LogError($"State {id} could not be loaded.");
            }
        }

        protected void CreateFakeState() {
            State = new T();
            LoadDefaultState();
        }
        
        public virtual void LoadState() {}
        public virtual void LoadDefaultState() {}
        public virtual void PostLoad() {}
        public virtual void WriteState() {}
    }
}