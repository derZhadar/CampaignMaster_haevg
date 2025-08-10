using System.IO;
using System.Xml.Serialization;
using SamCorp.WPF.Extended;

namespace CampaignMaster.Misc {

    public class Settings {

        [XmlIgnore]
        private const string _SettingsFileName = "settings.cfg";

        public double PlayerMapScaleX = 0.975;
        public double PlayerMapScaleY = 0.95;
        public double PlayerMapTranslateX = 25;
        public double PlayerMapTranslateY = 25;

        public Settings() {
        }

        public void Save() {
            XmlSerializerExtended<Settings>.SerializeToFile(this, _SettingsFileName);
        }

        public static Settings LoadSettings() {
            return !File.Exists(_SettingsFileName) ? new Settings() : XmlSerializerExtended<Settings>.DeserializeFromFile(_SettingsFileName);
        }

    }

}