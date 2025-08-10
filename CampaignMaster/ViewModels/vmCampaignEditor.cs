using System;
using System.Windows.Input;
using CampaignMaster.Models;
using SamCorp.WPF.Alerts;
using SamCorp.WPF.Commands;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.ViewModels {

    public class vmCampaignEditor : ViewModelBase {

        public mdlCampaign Campaign => App.CurrentCampaign;

        public ICommand CommandSaveCampaign => new Command(SaveCampaign);
        public ICommand CommandAddNewCharacter => new Command(AddNewCharacter);
        public ICommand CommandDeleteCharacter => new Command<mdlCharacter>(DeleteCharacter);

        public vmCampaignEditor() {
            if (App.CurrentCampaign?.Characters is null) {
                App.CurrentCampaign = new mdlCampaign();
            }

            App.CampaignChanged += App_CampaignChanged;
        }

        private void App_CampaignChanged(object sender, EventArgs e) {
            RaisePropertyChanged(nameof(Campaign));
        }

        private void AddNewCharacter() {
            if (Campaign.Characters.Count >= 5) {
                return;
            }

            Campaign.Characters.Add(new mdlCharacter {
                Name = "<New Character>",
            });

            RaisePropertyChanged(nameof(Campaign));
        }

        private void DeleteCharacter(mdlCharacter character) {
            if (Campaign.Characters.Count <= 0) {
                return;
            }

            Campaign.Characters.Remove(character);
            RaisePropertyChanged(nameof(Campaign));
        }

        private async void SaveCampaign() {
            await Campaign.Save();
            Alert.FadeInfo("Campaign saved");
        }

    }

}