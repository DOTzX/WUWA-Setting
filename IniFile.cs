using System.Runtime.InteropServices;
using System.Text;

namespace WUWA_Setting {

    internal class IniFile {
        private string path;

        internal IniFile (string iniPath) {
            path = iniPath;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString (string section, string key, string value, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString (string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);

        internal string Read (string section, string key, string defaultValue = null) {
            StringBuilder retVal = new StringBuilder(512);
            GetPrivateProfileString(section, key, defaultValue, retVal, retVal.Capacity, path);
            return retVal.ToString();
        }

        internal void Write (string section, string key, string value) {
            WritePrivateProfileString(section, key, value, path);
        }

        internal void DeleteSection (string section) {
            WritePrivateProfileString(section, null, null, path);
        }
    }

}
