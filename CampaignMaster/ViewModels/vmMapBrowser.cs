using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Runtime.Serialization.Formatters.Binary;

using SamCorp.WPF.Logging;
using SamCorp.WPF.Commands;
using SamCorp.WPF.Models;
using SamCorp.WPF.ViewModels;
using SamCorp.WPF.Alerts;

using CampaignMaster.Data;
using CampaignMaster.Models;

namespace CampaignMaster.ViewModels
{
    public class vmMapBrowser : ViewModelBase
    {
        public ObservableCollection<claMap> Maps { get; set; }

        public vmMapBrowser()
        {
            try
            {
                Maps = new ObservableCollection<claMap>
                {
                    null, null, null, null, null, null, null, null
                };
            }
            catch (Exception ex) { Alert.Error(ex); }
        }

        public void LoadMaps()
        {
            try
            {
                Maps.Clear();
                string[] files = Directory.GetFiles(App.CurrentCampaign.DirectoryMaps, "*.cmm", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    BinaryFormatter AFormatter = new BinaryFormatter();
                    using (FileStream fs = File.Open(file, FileMode.Open))
                        Maps.Add((claMap)AFormatter.Deserialize(fs));
                }
            }
            catch (Exception ex) { Log.Error(ex); }
        }
    }
}
