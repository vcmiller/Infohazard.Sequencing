using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infohazard.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Sequencing {
    public abstract class PersistedGameObjectBase : MonoBehaviour {
        public ObjectSaveData SaveData { get; protected set; }

        public bool Initialized { get; protected set; }
        
        public bool Initializing { get; protected set; }
        
        public abstract ulong InstanceID { get; }
        
        protected List<IPersistedComponent> Components;

        public event Action LoadCompleted;

        protected void OnLoadCompleted() {
            LoadCompleted?.Invoke();
        }
        
        protected virtual void Awake() {
            if (!Application.isPlaying) return;

            Components = new List<IPersistedComponent>();
            GetPersistedComponents(transform, Components);
        }

        private static readonly List<IPersistedComponent> TempComponents = new List<IPersistedComponent>();
        private static void GetPersistedComponents(Transform transform, List<IPersistedComponent> components) {
            TempComponents.Clear();
            transform.GetComponents(TempComponents);
            components.AddRange(TempComponents);
            
            for (int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                if (child.TryGetComponent(out PersistedGameObjectBase _)) continue;
                GetPersistedComponents(child, components);
            }
        }

        protected void InitializeComponents() {
            foreach (IPersistedComponent component in Components) {
                component.Initialize(this, SaveData, new ComponentID(transform, (Component) component).ToString());
            }
        }

        protected async UniTask PostLoad() {
            await UniTask.WhenAll(Components.Select(component => component.PostLoad()));

            Initialized = true;
            Initializing = false;
            OnLoadCompleted();
        }

        public void WriteState() {
            foreach (IPersistedComponent component in Components) {
                if (!(Object) component) {
                    Debug.LogException(new Exception($"GameObject: {gameObject} has a destroyed component."));
                    continue;
                }
                
                component.WriteState();
            }
        }
    }
}