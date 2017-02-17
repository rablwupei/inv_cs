
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace inv_cs
{
    class Manager
    {
        public static InvContext ui;
        public static string exeDir;
        public static string jsonPath;
        public static Timer timer;
        public static InvList invList;

        public static bool DEBUG = false;

        public static void Start() {
            exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            jsonPath = Path.Combine(exeDir, "list.json");

            ReadJson();
            RefreshInfo();
            StartTick();
        }

        public static System.Text.Encoding UTF8 = new System.Text.UTF8Encoding(false);
        public static DateTime listFileTime;

        public static void ReadJson() {
            ui.RefreshIcon("==");
            invList = null;

            try {
                string json = null;
                if (DEBUG) {
                    json = UTF8.GetString(Properties.Resources.list);
                } else {
                    if (!File.Exists(jsonPath)) {
                        File.WriteAllBytes(jsonPath, Properties.Resources.list);
                    }
                    json = File.ReadAllText(jsonPath, UTF8);
                    listFileTime = File.GetLastWriteTime(jsonPath);
                }

                invList = JsonConvert.DeserializeObject<InvList>(json);
            } catch (Exception) {
                invList = null;
            }
        }

        public static async void RefreshInfo() {
            if (invList == null || invList.list == null) {
                postError("list.json parse error");
                return;
            }
            if (invList.list.Count <= 0) {
                postError("list.json count <= 0");
                return;
            }

            var info = invList.list[0];
            var logic = info.getLogic();
            var url = logic.getUrl();
            string response = null;
            try {
                using (var client = new HttpClient()) {
                    response = await client.GetStringAsync(url);
                    //System.Console.WriteLine(response);
                }
            } catch (Exception) {
                response = null;
            }
            if (string.IsNullOrEmpty(response)) {
                postError("http error.");
                return;
            }
            if (!logic.onResponse(response)) {
                postError("response error.");
                return;
            }
            ui.text = logic.getTip();
            ui.RefreshIcon(info.percent ? logic.getPercent() : logic.getCurrent());
        }

        public static void postMessage(float value, string text) {
            ui.text = getTipText(text);
            ui.RefreshIcon(value);
        }

        public static void postError(string text) {
            ui.text = getTipText("error: " + text);
            ui.RefreshIcon("??");
        }

        public static string getTipText(string text) {
            return string.Format("ppinv {0}\n{1}", getVersion(), text);
        }

        public static string getVersion() { 
            var verson = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return "v" + verson.Substring(0, verson.LastIndexOf('.'));
        }

        public static void StartTick() {
            if (timer == null) {
                timer = new Timer();
                timer.Tick += new EventHandler(OnTick);
            } else {
                StopTick();
            }
            var interval = invList.interval;
            if (interval < 0.5f) {
                interval = 0.5f;
            }
            timer.Interval = (int)(interval * 1000);
            timer.Start();
        }

        public static void StopTick() {
            if (timer != null) {
                timer.Stop();
            }
        }

        public static bool RefreshIfFileDiff() {
            if (listFileTime == null) {
                return false;
            }
            if (!File.Exists(jsonPath)) {
                return false;
            }
            var curFileTime = File.GetLastWriteTime(jsonPath);
            if (curFileTime != listFileTime) {
                listFileTime = curFileTime;
                return true;
            }
            return false;
        }

        private static void OnTick(object sender, EventArgs e) {
            if (RefreshIfFileDiff()) {
                ReadJson();
                StartTick();
            }
            RefreshInfo();
        }

        public static void OnRefresh(object sender, EventArgs e) {
            ReadJson();
            RefreshInfo();
            StartTick();
        }

    }
}
