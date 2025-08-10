using System;
using System.Collections.ObjectModel;
using System.Linq;
using CampaignMaster.Models;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.ViewModels {

    public class vmCharacterToolBar : ViewModelBase {

        private ObservableCollection<mdlCharacter> _Characters = new();

        public ObservableCollection<mdlCharacter> Characters {
            get => _Characters;
            set => SetField(ref _Characters, value);
        }

        public vmCharacterToolBar() {
            App.CurrentCampaign ??= new mdlCampaign();

            App.CampaignChanged += App_CampaignChanged;
            CampaignCharactersChanged();
        }

        private void App_CampaignChanged(object sender, EventArgs e) {
            CampaignCharactersChanged();
        }

        private void CampaignCharactersChanged() {
            Characters.Clear();

            if (App.CurrentCampaign == null) {
                return;
            }

            foreach (var character in App.CurrentCampaign.Characters.Where(c => c.Name != null && !c.Name.Contains('<'))) {
                Characters.Add(character);
            }
        }

    }

}