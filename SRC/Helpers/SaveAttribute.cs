using System;

namespace GearCamChat {
	/// <summary>
	/// Save attribute for the LoadPlugin and SavePlugin functions
	/// Just an empty attribute to signalize that this property should be saved
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class SaveAttribute : Attribute {
		private readonly bool m_Save = true;


		public SaveAttribute(bool save) {
			m_Save = save;
		}

		public SaveAttribute() : this(true) {}

		public bool Save {
			get { return m_Save; }
		}
	}
}