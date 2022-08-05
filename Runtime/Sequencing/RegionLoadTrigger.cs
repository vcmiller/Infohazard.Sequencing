using UnityEngine;

namespace Infohazard.Sequencing {
    public class RegionLoadTrigger : MonoBehaviour {
        [SerializeField] private LevelManifestRegionEntry[] _regionsToLoad;
        [SerializeField] private LevelManifestRegionEntry[] _regionsToUnload;

        [SerializeField] private SceneGroup _sceneGroup;

        public void Activate() {
            var lvl = LevelRoot.Current;
            if (!lvl) {
                Debug.LogError("Cannot load or unload regions without a current LevelRoot.");
                return;
            }

            foreach (LevelManifestRegionEntry regionEntry in _regionsToLoad) {
                lvl.LoadRegion(regionEntry, _sceneGroup);
            }

            foreach (LevelManifestRegionEntry regionEntry in _regionsToUnload) {
                lvl.UnloadRegion(regionEntry);
            }
        }
    }
}