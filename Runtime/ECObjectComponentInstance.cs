namespace EnervaCore {
	public class ECObjectComponentInstance {
		public ECObjectComponentData ComponentData { get; set; }
		public ECObjectInstance Parent { get; set; }

		public string ID => ComponentData?.ID;		
		public string[] Tags => ComponentData?.Tags;
		
		public bool HasTag(string tag) {
			return ComponentData?.HasTag(tag) ?? false;
		}
		public void AddTag(string tag) {
			ComponentData?.AddTag(tag);
		}
	}
}