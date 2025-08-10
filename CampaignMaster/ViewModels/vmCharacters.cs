using System;
using System.Collections.ObjectModel;
using CampaignMaster.Models;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.ViewModels {

    public class vmCharacters : ViewModelBase {

        private mdlCharacter _selectedCharacter;

        public mdlCharacter SelectedCharacter {
            get => _selectedCharacter;
            set => SetField(ref _selectedCharacter, value);
        }

        public ObservableCollection<mdlCharacter> Characters => App.CurrentCampaign.Characters;

        public vmCharacters() {
            App.CampaignChanged += App_CampaignChanged;

            if (App.CurrentCampaign?.Characters is null) {
                App.CurrentCampaign = new mdlCampaign();
                SelectedCharacter = Characters[3];
            }

            Characters.CollectionChanged += (sender, args) => RaisePropertyChanged(nameof(Characters));
        }

        private void App_CampaignChanged(object sender, EventArgs e) {
            RaisePropertyChanged(nameof(Characters));
            SelectedCharacter = null;
        }

    }

}