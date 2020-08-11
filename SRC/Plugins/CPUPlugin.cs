using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using OverlayTools.Helpers;

namespace OverlayTools.Plugins {
	/// <summary>
	/// CPU Plugin
	/// Displays CPU Usage
	/// (c) 2007 Metty
	/// </summary>
	public class CPUPlugin : APlugin {
		private int cpu = 0;
		private PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
		private string m_Format = "{0}%";

		public override string BaseName {
			get { return "cpu"; }
		}

		public override string Name {
			get { return "CPU Usage"; }
		}

		[Save, Category("Specific")]
		public string Format {
			get { return m_Format; }
			set { m_Format = value; }
		}

		public override void Tick() {
			cpu = (int) cpuCounter.NextValue();
		}

		public override void Render(Graphics g) {
			Draw.DrawString(g, Draw.FontSmall, 0, 0, string.Format(Format, cpu));
		}
	}
}