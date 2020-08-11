using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GearCamChat.Helpers {
	/// <summary>
	/// A Win32 keyboard hook
	/// Used for doing stuff when a key is pressed
	/// Not directly a overlay, but is connected it - and needs to be in a .dll
	/// (c) 2007 Metty
	/// </summary>
	public class KeyboardHook {
		#region Static

		#region Delegates

		/// <summary>
		/// Win32 HookProc
		/// </summary>
		/// <param name="code">Code (< 0 means not for your, just forward)</param>
		/// <param name="wParam">Hook specific</param>
		/// <param name="lParam">Hook specific</param>
		/// <returns></returns>
		public delegate IntPtr HookProc(int code, int wParam, int lParam);

		/// <summary>
		/// Used for forwarding Keys
		/// </summary>
		/// <param name="k">Pressed Keys</param>
		public delegate void KeyboardEvent(Keys k);

		#endregion

		public const int WH_KEYBOARD_LL = 13;
		public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

		[DllImport("user32.dll")]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, int wParam, int lParam);

		[DllImport("user32.dll")]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[StructLayout(LayoutKind.Explicit)]
		public struct KeyStruct {
			//Fieldofset needed, otherwise resharper may mix this all up
			[FieldOffset(4*sizeof (int))] public IntPtr dwExtraInfo;
			[FieldOffset(2*sizeof (int))] public int flags;
			[FieldOffset(1*sizeof (int))] public int scanCode;
			[FieldOffset(3*sizeof (int))] public int time;
			[FieldOffset(0*sizeof (int))] public int vkCode;
		}

		#endregion

		private IntPtr hook = IntPtr.Zero;
		private HookProc hookprc;
		private KeyboardEvent m_DownEvent = null;
        private KeyboardEvent m_UpEvent = null;

		/// <summary>
		/// Fired when a key is pressed
		/// </summary>
		public KeyboardEvent DownEvent {
            get { return m_DownEvent; }
            set { m_DownEvent = value; }
		}

        /// <summary>
        /// Fired when a key is released
        /// </summary>
        public KeyboardEvent UpEvent
        {
            get { return m_UpEvent; }
            set { m_UpEvent = value; }
        }

		/// <summary>
		/// Installs a Win32 hook into the system
		/// </summary>
		public void InstallHooK() {
			if (hook != IntPtr.Zero) {
				return;
			}

			hookprc = OnHook;
			hook = SetWindowsHookEx(13, hookprc, Marshal.GetHINSTANCE(GetType().Module), 0);
			if (hook == IntPtr.Zero) {
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}
		}

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		/// <param name="code">-</param>
		/// <param name="wParam">KeyStruct</param>
		/// <param name="lParam">-</param>
		/// <returns></returns>
		public IntPtr OnHook(int code, int wParam, int lParam) {
			IntPtr res = CallNextHookEx(hook, code, wParam, lParam);

			if (code >= 0 && m_DownEvent != null && wParam == WM_KEYDOWN) {
				KeyStruct s = (KeyStruct) Marshal.PtrToStructure(new IntPtr(lParam), typeof (KeyStruct));
				Keys key = (Keys) s.vkCode;
				m_DownEvent(key);
			}
            else if (code >= 0 && m_UpEvent != null && wParam == WM_KEYUP)
            {
                KeyStruct s = (KeyStruct)Marshal.PtrToStructure(new IntPtr(lParam), typeof(KeyStruct));
                Keys key = (Keys)s.vkCode;
                m_UpEvent(key);
            }

			return res;
		}

		/// <summary>
		/// Uninstalls a installed Win32 hook
		/// </summary>
		public void UninstallHook() {
			if (hook == IntPtr.Zero) {
				return;
			}

			UnhookWindowsHookEx(hook);
			hook = IntPtr.Zero;
			hookprc = null;
		}
	}
}