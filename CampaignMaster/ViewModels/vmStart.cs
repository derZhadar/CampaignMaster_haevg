using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Input;
using CampaignMaster.Models;
using SamCorp.WPF.Alerts;
using SamCorp.WPF.Commands;
using SamCorp.WPF.Controls;
using SamCorp.WPF.Logging;
using SamCorp.WPF.ViewModels;

namespace CampaignMaster.ViewModels {

    public class vmStart : ViewModelBase {

        public ObservableCollection<mdlCampaign> Campaigns { get; set; }

        public ICommand CommandOpenCampaign {
            get => new Command<ICloseable>(OpenCampaign);
        }

        public vmStart() {
            try {
                Campaigns = new ObservableCollection<mdlCampaign>();
                LoadCampaigns();
            } catch (Exception ex) {
                Alert.Error(ex);
            }
        }

        private void OpenCampaign(ICloseable closeable) {
            //try
            //{
            //    System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            //    ofd.InitialDirectory = Environment.CurrentDirectory;
            //    ofd.Filter = "Campaign files(*.cmp) | *.cmp";

            //    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    {
            //        BinaryFormatter formatter = new BinaryFormatter();
            //        using (Stream stream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
            //        {
            //            mdlCampaign cmp = (serCampaign)formatter.Deserialize(stream);

            //            wndCampaignEditor ce = new wndCampaignEditor();
            //            ce.DataContext = new vmCampaignEditor() { Campaign = cmp };
            //            ce.Show();
            //            stream.Close();
            //            closeable?.Close();
            //        }
            //    }
            //}
            //catch (Exception ex) { Alert.Error(ex); }
        }

        public void LoadCampaigns() {
            try {
                var files = Directory.GetFiles(Environment.CurrentDirectory, "*.cmp", SearchOption.AllDirectories);
                foreach (var file in files) {
                    var AFormatter = new BinaryFormatter();
                    using (var fs = File.Open(file, FileMode.Open))
                        Campaigns.Add((serCampaign)AFormatter.Deserialize(fs));
                }
            } catch (Exception ex) {
                Log.Error(ex);
            }
        }

    }

}