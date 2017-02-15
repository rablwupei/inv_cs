using inv_cs.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace inv_cs
{
    class InvContext : ApplicationContext {

        Bitmap imageIcon;
        NotifyIcon trayIcon;

        public InvContext() {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            trayIcon.Visible = true;
            Manager.ui = this;
            Manager.Start();
        }

        private void InitializeComponent() {
            imageIcon = Resources.num;
            trayIcon = new NotifyIcon();
            trayIcon.BalloonTipIcon = ToolTipIcon.Info;

            var menuItem1 = new System.Windows.Forms.MenuItem();
            var menuItem2 = new System.Windows.Forms.MenuItem();
            var menuItem3 = new System.Windows.Forms.MenuItem();
            var contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { 
                menuItem1, 
                menuItem2, 
                menuItem3,
            });
            trayIcon.ContextMenu = contextMenu;

            menuItem3.Index = 0;
            menuItem3.Text = Manager.getVersion();
            //menuItem3.Click += new System.EventHandler(Manager.OnRefresh);

            menuItem2.Index = 1;
            menuItem2.Text = "-";

            menuItem1.Index = 2;
            menuItem1.Text = "退出";
            menuItem1.Click += new System.EventHandler(OnQuit);
        }

        public string text {
            get {
                return trayIcon.Text;
            }
            set {
                if (value.Length >= 64) {
                    SetNotifyIconText(trayIcon, value);
                } else {
                    trayIcon.Text = value;
                }
            }
        }

        public static void SetNotifyIconText(NotifyIcon ni, string text) {
            if (text.Length >= 128) text = text.Substring(0, 127);
            Type t = typeof(NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(ni, text);
            if ((bool)t.GetField("added", hidden).GetValue(ni))
                t.GetMethod("UpdateIcon", hidden).Invoke(ni, new object[] { true });
        }

        const string mapping = "0123456789+-!?=. ";
        const int mapping_width = 4;
        const int mapping_height = 7;
        const int mapping_point_x = 0;
        const int mapping_point_width = 1;
        const int mapping_padding = 1;
        const int draw_padding = 1;
        const int icon_width = 16;
        const int icon_height = 16;

        public void RefreshIcon(float num) {
            string numStr = null;
            if (num < 0) {
                if (num > -1) {
                    numStr = num.ToString("0.#");
                } else if (num > -10) {
                    numStr = num.ToString("0.#");
                } else if (num > -100) {
                    numStr = num.ToString("0");
                } else {
                    numStr = num.ToString("0");
                }
            } else {
                if (num < 10) {
                    numStr = num.ToString("0.##");
                } else if (num < 100) {
                    numStr = num.ToString("0.#");
                } else {
                    numStr = num.ToString("0");
                }
            }
            RefreshIcon(numStr);
        }

        Pen pen;
        Bitmap bitmap;
        Graphics graph;

        public void RefreshIcon(string num) {
            var image = imageIcon;
            String str = num;

            if (graph == null) {
                pen = new Pen(Color.Black);
                bitmap = new Bitmap(icon_width, icon_height);
                graph = Graphics.FromImage(bitmap);
            }

            graph.Clear(Color.Transparent);

            graph.DrawLine(pen, 0, icon_height - 1, icon_width - 1, icon_height - 1);
            graph.DrawLine(pen, 0, 0, icon_width - 1, 0);

            int width = -mapping_padding;
            for (int i = 0; i < str.Length; i++) {
                var ch = str[i];
                if (ch == '.') {
                    width += mapping_point_width + draw_padding;
                } else {
                    var index = mapping.IndexOf(ch);
                    if (index != -1) {
                        width += mapping_width + draw_padding;
                    }
                }
            }

            int start_x = (icon_width - width) / 2;
            int start_y = (icon_height - mapping_height) / 2;

            for (int i = 0; i < str.Length; i++) {
                var ch = str[i];
                var index = mapping.IndexOf(ch);
                if (index != -1) {
                    var src_x = index * (mapping_width + mapping_padding);
                    switch (ch) {
                        case '.':
                            graph.DrawImage(image, new Rectangle(start_x, start_y, mapping_point_width, mapping_height),
                                src_x + mapping_point_x, 0, mapping_point_width, mapping_height, System.Drawing.GraphicsUnit.Pixel);
                            start_x += mapping_point_width + draw_padding;
                            break;
                        case ' ':
                            start_x += mapping_width + draw_padding;
                            break;
                        default:
                            graph.DrawImage(image, new Rectangle(start_x, start_y, mapping_width, mapping_height),
                                src_x, 0, mapping_width, mapping_height, System.Drawing.GraphicsUnit.Pixel);
                            start_x += mapping_width + draw_padding;
                            break;
                    }
                }
            }

            IntPtr Hicon = bitmap.GetHicon();
            trayIcon.Icon = Icon.FromHandle(Hicon);
        }

        private void OnApplicationExit(object sender, EventArgs e) {
            trayIcon.Visible = false;
        }

        private void OnQuit(object sender, EventArgs e) {
            Application.Exit();
        }

    }
}
