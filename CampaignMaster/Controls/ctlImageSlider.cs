using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignMaster.Controls
{
    public class ctlImageSlider : Slider
    {
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), typeof(ctlImageSlider), new PropertyMetadata());
        public ImageSource Image { get => (ImageSource)GetValue(ImageProperty); set => SetValue(ImageProperty, value); }

        public ctlImageSlider() { }
    }
}
