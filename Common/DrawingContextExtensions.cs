using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfChartV1.Common
{
    public static class DrawingContextExtensions
    {
        public static void DrawImage(this DrawingContext dc, ImageSource bitmap)
        {
            dc.DrawImage(bitmap, new Rect(0, 0, bitmap.Width, bitmap.Height));
        }
    }
}
