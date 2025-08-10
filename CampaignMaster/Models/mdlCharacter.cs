using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CampaignMaster.Misc;
using CampaignMaster.Windows;
using SamCorp.WPF.Alerts;
using SamCorp.WPF.Commands;
using SamCorp.WPF.Extensions;
using SamCorp.WPF.Models;

namespace CampaignMaster.Models {

    public class mdlCharacter : ModelBase {

        public static readonly List<int> TableLevelXp = new() {
            0,
            1000,
            3000,
            6000,
            10000,
            15000,
            21000,
            28000,
            36000,
            45000,
            55000,
            66000,
            78000,
            91000,
            105000,
            120000,
            136000,
            153000,
            171000,
            190000
        };

        private int _Xp = 0;
        private int _XpSession = 0;
        private int _XpSessionCount = 0;

        private bool _IsDead;
        private bool _IsPoisoned;
        private bool _IsDazed;
        private bool _IsEnchanted;
        private bool _IsBurning;
        private bool _IsActiveCombatant;

        private string _PlayerName;
        private string _ColorCode = "#FFFFEFD5";
        private CharacterCombatIndicatorAnchor _CombatIndicatorAnchor = CharacterCombatIndicatorAnchor.None;

        private string _Name = "<New Character>";
        private string _Race = "Race";
        private int _Age = 33;
        private string _Class = "Class";
        private int _Level;
        private int _HitPoints;
        private int _HitDice;
        private int _Damage;
        private int _ArmorClass;
        private int _Initiative;

        #region Attributes

        private int _Strength = 8;
        private int _Dexterity = 10;
        private int _Constitution = 12;
        private int _Intelligence = 11;
        private int _Wisdom = 16;
        private int _Charisma = 15;

        public int Strength {
            get => _Strength;
            set => SetField(ref _Strength, value);
        }

        public int Dexterity {
            get => _Dexterity;
            set => SetField(ref _Dexterity, value);
        }

        public int Constitution {
            get => _Constitution;
            set => SetField(ref _Constitution, value);
        }

        public int Intelligence {
            get => _Intelligence;
            set => SetField(ref _Intelligence, value);
        }

        public int Wisdom {
            get => _Wisdom;
            set => SetField(ref _Wisdom, value);
        }

        public int Charisma {
            get => _Charisma;
            set => SetField(ref _Charisma, value);
        }

        #endregion

        #region Skills

        private int _Climb;
        private int _Jump;
        private int _Swim;
        private int _Balance;
        private int _EscapeArtist;
        private int _Hide;
        private int _MoveSilently;
        private int _OpenLock;
        private int _Ride;
        private int _SleightOfHand;
        private int _Tumble;
        private int _UseRope;
        private int _Concentration;
        private int _Appraise;
        private int _Craft;
        private int _DecipherScript;
        private int _DisableDevice;
        private int _Forgery;
        private int _Knowledge;
        private int _Search;
        private int _Spellcraft;
        private int _Heal;
        private int _Listen;
        private int _Profession;
        private int _SenseMotive;
        private int _Spot;
        private int _Survival;
        private int _Bluff;
        private int _Diplomacy;
        private int _Disguise;
        private int _GatherInformation;
        private int _HandleAnimal;
        private int _Intimidate;
        private int _Perform;
        private int _UseMagicDevice;

        public int Climb {
            get => _Climb;
            set => SetField(ref _Climb, value);
        }

        public int Jump {
            get => _Jump;
            set => SetField(ref _Jump, value);
        }

        public int Swim {
            get => _Swim;
            set => SetField(ref _Swim, value);
        }

        public int Balance {
            get => _Balance;
            set => SetField(ref _Balance, value);
        }

        public int EscapeArtist {
            get => _EscapeArtist;
            set => SetField(ref _EscapeArtist, value);
        }

        public int Hide {
            get => _Hide;
            set => SetField(ref _Hide, value);
        }

        public int MoveSilently {
            get => _MoveSilently;
            set => SetField(ref _MoveSilently, value);
        }

        public int OpenLock {
            get => _OpenLock;
            set => SetField(ref _OpenLock, value);
        }

        public int Ride {
            get => _Ride;
            set => SetField(ref _Ride, value);
        }

        public int SleightOfHand {
            get => _SleightOfHand;
            set => SetField(ref _SleightOfHand, value);
        }

        public int Tumble {
            get => _Tumble;
            set => SetField(ref _Tumble, value);
        }

        public int UseRope {
            get => _UseRope;
            set => SetField(ref _UseRope, value);
        }

        public int Concentration {
            get => _Concentration;
            set => SetField(ref _Concentration, value);
        }

        public int Appraise {
            get => _Appraise;
            set => SetField(ref _Appraise, value);
        }

        public int Craft {
            get => _Craft;
            set => SetField(ref _Craft, value);
        }

