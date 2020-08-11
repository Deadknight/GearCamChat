using System.ComponentModel;
using System.Drawing;

namespace GearCamChat {
	/// <summary>
	/// Abstract Plugin
	/// Basic implementation of IPlugin
	/// </summary>
	public abstract class APlugin : IPlugin {
		protected bool m_Active = false;
		protected int m_Page = 0;
		protected int m_X = 10;
		protected int m_Y = 10;

		#region IPlugin Members

		public abstract void Tick();
		public abstract void Render(Graphics g);

		[Browsable(false)]
		public abstract string BaseName { get; }

		[Browsable(false)]
		public abstract string Name { get; }

		[Save, Category("General")]
		public virtual int X {
			get { return m_X; }
			set { m_X = value; }
		}

		[Save, Category("General")]
		public virtual int Y {
			get { return m_Y; }
			set { m_Y = value; }
		}

		[Save, Category("General")]
		public virtual int Page {
			get { return m_Page; }
			set { m_Page = value; }
		}


		[Save, Category("General")]
		public virtual bool Active {
			get { return m_Active; }
			set { m_Active = value; }
		}

		#endregion
	}
}