using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using CampaignMaster.Windows;
using SamCorp.WPF.Commands;
using SamCorp.WPF.Models;

namespace CampaignMaster.Models {

    public class mdlCampaign : ModelBase {

        private string _Name;

        private int _Hour;
        private int _Day;
        private int _Year;

        public int Hour {
            get => _Hour;
            set => SetField(ref _Hour, value);
        }

        public int Day {
            get => _Day;
            set => SetField(ref _Day, value);
        }

        public int Year {
            get => _Year;
            set => SetField(ref _Year, value);
        }

        public string Name {
            get => _Name;
            set => SetField(ref _Name, value);
        }

        public string FileName => $"{Regex.Replace(Name, @"\s+", "")}.cmp";

        public string DirectoryName => Path.Combine("Campaigns", _Name);

        public string DirectoryImages => Path.Combine(DirectoryName, "Images");

        public string DirectoryMaps => Path.Combine(DirectoryName, "Maps");

        public ObservableCollection<mdlCharacter> Characters { get; set; }
        public ObservableCollection<mdlCharacter> CombatOrder { get; set; } = new();

        private mdlCharacter _ActiveCombatant;

        public mdlCharacter ActiveCombatant {
            get => _ActiveCombatant;
            set => SetField(ref _ActiveCombatant, value);
        }

        public StrokeCollection Notes { get; set; }

        public ICommand CommandLoadCampaign => new Command(() => LoadCampaign(live: true));
        public ICommand CommandEditCampaign => new Command(() => LoadCampaign(live: false));

        public mdlCampaign() {
            Name = "Campaign Name";

            Year = 998;
            Day = 1;
            Hour = 8;

            Characters = new ObservableCollection<mdlCharacter> {
                new() { Name = "Tineas", PlayerName = "Alex", Class = "Cancer Mage", Level = 5, Xp = 1200, Race = "Mensch", Color = Brushes.SaddleBrown },
                new() { Name = "Scathgar", PlayerName = "Fabian", Class = "Shadowmage", Level = 5, Xp = 1130, Race = "Mensch", Color = Brushes.DarkSlateBlue },
                new() { Name = "Galac", PlayerName = "Flo", Class = "Artificer", Level = 5, Xp = 1240, Race = "Zwerg", Color = Brushes.Orchid },
                new() { Name = "Frulamin", PlayerName = "Fabi", Class = "Rogue", Level = 5, Xp = 1310, Race = "Halb-Elf", Color = Brushes.ForestGreen },
            };

            Notes = new StrokeCollection();
        }

        public Task Save() {
            return Task.Run(async () => {
                await CreateDirectoryTree();

                serCampaign cmp = this;
                var formatter = new BinaryFormatter();
                await using var fs = new FileStream(Path.Combine(DirectoryName, FileName), FileMode.Create, FileAccess.Write);
                formatter.Serialize(fs, cmp);
                fs.Close();
            });
        }

        private Task CreateDirectoryTree() {
            return Task.Run(() => {
                if (!Directory.Exists("Campaigns"))
                    Directory.CreateDirectory("Campaigns");
                if (!Directory.Exists(DirectoryName))
                    Directory.CreateDirectory(DirectoryName);
                if (!Directory.Exists(DirectoryImages))
                    Directory.CreateDirectory(DirectoryImages);
                if (!Directory.Exists(DirectoryMaps))
                    Directory.CreateDirectory(DirectoryMaps);
            });
        }

        private async void LoadCampaign(bool live) {
            if (Application.Current.MainWindow is not wndCampaignMaster mainWindow) {
                return;
            }

            App.CurrentCampaign = this;
            var load = new wndLoading();
            load.Show();
            mainWindow.Initialize(live: live);
            await Task.Delay(100);
            load.Close();
        }

    }

    [Serializable]
    public class serCampaign {

        public string Name { get; set; }
        public int Hour { get; set; }
        public int Day { get; set; }
        public int Year { get; set; }
        public List<serCharacter> Characters { get; set; }
        public byte[] Notes { get; set; }

        public serCampaign(mdlCampaign cmp) {
            Name = cmp.Name;
            Hour = cmp.Hour;
            Day = cmp.Day;
            Year = cmp.Year;

            Characters = new List<serCharacter>();
            cmp.Characters.ToList().ForEach((c) => Characters.Add(c));

            using (var ms = new MemoryStream()) {
                cmp.Notes.Save(ms);
                Notes = ms.ToArray();
            }
        }

        public static implicit operator serCampaign(mdlCampaign cmp) {
            return new serCampaign(cmp);
        }

        public static implicit operator mdlCampaign(serCampaign cmp) {
            var result = new mdlCampaign();
            result.Name = cmp.Name;
            result.Hour = cmp.Hour;
            result.Day = cmp.Day;
            result.Year = cmp.Year;
            result.Characters = new ObservableCollection<mdlCharacter>();
            cmp.Characters.ForEach((c) => result.Characters.Add(c));

            if (cmp.Notes != null) {
                using (var ms = new MemoryStream(cmp.Notes))
                    new StrokeCollection(ms).ToList().ForEach((s) => result.Notes.Add(s));
            }

            return result;
        }

    }

}