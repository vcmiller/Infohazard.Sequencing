using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Infohazard.Sequencing {
    public class MultiProgressSource : IProgressSource {
        private List<IProgressSource> _sources;

        public MultiProgressSource(List<IProgressSource> list) {
            _sources = list;
        }

        public MultiProgressSource(IEnumerable<IProgressSource> sources) {
            _sources = sources.ToList();
        }

        public float Progress => _sources.Sum(source => source.Progress) / _sources.Count;
        
        public UniTask WaitForCompletionTask() {
            return UniTask.WhenAll(_sources.Select(source => source.WaitForCompletionTask()));
        }
    }
}