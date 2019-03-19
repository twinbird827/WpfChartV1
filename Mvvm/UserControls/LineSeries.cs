using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfChartV1.Common;

namespace WpfChartV1.Mvvm.UserControls
{
    public class LineSeries : Series
    {
        /// <summary>
        /// 線を繋ぐ頂点
        /// </summary>
        public Line[] Lines { get; set; }

        public DateTime XStartDate { get; set; }

    }
}
