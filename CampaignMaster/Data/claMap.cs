using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace CampaignMaster.Data {

    [Serializable]
    public class claMap {

        public string Name { get; set; }
        public string CampaignName { get; set; }
        public byte[] Strokes { get; set; }
        public byte[] Scratches { get; set; }
        public List<string> Shapes { get; set; }
        public List<string> ShapesCloned { get; set; }
        public string BackgroundImage { get; set; }
        public string ScratchImage { get; set; }
        public DateTime EditDateTime { get; set; }
        public string DateTimeInfo => EditDateTime.ToString("yyyy-MM-dd HH:mm");

        public byte[] PreviewImageByte { get; set; }

        public BitmapImage PreviewImage {
            get {
                var img = new BitmapImage();
                using (var mem = new MemoryStream(PreviewImageByte)) {
                    mem.Position = 0;
                    img.BeginInit();
                    img.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.UriSource = null;
                    img.StreamSource = mem;
                    img.EndInit();
                }

                img.Freeze();

                return img;
            }
        }

        public string Key => CampaignName + "_" + Name;

        public claMap() {
            Shapes = new List<string>();
            ShapesCloned = new List<string>();
        }

        public override string ToString() {
            return Key;
        }

    }

}