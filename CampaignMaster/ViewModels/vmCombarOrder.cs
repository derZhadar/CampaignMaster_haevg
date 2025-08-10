using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CampaignMaster.Models;
using SamCorp.WPF.Alerts;
using SamCorp.WPF.Commands;
using SamCorp.WPF.Extended;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.ViewModels {

    public class vmCombatOrder : ViewModelBase {

        private MediaPlayerExtended _MediaPlayer = new();

        private ObservableCollection<mdlCharacter> _Characters = new();
        private ObservableCollection<mdlCharacter> _Npcs = new();

        public ObservableCollection<mdlCharacter> Characters {
            get => _Characters;
            set => SetField(ref _Characters, value);
        }

        public ObservableCollection<mdlCharacter> Npcs {
            get => _Npcs;
            set => SetField(ref _Npcs, value);
        }

        private int _CombatantIndex = 1;
        private bool _ShowActiveCombatant;

        public bool ShowActiveCombatant {
            get => _ShowActiveCombatant;
            set => SetField(ref _ShowActiveCombatant, value);
        }

        public mdlCharacter ActiveCombatant {
            get => App.CurrentCampaign.ActiveCombatant;
            set {
                if (ActiveCombatant != null) {
                    ActiveCombatant.IsActiveCombatant = false;
                }

                App.CurrentCampaign.ActiveCombatant = value;

                if (ActiveCombatant != null) {
                    ActiveCombatant.IsActiveCombatant = true;
                }

                RaisePropertyChanged();
            }
        }

        public ObservableCollection<mdlCharacter> CombatOrder => App.CurrentCampaign.CombatOrder;

        public ICommand CommandAddCharacter => new Command<mdlCharacter>(AddCharacter);
        public ICommand CommandMoveCharacterUp => new Command<mdlCharacter>(MoveCharacterUp);
        public ICommand CommandMoveCharacterDown => new Command<mdlCharacter>(MoveCharacterDown);
        public ICommand CommandClearCombatOrder => new Command(ClearCombatOrder);
        public ICommand CommandNextCharacter => new Command(NextCharacter);

        public ICommand CommandToggleCombatOrderVisibility => new Command(() => {
            if (CombatOrder.Any()) {
                ShowActiveCombatant = !_ShowActiveCombatant;
            } else {
                ShowActiveCombatant = false;
            }
        });

        public vmCombatOrder() {
            if (App.CurrentCampaign?.Characters is null) {
                App.CurrentCampaign = new mdlCampaign();
            }

            App.CampaignChanged += App_CampaignChanged;
            CampaignCharactersChanged();

            //if (Application.Current.MainWindow != null) {
            //    return;
            //}

            AddCharacter(App.CurrentCampaign.Characters[0]);
            AddCharacter(App.CurrentCampaign.Characters[1]);
            AddCharacter(mdlCharacter.CreateNpc("guard"));
            AddCharacter(App.CurrentCampaign.Characters[2]);
            AddCharacter(mdlCharacter.CreateNpc("pirate"));
            AddCharacter(App.CurrentCampaign.Characters[3]);
            AddCharacter(mdlCharacter.CreateNpc("guard"));
            AddCharacter(mdlCharacter.CreateNpc("thug"));
            AddCharacter(mdlCharacter.CreateNpc("pirate"));

            CombatOrder[0].IsDead = true;
            CombatOrder[2].IsPoisoned = true;
            CombatOrder[4].IsDazed = true;
            CombatOrder[6].IsEnchanted = true;
            CombatOrder[8].IsBurning = true;
            ActiveCombatant = CombatOrder[1];

            foreach (var npcType in mdlCharacter.GetNpcTypes()) {
                Npcs.Add(new mdlCharacter { NpcType = npcType });
            }
        }

        private void App_CampaignChanged(object sender, EventArgs e) {
            CampaignCharactersChanged();
        }

        private void CampaignCharactersChanged() {
            Characters.Clear();
            ClearCombatOrder();

            if (App.CurrentCampaign == null) {
                return;
            }

            foreach (var character in App.CurrentCampaign.Characters.Where(c => c.Name != null && !c.Name.Contains('<'))) {
                Characters.Add(character);
            }
        }

        private void AddCharacter(mdlCharacter character) {
            character.ResetStatus();
            CombatOrder.Insert(CombatOrder.Count, character.IsNpc ? mdlCharacter.CreateNpc(character.NpcType) : character);
            RaisePropertyChanged(nameof(CombatOrder));
        }

        private void MoveCharacterUp(mdlCharacter character) {
            MoveCharacter(character, up: true);
        }

        private void MoveCharacterDown(mdlCharacter character) {
            MoveCharacter(character, up: false);
        }

        private void MoveCharacter(mdlCharacter character, bool up) {
            var index = CombatOrder.IndexOf(character);
            if ((up && index == 0) || (!up && index == CombatOrder.Count - 1)) {
                return;
            }

            CombatOrder.Move(index, up ? index - 1 : index + 1);
            RaisePropertyChanged(nameof(CombatOrder));

            _CombatantIndex = CombatOrder.IndexOf(ActiveCombatant);
        }

        private void ClearCombatOrder() {
            CombatOrder.Clear();
            _CombatantIndex = -1;
            ActiveCombatant = null;
            RaisePropertyChanged(nameof(CombatOrder));

            mdlCharacter.ResetNpcCreation();

            if (!_MediaPlayer.IsPlaying) {
                return;
            }

            _MediaPlayer.Stop();
            Alert.FadeInfo("Stopped ambiente", "Battle");
        }

        private void NextCharacter() {
            _CombatantIndex++;
            if (_CombatantIndex >= CombatOrder.Count) {
                _CombatantIndex = 0;
            }

            ActiveCombatant = CombatOrder[_CombatantIndex];

            if (_MediaPlayer.IsPlaying) {
                return;
            }

            _MediaPlayer.Open(new Uri(@$"Resources\Sounds\battle.mp3", UriKind.Relative));
            _MediaPlayer.PlayLoop();
            Alert.FadeInfo("Playing ambiente", "Battle");
        }

    }

}