        public int DecipherScript {
            get => _DecipherScript;
            set => SetField(ref _DecipherScript, value);
        }

        public int DisableDevice {
            get => _DisableDevice;
            set => SetField(ref _DisableDevice, value);
        }

        public int Forgery {
            get => _Forgery;
            set => SetField(ref _Forgery, value);
        }

        public int Knowledge {
            get => _Knowledge;
            set => SetField(ref _Knowledge, value);
        }

        public int Search {
            get => _Search;
            set => SetField(ref _Search, value);
        }

        public int Spellcraft {
            get => _Spellcraft;
            set => SetField(ref _Spellcraft, value);
        }

        public int Heal {
            get => _Heal;
            set => SetField(ref _Heal, value);
        }

        public int Listen {
            get => _Listen;
            set => SetField(ref _Listen, value);
        }

        public int Profession {
            get => _Profession;
            set => SetField(ref _Profession, value);
        }

        public int SenseMotive {
            get => _SenseMotive;
            set => SetField(ref _SenseMotive, value);
        }

        public int Spot {
            get => _Spot;
            set => SetField(ref _Spot, value);
        }

        public int Survival {
            get => _Survival;
            set => SetField(ref _Survival, value);
        }

        public int Bluff {
            get => _Bluff;
            set => SetField(ref _Bluff, value);
        }

        public int Diplomacy {
            get => _Diplomacy;
            set => SetField(ref _Diplomacy, value);
        }

        public int Disguise {
            get => _Disguise;
            set => SetField(ref _Disguise, value);
        }

        public int GatherInformation {
            get => _GatherInformation;
            set => SetField(ref _GatherInformation, value);
        }

        public int HandleAnimal {
            get => _HandleAnimal;
            set => SetField(ref _HandleAnimal, value);
        }

        public int Intimidate {
            get => _Intimidate;
            set => SetField(ref _Intimidate, value);
        }

        public int Perform {
            get => _Perform;
            set => SetField(ref _Perform, value);
        }

        public int UseMagicDevice {
            get => _UseMagicDevice;
            set => SetField(ref _UseMagicDevice, value);
        }

        #endregion

        #region Saves

        private int _Fortitude;
        private int _Reflex;
        private int _Will;

        public int Fortitude {
            get => _Fortitude;
            set => SetField(ref _Fortitude, value);
        }

        public int Reflex {
            get => _Reflex;
            set => SetField(ref _Reflex, value);
        }

        public int Will {
            get => _Will;
            set => SetField(ref _Will, value);
        }

        #endregion

        public StrokeCollection Notes { get; set; } = new();

        public bool IsNpc => !NpcType.IsNullOrEmpty();
        public string NpcType { get; set; }

        public string ColorCode {
            get => _ColorCode;
            set {
                if (value.IsNullOrEmpty()) {
                    return;
                }

                SetField(ref _ColorCode, value);
                RaisePropertyChanged(nameof(Color));
            }
        }

        public SolidColorBrush Color {
            get => IsNpc ? "#FFB22222".ToBrush() : ColorCode.ToBrush();
            set => ColorCode = value.Color.ToString();
        }

        public CharacterCombatIndicatorAnchor CombatIndicatorAnchor {
            get => _CombatIndicatorAnchor;
            set => SetField(ref _CombatIndicatorAnchor, value);
        }

        public string PlayerName {
            get => _PlayerName;
            set => SetField(ref _PlayerName, value);
        }

        public string Name {
            get => _Name;
            set => SetField(ref _Name, value);
        }

        public string Class {
            get => _Class;
            set => SetField(ref _Class, value);
        }

        public string Race {
            get => _Race;
            set => SetField(ref _Race, value);
        }

        public int Age {
            get => _Age;
            set => SetField(ref _Age, value);
        }

        public int Level {
            get => _Level;
            set => SetField(ref _Level, value);
        }

        public int HitPoints {
            get => _HitPoints;
            set => SetField(ref _HitPoints, value);
        }

        public int HitDice {
            get => _HitDice;
            set => SetField(ref _HitDice, value);
        }

        public int Damage {
            get => _Damage;
            set => SetField(ref _Damage, value);
        }

        public int ArmorClass {
            get => _ArmorClass;
            set => SetField(ref _ArmorClass, value);
        }

        public int Initiative {
            get => _Initiative;
            set => SetField(ref _Initiative, value);
        }

        public int Xp {
            get => _Xp;
            set {
                SetField(ref _Xp, value);
                RaisePropertyChanged(nameof(XpTotal));
                RaisePropertyChanged(nameof(NewLevelIndicator));

                if (Xp < TableLevelXp[Level + 1]) {
                    return;
                }

                Level++;
                Alert.FadeInfo("Level-Up!", $"{Name} is now level {Level}!");
            }
        }

