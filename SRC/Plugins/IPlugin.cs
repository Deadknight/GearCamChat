using System.Drawing;

namespace GearCamChat {
	/// <summary>
	/// IPlugin interface
	/// Represents a plugin
	/// </summary>
	public interface IPlugin {
		string BaseName { get; }
		string Name { get; }
		int X { get; set; }
		int Y { get; set; }
		int Page { get; set; }
		bool Active { get; set; }
		void Tick();
		void Render(Graphics g);
	}
}