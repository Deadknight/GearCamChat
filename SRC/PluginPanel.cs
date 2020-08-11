using System.Windows.Forms;

namespace GearCamChat {
	/// <summary>
	/// Options tab for a plugin
	/// (Just a simple propertygrid)
	/// </summary>
	public partial class PluginPanel : UserControl {
		public PluginPanel() {
			InitializeComponent();
		}

		public PluginPanel(IPlugin p) : this() {
			grid.SelectedObject = p;
		}
	}
}