        public int XpSession {
            get => _XpSession;
            set {
                SetField(ref _XpSession, value);
                RaisePropertyChanged(nameof(XpTotal));
                RaisePropertyChanged(nameof(NewLevelIndicator));
            }
        }

        public int XpTotal => Xp + XpSession;

        public string NewLevelIndicator => XpTotal >= TableLevelXp[Level + 1] ? "*" : "";

        public int XpSessionCount {
            get => _XpSessionCount;
            set => SetField(ref _XpSessionCount, value);
        }

        public bool IsDead {
            get => _IsDead;
            set => SetField(ref _IsDead, value);
        }

        public bool IsPoisoned {
            get => _IsPoisoned;
            set => SetField(ref _IsPoisoned, value);
        }

        public bool IsDazed {
            get => _IsDazed;
            set => SetField(ref _IsDazed, value);
        }

        public bool IsEnchanted {
            get => _IsEnchanted;
            set => SetField(ref _IsEnchanted, value);
        }

        public bool IsBurning {
            get => _IsBurning;
            set => SetField(ref _IsBurning, value);
        }

        public bool IsActiveCombatant {
            get => _IsActiveCombatant;
            set => SetField(ref _IsActiveCombatant, value);
        }

        public byte[] CharacterImageByte { get; set; }

        private BitmapImage _CharacterImage;

