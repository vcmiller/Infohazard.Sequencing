using System.Collections.Generic;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class FakePersistedLevelRoot : MonoBehaviour {
        private void Start() {
            LoadObjects();
        }

        public virtual void LoadObjects() {
            List<PersistedGameObject> gameObjects = new List<PersistedGameObject>();

            PersistedGameObject.CollectGameObjects(gameObject.scene, gameObjects);
            PersistedGameObject.InitializeGameObjects(gameObjects);
        }
    }
}