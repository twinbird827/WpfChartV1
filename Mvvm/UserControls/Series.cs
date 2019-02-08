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
    public abstract class Series
    {
        /// <summary>
        /// ｼﾘｰｽﾞの最小値を設定、または取得します。
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// ｼﾘｰｽﾞの最大値を設定、または取得します。
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// ｼﾘｰｽﾞの表示ﾌｫｰﾏｯﾄを設定、または取得します。
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// ｼﾘｰｽﾞの縦軸ﾀｲﾄﾙを設定、または取得します。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// ｼﾘｰｽﾞのｸﾞﾗﾌ線の色を設定、または取得します。
        /// </summary>
        public Color Foreground { get; set; }

        /// <summary>
        /// ｼﾘｰｽﾞのｸﾞﾗﾌ線の太さを設定、または取得します。
        /// </summary>
        public double Thickness { get; set; }

    }
}