        public BitmapImage CharacterImage {
            get {
                if (_CharacterImage != null) {
                    return _CharacterImage;
                }

                var img = new BitmapImage();
                if (CharacterImageByte == null || CharacterImageByte.Length == 0) {
                    img.BeginInit();
                    if (IsNpc) {
                        img.StreamSource = Assembly.GetExecutingAssembly().GetResourceStream($"{NpcType}_01", "png");
                    } else {
                        img.UriSource = new Uri($"pack://application:,,,/CampaignMaster;component/Resources/Images/Icons/CharacterPortraitDefault.png");
                    }

                    img.EndInit();
                } else {
                    using var mem = new MemoryStream(CharacterImageByte);
                    mem.Position = 0;
                    img.BeginInit();
                    img.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.UriSource = null;
                    img.StreamSource = mem;
                    img.EndInit();
                }

                img.Freeze();

                _CharacterImage = img;

                return _CharacterImage;
            }

            set {
                SetField(ref _CharacterImage, value);
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_CharacterImage));
                using var ms = new MemoryStream();
                encoder.Save(ms);
                CharacterImageByte = ms.ToArray();
            }
        }

        public ICommand CommandToggleDead => new Command(() => IsDead = !IsDead);
        public ICommand CommandTogglePoisoned => new Command(() => IsPoisoned = !IsPoisoned);
        public ICommand CommandToggleDazed => new Command(() => IsDazed = !IsDazed);
        public ICommand CommandToggleEnchanted => new Command(() => IsEnchanted = !IsEnchanted);
        public ICommand CommandToggleBurning => new Command(() => IsBurning = !IsBurning);
        public ICommand CommandDamageCharacter => new Command(() => Damage++);
        public ICommand CommandSelectCharacterImage => new Command(SelectCharacterImage);
        public ICommand CommandRaiseXp => new Command(() => RaiseXp(1));
        public ICommand CommandRaiseXpDouble => new Command(() => RaiseXp(2));
        public ICommand CommandTakeOverSessionXp => new Command(TakeOverSessionXp);
        public ICommand CommandSelectColor => new Command(() => ColorCode = wndColorPicker.PickColor(ColorCode));

        public mdlCharacter() {
        }

        private void SelectCharacterImage() {
            using var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK || ofd.FileName.IsNullOrEmpty()) {
                return;
            }

            if (!File.Exists(ofd.FileName)) {
                return;
            }

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelHeight = 200;
            bi.DecodePixelWidth = 200;
            bi.UriSource = new Uri(ofd.FileName, UriKind.Relative);
            bi.EndInit();
            bi.Freeze();

            CharacterImage = bi;
        }

        public void ResetStatus() {
            IsActiveCombatant = false;
            IsDead = false;
            IsPoisoned = false;
            IsDazed = false;
            IsEnchanted = false;
            Damage = 0;
        }

        public void RaiseXp(int amountCount) {
            var xpAmount = amountCount * (TableLevelXp[Level + 1] / 100);
            XpSession += xpAmount;
            XpSessionCount += amountCount;
            Alert.FadeInfo($"Session XP raised for {Name} ({"".PadLeft(amountCount, '+')} = {xpAmount}XP)");
        }

        public void TakeOverSessionXp() {
            Xp += XpSession;

            XpSession = 0;
            XpSessionCount = 0;
        }

        private static readonly Random _Random = new();

        private static Dictionary<string, int> _NpcNumbers = new();
        private static Dictionary<string, List<int>> _UsedNpcNumbers = new();

        public static void ResetNpcCreation() {
            _NpcNumbers = new Dictionary<string, int>();
            _UsedNpcNumbers = new Dictionary<string, List<int>>();
        }

        public static mdlCharacter CreateNpc(string type) {
            if (!_NpcNumbers.ContainsKey(type)) {
                _NpcNumbers.Add(type, Assembly.GetExecutingAssembly().GetManifestResourceNames().Count(r => r.Contains("." + type + "_")));
                _UsedNpcNumbers.Add(type, new List<int>());
            }

            var range = Enumerable.Range(1, _NpcNumbers[type]).Except(_UsedNpcNumbers[type]);
            var index = _Random.Next(0, range.Count() - 1);
            var number = range.ElementAt(index);

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"CampaignMaster.Resources.Images.NPC.{type.ToLower()}_{number:D2}.png");
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = stream;
            img.EndInit();
            img.Freeze();

            var npc = new mdlCharacter {
                NpcType = type,
                CharacterImage = img
            };

            _UsedNpcNumbers[type].Add(number);

            if (_UsedNpcNumbers[type].Count == _NpcNumbers[type]) {
                _UsedNpcNumbers[type] = new List<int>();
            }

            return npc;
        }

        public static IEnumerable<string> GetNpcTypes() {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(r => r.Contains("NPC")).Select(r => r.Split('_').First().Split('.').Last()).Distinct();
        }

    }

    [Serializable]
    public class serCharacter {

        #region Attributes

        public int Strength { get; set; }

        public int Dexterity { get; set; }

        public int Constitution { get; set; }

        public int Intelligence { get; set; }

        public int Wisdom { get; set; }

        public int Charisma { get; set; }

        #endregion

        #region Skills

        public int Climb { get; set; }
        public int Jump { get; set; }
        public int Swim { get; set; }
        public int Balance { get; set; }
        public int EscapeArtist { get; set; }
        public int Hide { get; set; }
        public int MoveSilently { get; set; }
        public int OpenLock { get; set; }
        public int Ride { get; set; }
        public int SleightOfHand { get; set; }
        public int Tumble { get; set; }
        public int UseRope { get; set; }
        public int Concentration { get; set; }
        public int Appraise { get; set; }
        public int Craft { get; set; }
        public int DecipherScript { get; set; }
        public int DisableDevice { get; set; }
        public int Forgery { get; set; }
        public int Knowledge { get; set; }
        public int Search { get; set; }
        public int Spellcraft { get; set; }
        public int Heal { get; set; }
        public int Listen { get; set; }
        public int Profession { get; set; }
        public int SenseMotive { get; set; }
        public int Spot { get; set; }
        public int Survival { get; set; }
        public int Bluff { get; set; }
        public int Diplomacy { get; set; }
        public int Disguise { get; set; }
        public int GatherInformation { get; set; }
        public int HandleAnimal { get; set; }
        public int Intimidate { get; set; }
        public int Perform { get; set; }
        public int UseMagicDevice { get; set; }

        #endregion

        #region Saves

        public int Fortitude { get; set; }

        public int Reflex { get; set; }

        public int Will { get; set; }

        #endregion

        public string PlayerName { get; set; }
        public string ColorCode { get; set; }

        public string Name { get; set; }

        public string Class { get; set; }

        public string Race { get; set; }

        public int Age { get; set; }

        public int Level { get; set; }

        public int HitPoints { get; set; }

        public int HitDice { get; set; }

        public int ArmorClass { get; set; }

        public int Initiative { get; set; }

        public int Xp { get; set; }

        public byte[] Notes { get; set; }

        public byte[] CharacterImageByte { get; set; }

        public serCharacter(mdlCharacter chr) {
            foreach (var propertyInfo in chr.GetType().GetProperties().Where(p => !p.Name.Equals(nameof(Notes)))) {
                var property = GetType().GetProperty(propertyInfo.Name);
                if (property == null)
                    continue;

                property.SetValue(this, propertyInfo.GetValue(chr), null);
            }

            using var ms = new MemoryStream();
            chr.Notes.Save(ms);
            Notes = ms.ToArray();
        }

        public static implicit operator serCharacter(mdlCharacter chr) {
            return new serCharacter(chr);
        }

        public static implicit operator mdlCharacter(serCharacter chr) {
            var result = new mdlCharacter();
            foreach (var propertyInfo in chr.GetType().GetProperties().Where(p => !p.Name.Equals(nameof(Notes)))) {
                var property = result.GetType().GetProperty(propertyInfo.Name);
                if (property == null)
                    continue;

                property.SetValue(result, propertyInfo.GetValue(chr), null);
            }

            if (chr.Notes == null) {
                return result;
            }

            using var ms = new MemoryStream(chr.Notes);
            new StrokeCollection(ms).ToList().ForEach((s) => result.Notes.Add(s));

            return result;
        }

    }

}