using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HideezClient.Extension
{
    static class VisualEx
    {
        public static DpiTransform GetDpiTransform(this Visual visual)
        {
            PresentationSource source = PresentationSource.FromVisual(visual);

            var dpiTransform = new DpiTransform();
            if (source != null)
            {
                dpiTransform.X = source.CompositionTarget.TransformToDevice.M11;
                dpiTransform.Y = source.CompositionTarget.TransformToDevice.M22;
            }

            return dpiTransform;
        }
    }

    class DpiTransform
    {
        public DpiTransform()
        {
            this.X = 1;
            this.Y = 1;
        }

        public DpiTransform(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }
    }
}
