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
using System.Windows.Ink;
using System.Collections.Specialized;

using SamCorp.WPF.Logging;
using SamCorp.WPF.Commands;
using SamCorp.WPF.Models;
using SamCorp.WPF.ViewModels;
using SamCorp.WPF.Alerts;

using CampaignMaster.Data;
using CampaignMaster.Models;

namespace CampaignMaster.ViewModels
{
    public class vmNotes : ViewModelBase
    {
        public StrokeCollection Notes { get; set; }

        public vmNotes()
        {
            Notes = new StrokeCollection();
            (Notes as INotifyCollectionChanged).CollectionChanged += Strokes_CollectionChanged;

            App.CampaignChanged += App_CampaignChanged;
        }

        private void App_CampaignChanged(object sender, EventArgs e)
        {
            Notes.Clear();
            foreach (Stroke s in App.CurrentCampaign.Notes)
                Notes.Add(s);
        }

        private void Strokes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Notes = (StrokeCollection)sender;
            App.CurrentCampaign.Notes = (StrokeCollection)sender;
        }
    }
}
