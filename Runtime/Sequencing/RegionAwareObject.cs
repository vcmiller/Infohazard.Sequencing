namespace Infohazard.Sequencing.Runtime {
    public interface IRegionAwareObject {
        public RegionRoot CurrentRegion { get; set; }
        public bool CanTransitionTo(RegionRoot region);
    }
}