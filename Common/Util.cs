using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfChartV1.Mvvm.UserControls;

namespace WpfChartV1.Common
{
    class Util
    {
        /// <summary>
        /// 目盛りの凸部分の長さ
        /// </summary>
        internal const int ScaleLineLength = 5;

        /// <summary>
        /// 目盛りに表示する値を取得する
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="splitCount">分割数</param>
        /// <returns></returns>
        internal static IEnumerable<string> GetScaleStrings(double min, double max, int splitCount, string format)
        {
            return Enumerable.Range(0, splitCount + 1)
                .Select(i => (min + ((max - min) / splitCount * i)))
                .Select(i => string.Format(format, i));
        }

        /// <summary>
        /// 目盛りに表示する値を取得する
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="splitCount">分割数</param>
        /// <param name="dateFormat">日付表示ﾌｫｰﾏｯﾄ</param>
        /// <param name="timeFormat">時刻表示ﾌｫｰﾏｯﾄ</param>
        /// <returns></returns>
        internal static IEnumerable<string> GetScaleStrings(DateTime min, DateTime max, int splitCount, string dateFormat, string timeFormat)
        {
            var dates = Enumerable.Range(0, splitCount + 1)
                .Select(i => (min + TimeSpan.FromTicks(((max - min).Ticks / splitCount * i))))
                .ToArray();
            var prev = min.Date != max.Date
                ? default(DateTime)
                : min;

            return dates.Select(d =>
            {
                var tStr = string.Format(timeFormat, d);
                var dStr = string.Format(dateFormat, d);

                try
                {
                    if (prev.Date < d.Date)
                    {
                        // 前の目盛りと日付が異なる場合は日付も表示
                        return $"{tStr}\n{dStr}";
                    }
                    else
                    {
                        // 時間のみ表示
                        return tStr;
                    }
                }
                finally
                {
                    prev = d;
                }
            })
            .ToArray();
        }

        /// <summary>
        /// 目盛りに表示する値を取得する
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="splitCount">分割数</param>
        /// <param name="dateFormat">日付表示ﾌｫｰﾏｯﾄ</param>
        /// <param name="timeFormat">時刻表示ﾌｫｰﾏｯﾄ</param>
        /// <returns></returns>
        internal static IEnumerable<string> GetScaleStrings(TimeSpan min, TimeSpan max, int splitCount, string dateFormat, string timeFormat)
        {
            var dates = Enumerable.Range(0, splitCount + 2)
                .Select(i => (min + TimeSpan.FromTicks(((max - min).Ticks / splitCount * i))))
                .ToArray();
            var prev = min.Days != max.Days
                ? default(TimeSpan)
                : min;

            return dates.Select(t =>
            {
                var tStr = string.Format(timeFormat, t);
                var dStr = string.Format(dateFormat, t);

                try
                {
                    if (prev.Days < t.Days)
                    {
                        // 前の目盛りと日付が異なる場合は日付も表示
                        return $"{tStr}\n{dStr}";
                    }
                    else
                    {
                        // 時間のみ表示
                        return tStr;
                    }
                }
                finally
                {
                    prev = t;
                }
            })
            .ToArray();
        }

        /// <summary>
        /// 表題文字列を描写用書式ｵﾌﾞｼﾞｪｸﾄで取得する
        /// </summary>
        /// <param name="text">表題文字列</param>
        /// <param name="color">表題の色</param>
        /// <returns>描写用書式ｵﾌﾞｼﾞｪｸﾄ</returns>
        internal static FormattedText GetFormattedText(string text, Brush brush, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            return new FormattedText(text,
                new CultureInfo("ja-jp"),
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                fontSize,
                brush
            );
        }

        ///// <summary>
        ///// 表題文字列を描写用書式ｵﾌﾞｼﾞｪｸﾄで取得する
        ///// </summary>
        ///// <param name="text">表題文字列</param>
        ///// <param name="color">表題の色</param>
        ///// <returns>描写用書式ｵﾌﾞｼﾞｪｸﾄ</returns>
        //internal static FormattedText GetFormattedText(string text, Chart c, Brush brush)
        //{
        //    return new FormattedText(text,
        //        new CultureInfo("ja-jp"),
        //        FlowDirection.LeftToRight,
        //        new Typeface(c.FontFamily, c.FontStyle, c.FontWeight, c.FontStretch),
        //        c.FontSize,
        //        brush
        //    );
        //}

        ///// <summary>
        ///// 表題文字列を描写用書式ｵﾌﾞｼﾞｪｸﾄで取得する
        ///// </summary>
        ///// <param name="text">表題文字列</param>
        ///// <param name="color">表題の色</param>
        ///// <returns>描写用書式ｵﾌﾞｼﾞｪｸﾄ</returns>
        //internal static FormattedText GetFormattedText(string text, Chart c)
        //{
        //    return new FormattedText(text,
        //        new CultureInfo("ja-jp"),
        //        FlowDirection.LeftToRight,
        //        new Typeface(c.FontFamily, c.FontStyle, c.FontWeight, c.FontStretch),
        //        c.FontSize,
        //        c.FreezeForeground
        //    );
        //}

        internal static PathFigure CreateLine(IEnumerable<Point> points)
        {
            // ﾊﾟｽを作成
            return new PathFigure(
                points.FirstOrDefault(),
                points.Skip(1).Select(p => new LineSegment() { Point = p, IsSmoothJoin = true, IsStroked = true}),
                false
            );
        }


        /// <summary>
        /// <code>DrawingVisual</code>に設定されている<code>Drawing</code>ｲﾝｽﾀﾝｽ全てのｱﾝﾁｴｲﾘｱｽを解除する
        /// </summary>
        /// <param name="dv"><code>DwawingVisual</code></param>
        /// <returns>ｱﾝﾁｴｲﾘｱｽを解除した<code>DrawingVisual</code></returns>
        internal static DrawingVisual SetRenderOptions(DrawingVisual dv)
        {
            var drawingGroup = dv.Drawing;

            foreach (var d in GetDrawings(drawingGroup))
            {
                if (!d.IsSealed)
                {
                    RenderOptions.SetBitmapScalingMode(d, BitmapScalingMode.Fant);
                    RenderOptions.SetEdgeMode(d, EdgeMode.Aliased);
                    d.Freeze();
                }
            }

            dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                ctx.DrawDrawing(drawingGroup);
            }
            
            return dv;
        }

        /// <summary>
        /// <code>DrawingGroup</code>に格納された全ての子ｲﾝｽﾀﾝｽを階層構造を考慮して取得します
        /// </summary>
        /// <param name="g"><code>DrawingGroup</code></param>
        /// <returns>階層を考慮して取得した<code>Drawing配列</code></returns>
        private static IEnumerable<Drawing> GetDrawings(DrawingGroup g)
        {
            if (g == null)
            {
                yield break;
            }

            foreach (var d in g.Children)
            {
                yield return d;

                foreach (var c in GetDrawings(d as DrawingGroup))
                {
                    yield return c;
                }
            }
        }
    }
}
