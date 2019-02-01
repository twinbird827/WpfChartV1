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
        //public Brush Foreground { get; set; }

        public Brush Foreground
        {
            get { return new SolidColorBrush(_FreezeForeground); }
            set
            {
                var brush = value as SolidColorBrush;
                _FreezeForeground = brush.Color;
            }
        }
        private Color _FreezeForeground;

        /// <summary>
        /// ｼﾘｰｽﾞのｸﾞﾗﾌ線の太さを設定、または取得します。
        /// </summary>
        public double Thickness { get; set; }

        /// <summary>
        /// ｼﾘｰｽﾞの縦軸ﾍｯﾀﾞを描写します。
        /// </summary>
        /// <param name="c">ﾁｬｰﾄｲﾝｽﾀﾝｽ</param>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        /// <param name="beginX">描写開始位置(X座標)</param>
        internal virtual void DrawHeader(ChartCreator c, DrawingContext dc, double beginX)
        {
            Util.GetScaleStrings(Min, Max, c.ScaleSplitCountY, Format)
                .Select(s => c.GetFormattedText(s))
                .Select((s, index) =>
                {
                    var x = beginX - c.ScaleLineLength;
                    var y = c.GraphHeight - c.GraphHeight / c.ScaleSplitCountY * index;

                    // 文字を描写
                    dc.DrawText(s, new Point(x - s.Width, y - s.Height / 2));

                    // 目盛り線を描写
                    dc.DrawLine(
                        c.FreezePen,
                        new Point(x, y),
                        new Point(x + c.ScaleLineLength, y)
                    );
                    return s;
                })
                .ToArray();

            // Y軸縦線
            dc.DrawLine(
                c.FreezePen,
                new Point(beginX, 0),
                new Point(beginX, c.GraphHeight)
            );

            // Y軸標題
            var text = c.GetFormattedText(Title);
            var point = new Point(beginX + c.ScaleLineLength, c.GraphHeight / 2 + text.Width / 2);

            // Y軸標題の中心で反時計回りに90度回転させる
            dc.PushTransform(new RotateTransform(-90, point.X, point.Y));

            // 表題位置補正
            point.Y -= GetHeaderWidth(c);
            dc.DrawText(text, point);
            dc.Pop();

        }

        /// <summary>
        /// 縦軸ﾍｯﾀﾞの幅を取得します。
        /// </summary>
        /// <param name="c">ﾁｬｰﾄｲﾝｽﾀﾝｽ</param>
        internal virtual double GetHeaderWidth(ChartCreator c)
        {
            // ﾀｲﾄﾙの幅(縦表記するので高さが幅になる)
            var titleWidth = c.GetFormattedText(Title).Height;
            // 目盛りの幅(目盛り表記の幅の中で最大値)
            var memoriWidth = Util.GetScaleStrings(Min, Max, c.ScaleSplitCountY, Format)
                .Select(s => c.GetFormattedText(s).Width)
                .Max();
            // 戻り値(ﾀｲﾄﾙの幅＋目盛りの幅＋目盛りの長さ)
            return titleWidth + memoriWidth + c.ScaleLineLength;
        }

        internal virtual double GetHeaderWidth(ChartCreatorEx c)
        {
            // ﾀｲﾄﾙの幅(縦表記するので高さが幅になる)
            var titleWidth = c.GetFormattedText(Title).Height;
            // 目盛りの幅(目盛り表記の幅の中で最大値)
            var memoriWidth = Util.GetScaleStrings(Min, Max, c.ScaleSplitCountY, Format)
                .Select(s => c.GetFormattedText(s).Width)
                .Max();
            // 戻り値(ﾀｲﾄﾙの幅＋目盛りの幅＋目盛りの長さ)
            return titleWidth + memoriWidth + c.ScaleLineLength;
        }

        /// <summary>
        /// ｼﾘｰｽﾞのｸﾞﾗﾌを描写します。
        /// </summary>
        /// <param name="c">ﾁｬｰﾄｲﾝｽﾀﾝｽ</param>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        internal void DrawSeries(ChartCreator c, DrawingContext dc)
        {
            
            var FreezeForeground = Foreground;
            var FreezePen = new Pen(FreezeForeground, Thickness);
            if (FreezePen.CanFreeze) FreezePen.Freeze();

            dc.DrawGeometry(null, FreezePen, CreateGeometry(c));
        }

        /// <summary>
        /// 描写用ﾃﾞｰﾀを間引きます。
        /// </summary>
        /// <param name="after">間引いた後の件数</param>
        internal protected abstract void ThinningOut(double after);

        /// <summary>
        /// ｼﾘｰｽﾞのｸﾞﾗﾌを作成します。
        /// </summary>
        /// <param name="c">ﾁｬｰﾄｲﾝｽﾀﾝｽ</param>
        protected abstract Geometry CreateGeometry(ChartCreator c);

        /// <summary>
        /// ｼﾘｰｽﾞのｸﾞﾗﾌを描写します。
        /// </summary>
        /// <param name="c">ﾁｬｰﾄｲﾝｽﾀﾝｽ</param>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        internal void DrawSeries(ChartCreatorEx c, WriteableBitmap dc)
        {
            CreateGeometry(c, dc, ((SolidColorBrush)Foreground).Color);
        }

        /// <summary>
        /// ｼﾘｰｽﾞのｸﾞﾗﾌを作成します。
        /// </summary>
        /// <param name="c">ﾁｬｰﾄｲﾝｽﾀﾝｽ</param>
        protected abstract void CreateGeometry(ChartCreatorEx c, WriteableBitmap dc, Color color);

    }
}
