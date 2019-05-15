using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfChartV1.Mvvm.UserControls
{
    public class DateOrTimeSeries
    {
        /// <summary>
        /// ｼﾘｰｽﾞのｸﾞﾗﾌ線の色を設定、または取得します。
        /// </summary>
        public Color Foreground { get; set; }

        /// <summary>
        /// ｼﾘｰｽﾞのｸﾞﾗﾌ線の太さを設定、または取得します。
        /// </summary>
        public double Thickness { get; set; }

        /// <summary>
        /// 線を繋ぐ頂点
        /// </summary>
        public DateOrTimeLine[] Lines { get; set; }

        /// <summary>
        /// 横軸の開始日
        /// </summary>
        public DateTime XBeginDate { get; set; }

    }
}
