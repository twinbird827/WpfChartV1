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

        private PathFigure CreateLine(IEnumerable<Line> lines, ChartCreator c, double zY, double zX)
        {
            // ﾗｲﾝを座標位置に変換
            var points = lines
                .Select(line => 
                {
                    var x = (line.X - c.BeginTimeX).Ticks * zX;
                    var y = (line.Y - Min) * zY;

                    //y = y >= Max * zY
                    //    ? y - 1
                    //    : y <= Min * zY
                    //    ? y + 1
                    //    : y;
                    return new Point(x, y);
                });

            return Util.CreateLine(points);
        }

        internal protected override void ThinningOut(double after)
        {
            var lineCount = Lines.Length;
            var afterCount = after * 3 / 2;
            if (afterCount < lineCount)
            {
                //var tmp = Lines;
                //Lines = Enumerable.Range(0, (int)afterCount)
                //    .Select(i => (int)Math.Ceiling(i * lineCount / afterCount))
                //    .Select(i => tmp.ElementAt(i));
                var tmp = Enumerable.Range(0, (int)afterCount)
                    .Select(i => (int)Math.Ceiling(i * lineCount / afterCount))
                    .Select(i => Lines[i])
                    .ToArray();
                Lines = tmp;
            }
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

        protected override void CreateGeometry(ChartCreatorEx c, WriteableBitmap dc, Color color)
        {
            // Y座標の倍率
            var zY = c.GraphHeight / (Max - Min);
            var zX = c.ZoomRatioX;

            if (Lines != null && Lines.Any())
            {
                Line before = null;
                foreach (var line in Lines)
                {
                    if (before != null)
                    {
                        dc.DrawLine(c.OtherThanCanvas, CreatePoint(c, before, zY, zX), CreatePoint(c, line, zY, zX), color);
                    }
                    before = line;
                }
            }
        }

        private Point CreatePoint(ChartCreatorEx c, Line line, double zY, double zX)
        {
            var x = (line.X - c.BeginTimeX).Ticks * zX;
            var y = (line.Y - Min) * zY;
            return new Point(x, y);
        }
    }
}
