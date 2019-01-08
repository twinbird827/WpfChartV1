using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfChartV1.Common;

namespace WpfChartV1.Mvvm.UserControls
{
    public class LineSeries : Series
    {
        /// <summary>
        /// 線を繋ぐ頂点
        /// </summary>
        public IEnumerable<Line> Lines { get; set; }

        private PathFigure CreateLine(IEnumerable<Line> lines, ChartCreator c, double zY, double zX)
        {
            // ﾗｲﾝを座標位置に変換
            var points = lines
                .Select(line => new Point((line.X - c.BeginTimeX).Ticks * zX, (line.Y - Min) * zY));

            return Util.CreateLine(points);
        }

        internal protected override void ThinningOut(double after)
        {
            var lineCount = Lines.Count();
            var afterCount = after * 3 / 2;
            if (afterCount < lineCount)
            {
                var tmp = Lines;
                Lines = Enumerable.Range(0, (int)afterCount)
                    .Select(i => (int)Math.Ceiling(i * lineCount / afterCount))
                    .Select(i => tmp.ElementAt(i));
            }
            else
            {
                Lines = Lines;
            }
            //var lineCount = Lines.Count();
            //var afterCount = after * 3 / 2;
            //if (afterCount < lineCount)
            //{
            //    Lines = Enumerable.Range(0, (int)afterCount)
            //        .Select(i => (int)Math.Ceiling(i * lineCount / afterCount))
            //        .Select(i => Lines.ElementAt(i)).ToArray();
            //}
            //else
            //{
            //    Lines = Lines.ToArray();
            //}
        }

        ///// <summary>
        ///// 最大公約数を求める
        ///// </summary>
        ///// <param name="a"></param>
        ///// <param name="b"></param>
        ///// <returns></returns>
        //private int Gcd(int a, int b)
        //{
        //    if (a < b)
        //        // 引数を入替えて自分を呼び出す
        //        return Gcd(b, a);
        //    while (b != 0)
        //    {
        //        var remainder = a % b;
        //        a = b;
        //        b = remainder;
        //    }
        //    return a;
        //}

        protected override Geometry CreateGeometry(ChartCreator c)
        {
            // Y座標の倍率
            var zY = c.GraphHeight / (Max - Min);
            var zX = c.ZoomRatioX;

            if (Lines != null && Lines.Any())
            {
                return new PathGeometry(new[] { CreateLine(Lines, c, zY, zX) });
            }
            else
            {
                return new PathGeometry();
            }
        }
    }
}
