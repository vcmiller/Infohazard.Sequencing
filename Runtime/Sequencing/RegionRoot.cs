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
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class RegionRoot : MonoBehaviour {
        [SerializeField] private LevelManifestRegionEntry _manifestEntry;
        [SerializeField] private Transform _dynamicObjectRoot;

        public LevelManifestRegionEntry ManifestEntry => _manifestEntry;
        
        public Transform DynamicObjectRoot => _dynamicObjectRoot;

        public int RegionIndex => _manifestEntry.RegionID;
        
        public bool Initialized { get; private set; }

        public event Action Unloading;
        
        public virtual async UniTask Initialize() {
            await LevelRoot.Current.RegisterRegion(this);
            Initialized = true;
        }

        public virtual UniTask Cleanup() {
            Unloading?.Invoke();
            Initialized = false;
            // It's ok if the level is unloaded before its regions (as long as it's in the same frame)
            if (LevelRoot.Current) {
                LevelRoot.Current.DeregisterRegion(this);
            }
            return UniTask.CompletedTask;
        }
    }
}