using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Infohazard.Sequencing {
    public class DebugTeleportPoint : MonoBehaviour {
        [SerializeField] private string _identifier;
        [SerializeField] private LevelManifestRegionEntry _region;
        [SerializeField] private LevelManifestRegionEntry[] _additionalRegions;
        [SerializeField] private UnityEvent _onWarp;

        public string Identifier => _identifier;
        public LevelManifestRegionEntry Region => _region;
        public IReadOnlyList<LevelManifestRegionEntry> AdditionalRegions => _additionalRegions;
        public UnityEvent OnWarp => _onWarp;
    }
}