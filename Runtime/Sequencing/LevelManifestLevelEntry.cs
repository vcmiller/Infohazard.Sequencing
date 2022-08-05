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

using System.Collections.Generic;
using System.Linq;
using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class LevelManifestLevelEntry : ScriptableObject {
        [SerializeField] private string _displayName;
        [SerializeField] private int _levelID;
        [SerializeField] private SceneRef _scene;

        [SerializeField, Expandable]
        private LevelManifestRegionEntry[] _regions;

        public string DisplayName => _displayName;
        public int LevelID => _levelID;
        public SceneRef Scene => _scene;
        public IReadOnlyList<LevelManifestRegionEntry> Regions => _regions;

        public LevelManifestRegionEntry GetRegionWithID(int id) {
            return _regions.FirstOrDefault(r => r.RegionID == id);
        }

        public LevelManifestRegionEntry GetRegionWithSceneName(string sceneName) {
            return _regions.FirstOrDefault(r => r.Scene.Name == sceneName);
        }
    }
}