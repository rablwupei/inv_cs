using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace inv_cs
{
    class InvList {
        public List<InvInfo> list;
        public float interval;
    }

    class InvInfo {
        const string URL_TYPE_SINA = "sina";

        public string url;
        public string urlType;
        public string param1;
        public string param2;
        public bool percent;

        public virtual InvLogic getLogic() {
            if (urlType == URL_TYPE_SINA) {
                return new InvLogicSina(this);
            }
            return null;
        }

    }

    abstract class InvLogic {

        public InvInfo info;

        public InvLogic(InvInfo info) {
            this.info = info;
        }

        public virtual string getUrl() {
            return info.url;
        }

        public abstract bool onResponse(string text);

        public abstract string getName();
        public abstract string getTime();
        public abstract float getCurrent();
        public abstract float getHigh();
        public abstract float getLow();
        public abstract float getOpening();
        public abstract float getClosing();

        public virtual float getPercent() {
            var cur = getCurrent();
            var closing = getClosing();
            return (cur / closing - 1) * 100;
        }

        public virtual string getTip() {
            var low = getLow();
            var cur = getCurrent();
            var closing = getClosing();
            return string.Format("{1} ({0})\n当前: {2}, {3:0.##}%\n最高: {5}, {4}",
                getTime(), getName(), cur, getPercent(), low, getHigh());
        }
    }

    class InvLogicSina : InvLogic {

        public InvLogicSina(InvInfo info) : base(info) { 
            
        }

        string[] contents;

        public override bool onResponse(string text) {
            var pattern = "(?<=\").*?(?=\")";
            var content = Regex.Match(text, pattern);
            var str = content.ToString();
            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            var strs = str.Split(',');
            if (strs.Length < 32) {
                return false;
            }
            contents = strs;
            return true;
        }

        public override string getTime() {
            //return contents[30] + " " + contents[31];
            return contents[31];
        }

        public override string getName() {
            return contents[0];
        }

        public override float getCurrent() {
            float value;
            if (float.TryParse(contents[3], out value)) {
                return value;
            }
            return 0f;
        }

        public override float getHigh() {
            float value;
            if (float.TryParse(contents[4], out value)) {
                return value;
            }
            return 0f;
        }

        public override float getLow() {
            float value;
            if (float.TryParse(contents[5], out value)) {
                return value;
            }
            return 0f;
        }

        public override float getOpening() {
            float value;
            if (float.TryParse(contents[1], out value)) {
                return value;
            }
            return 0f;
        }

        public override float getClosing() {
            float value;
            if (float.TryParse(contents[2], out value)) {
                return value;
            }
            return 0f;
        }

    }
}
