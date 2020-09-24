using System.Runtime.InteropServices;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    public class IniFile
    {
        public static string RelativePath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        public IniFile(string Path) =>
            RelativePath = Path;

        public void IniWriteValue(string section, string key, string value) =>
            WritePrivateProfileString(section, key, value, RelativePath);

        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder();
            GetPrivateProfileString(Section, Key, "", temp, 255, RelativePath);
            return temp.ToString();
        }
    }
}
