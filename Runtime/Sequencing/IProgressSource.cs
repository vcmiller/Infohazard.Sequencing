using Cysharp.Threading.Tasks;

namespace Infohazard.Sequencing {
    public interface IProgressSource {
        public float Progress { get; }

        public UniTask WaitForCompletionTask();
    }
}