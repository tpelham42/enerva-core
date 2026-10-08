using EnervaCore.Interfaces;

namespace EnervaCore {
	public class ECObjectComponentInstance : IECObjectComponentInstance {
		public ECObjectComponentData ComponentData { get; set; }
		public IECObjectInstance Parent { get; set; }

		public string ID => ComponentData?.ID;		
		public string[] Tags => ComponentData?.Tags;
		
		public bool HasTag(string tag) {
			return ComponentData?.HasTag(tag) ?? false;
		}
		public void AddTag(string tag) {
			ComponentData?.AddTag(tag);
		}

		public virtual void OnInit() {
            // Default implementation does nothing. Override in derived classes.
        }
    }
}