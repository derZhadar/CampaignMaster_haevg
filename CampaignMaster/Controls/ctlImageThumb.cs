using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CampaignMaster.Controls
{
    public class ctlImageThumb : Thumb
    {
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), typeof(ctlImageThumb), new PropertyMetadata());
        public ImageSource Image { get => (ImageSource)GetValue(ImageProperty); set => SetValue(ImageProperty, value); }

        public ctlImageThumb() { }
    }
}
