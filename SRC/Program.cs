using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using OverlayLib;
using GearCamChat.Helpers;
using GearCamChat.Plugins;

namespace GearCamChat {
	/// <summary>
	/// Overlay Tools
	/// (c) 2007 Metty
	/// </summary>
	public static class Program {
		public static readonly Overlay Overlay = new Overlay();
		public static readonly SortedDictionary<string, IPlugin> Plugins = new SortedDictionary<string, IPlugin>();
		public static MainForm MainForm;

		/// <summary>
		/// The "General" Plugin, containing important information
		/// </summary>
		public static GeneralPlugin General {
			get {
				if (Plugins.ContainsKey("general")) {
					return (GeneralPlugin) Plugins["general"];
				}
				else {
					return null;
				}
			}
		}

		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		public static void Main(string[] args) {

			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal; //No influence on games, ...
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm = new MainForm();
			LoadPlugins();

			//Initialise overlay
			try {
				Overlay.Renderer = OnRender;
				UpdatePosition();
				UpdateSize();
				Overlay.Initialise();
			}
			catch (Exception e) {
				MessageBox.Show(e.ToString());
				return;
			}

			//Initialise keyboard
			KeyboardHook hk = new KeyboardHook();
			hk.DownEvent = OnKeyboardDown;
            hk.UpEvent = OnKeyboardUp;
			hk.InstallHooK();

			//Start app
			General.Active = false;
			StartTimer();
            MainForm.SetStartup();
            General.SetMain(MainForm);
			Application.Run();

			//Deinitialise
			hk.UninstallHook();
			Overlay.Dispose();
		}

		/// <summary>
		/// Loads all plugins using reflections
		/// Sets [Save] Properties to their ini values
		/// </summary>
		public static void LoadPlugins() {
			INIStreamer ini = new INIStreamer("overlays.ini");
			ini.ReadIni();

			//Load plugins and values
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) 
            {
                if (a.ManifestModule.Name == "GearCamChat.exe")
                {
                    foreach (Type t in a.GetTypes())
                    {
                        if (t.IsAbstract || t.IsInterface)
                        {
                            continue;
                        }
                        if (t.GetInterface(typeof(IPlugin).FullName) == null)
                        {
                            continue;
                        }

                        //Get the plugin
                        IPlugin p = (IPlugin)a.CreateInstance(t.FullName);

                        //Fill in all saved vars
                        INIStreamer.Topic tpc = ini.GetTopic(p.BaseName);
                        if (tpc != null)
                        {
                            foreach (PropertyInfo i in t.GetProperties())
                            {
                                if (i.GetCustomAttributes(typeof(SaveAttribute), true).Length == 0)
                                {
                                    continue;
                                }
                                if (!tpc.Items.ContainsKey(i.Name))
                                {
                                    continue;
                                }

                                object val;
                                if (i.PropertyType.IsEnum)
                                {
                                    val = Enum.Parse(i.PropertyType, tpc.Items[i.Name].ToString());
                                }
                                else
                                {
                                    val = Convert.ChangeType(tpc.Items[i.Name], i.PropertyType);
                                }
                                i.SetValue(p, val, null);
                            }
                        }

                        //Add to plugin list
                        Plugins.Add(p.BaseName, p);
                    }
                }
			}

			//Add tabs
			AddToTab(Plugins["general"]);
			foreach (KeyValuePair<string, IPlugin> p in Plugins) {
				if (p.Key != "general") {
					AddToTab(p.Value);
				}
			}
		}

		/// <summary>
		/// Adds a tab for a plugin to the mainform
		/// </summary>
		/// <param name="p">Plugin to add</param>
		private static void AddToTab(IPlugin p) {
			TabPage page = new TabPage();
			page.Text = p.Name;
			page.Name = "tab_plugin_" + p.BaseName;
			PluginPanel pp = new PluginPanel(p);
			pp.Dock = DockStyle.Fill;
			page.Controls.Add(pp);
			MainForm.tabs.TabPages.Add(page);
		}

