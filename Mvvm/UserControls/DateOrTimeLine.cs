using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfChartV1.Mvvm.UserControls
{
    public class DateOrTimeLine
    {
        /// <summary>
        /// 線を繋ぐ頂点のX座標
        /// </summary>
        public DateTime X { get; set; }

        /// <summary>
        /// 線を繋ぐ頂点のY座標
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// ｺﾝｽﾄﾗｸﾀ
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        public DateOrTimeLine(DateTime x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
