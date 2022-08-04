using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
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