		/// <summary>
		/// Saves all the plugin data into a .ini file
		/// </summary>
		public static void SavePlugins() {
			INIStreamer ini = new INIStreamer("overlays.ini");

			foreach (IPlugin p in Plugins.Values) {
				foreach (PropertyInfo i in p.GetType().GetProperties()) {
					if (i.GetCustomAttributes(typeof (SaveAttribute), true).Length == 0) {
						continue;
					}

					ini.SetItem(p.BaseName, i.Name, i.GetValue(p, null).ToString());
				}
			}

			ini.WriteIni();
		}

		/// <summary>
		/// Starts the render and tick timers
		/// </summary>
		public static void StartTimer() {
			Timer t = new Timer();
			t.Tick += Render;
			t.Interval = General.RenderInterval;
			t.Start();

			Timer a = new Timer();
			a.Tick += OnTick;
			a.Interval = General.TickInterval;
			a.Start();
		}

		/// <summary>
		/// Called every tick
		/// Gives the plugins the chance to do some stuff
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void OnTick(object sender, EventArgs args) {
			if (!General.Active) {
				return;
			}

			foreach (IPlugin p in Plugins.Values) {
				//Only update visible, active plugins
				if (p.Active && (p.Page == General.Page || p.Page == -1)) {
					p.Tick();
				}
			}
		}

		/// <summary>
		/// Updates the position of the overlay
		/// </summary>
		public static void UpdatePosition() {
			if (General != null) {
				Overlay.Position = new Point(General.X, General.Y);
			}
		}

		/// <summary>
		/// Updates the size of the overlay
		/// </summary>
		public static void UpdateSize() {
			if (General != null) {
				Overlay.Size = new Size(General.Width, General.Height);
			}
		}

		/// <summary>
		/// Called every tick
		/// Causes the overlay to re-render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void Render(object sender, EventArgs args) {
			if (!General.Active) {
				return;
			}

			Overlay.Update();
		}

		/// <summary>
		/// Called when the overlay is redrawn
		/// </summary>
		/// <param name="g">Graphics g</param>
		public static void OnRender(Graphics g) {
			g.Clear(General.BackgroundColor);

			if (General.FakeAlpha) {
				g.CopyFromScreen(Overlay.Boundings.Location, Point.Empty, Overlay.Boundings.Size, CopyPixelOperation.SourceCopy);
			}

			foreach (IPlugin p in Plugins.Values) {
				if (p.Active && (p.Page == General.Page || p.Page == -1)) {
					g.ResetTransform();
					g.TranslateTransform(p.X, p.Y, MatrixOrder.Append);
					p.Render(g);
				}
			}
		}

		/// <summary>
		/// Called when a key is pressed
		/// Handles shortcuts
		/// </summary>
		/// <param name="k">Key</param>
		public static void OnKeyboardDown(Keys k) {
			if (k == General.ChangeVisibleKey) 
            {
                bool lastActive = MainForm.Active;
                if (lastActive == false)
                {
                    MainForm.Active = true;
                    if (!MainForm.Webcam.IsRunning)
                        MainForm.Webcam.Start(0);
                }
			}
			else if (k == General.ChangePageKey) {
				int max = 1;

				foreach (IPlugin p in Plugins.Values) {
					if (p.Active && p.Page > max) {
						max = p.Page;
					}
				}

				General.Page = (General.Page + 1)%(max + 1);
				Render(null, EventArgs.Empty);
			}
		}

        public static void OnKeyboardUp(Keys k)
        {
            if (k == General.ChangeVisibleKey)
            {
                bool lastActive = MainForm.Active;
                if (lastActive == true)
                {
                    MainForm.Active = false;
                    if (MainForm.Webcam.IsRunning)
                        MainForm.Webcam.Stop();
                }
            }
        }
	}
}