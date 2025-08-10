using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.ViewModels {

    internal class vmAudioPlayer : ViewModelBase {

        public ObservableCollection<AudioFile> TavernSounds { get; set; } = new();
        public ObservableCollection<AudioFile> CitySounds { get; set; } = new();
        public ObservableCollection<AudioFile> ForestSounds { get; set; } = new();
        public ObservableCollection<AudioFile> DungeonSounds { get; set; } = new();
        public ObservableCollection<AudioFile> ScarySounds { get; set; } = new();

        public vmAudioPlayer() {
            TavernSounds = new ObservableCollection<AudioFile>(LoadFiles("Tavern"));
            CitySounds = new ObservableCollection<AudioFile>(LoadFiles("City"));
            ForestSounds = new ObservableCollection<AudioFile>(LoadFiles("Forest"));
            DungeonSounds = new ObservableCollection<AudioFile>(LoadFiles("Dungeon"));
            ScarySounds = new ObservableCollection<AudioFile>(LoadFiles("Scary"));
        }

        private IEnumerable<AudioFile> LoadFiles(string folder) {
            var sourcePath = @"Resources/Sounds/" + folder;

            if (!Directory.Exists(sourcePath)) {
                return new List<AudioFile>();
            }

            var files = new List<string>();
            files.AddRange(Directory.GetFiles(sourcePath));

            return files.Select(f => new AudioFile(f));
        }

    }

    internal class AudioFile {

        public string Name { get; set; }
        public string FilePath { get; set; }

        public AudioFile(string filePath) {
            FilePath = filePath;
            Name = Path.GetFileNameWithoutExtension(filePath);
        }

    }

}