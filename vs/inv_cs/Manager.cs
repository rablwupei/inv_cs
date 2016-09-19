using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace inv_cs
{
    class Manager
    {
        public static InvContext ui;
        public static string exeDir;
        public static Timer timer;

        public static void Start() {
            exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            ui.RefreshIcon("==");

            StartTick();
        }

        public static void StartTick() {
            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += new EventHandler(OnTick);
            timer.Start();
        }

        static float value = 0;

        private static void OnTick(object sender, EventArgs e) {
            ui.RefreshIcon(value);
            timer.Stop();
            value += 0.01f;
        }

    }
}
