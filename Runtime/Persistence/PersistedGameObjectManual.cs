using Cysharp.Threading.Tasks;

namespace Infohazard.Sequencing {
    public class PersistedGameObjectManual : PersistedGameObjectBase {
        public override ulong InstanceID => SaveData?.InstanceID ?? 0;

        public UniTask Initialize(ObjectSaveData data) {
            Initializing = true;
            SaveData = data;
            InitializeComponents();
            return PostLoad();
        }
    }
}