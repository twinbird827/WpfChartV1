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
    static class WriteableBitmapExtensions
    {
        /// <summary>
        /// 線を描写します。
        /// </summary>
        /// <param name="bitmap">ﾋﾞｯﾄﾏｯﾌﾟｲﾒｰｼﾞ</param>
        /// <param name="margin">余白(左と上を使用)</param>
        /// <param name="begin">開始位置</param>
        /// <param name="end">終了位置</param>
        /// <param name="color">色</param>
        public static void DrawLine(this WriteableBitmap bitmap, Thickness margin, Point begin, Point end, Color color)
        {
            begin.X += margin.Left;
            begin.Y += margin.Top;
            end.X += margin.Left;
            end.Y += margin.Top;
            bitmap.DrawLine(begin, end, color);
            //bitmap.DrawLine(new Point(begin.X + margin.Left, begin.Y + margin.Top), new Point(end.X + margin.Left, end.Y + margin.Top), color);
        }

        /// <summary>
        /// 線を描写します。
        /// </summary>
        /// <param name="bitmap">ﾋﾞｯﾄﾏｯﾌﾟｲﾒｰｼﾞ</param>
        /// <param name="begin">開始位置</param>
        /// <param name="end">終了位置</param>
        /// <param name="color">色</param>
        public static void DrawLine(this WriteableBitmap bitmap, Point begin, Point end, Color color)
        {
            bitmap.DrawLine((int)begin.X, (int)begin.Y, (int)end.X, (int)end.Y, color);
        }

        /// <summary>
        /// 四角形を描写します。
        /// </summary>
        /// <param name="bitmap">ﾋﾞｯﾄﾏｯﾌﾟｲﾒｰｼﾞ</param>
        /// <param name="margin">余白(左と上を使用)</param>
        /// <param name="begin">四角形の左上位置</param>
        /// <param name="end">四角形の右下位置</param>
        /// <param name="color">色</param>
        public static void DrawRectangle(this WriteableBitmap bitmap, Thickness margin, Point begin, Point end, Color color)
        {
            begin.X += margin.Left;
            begin.Y += margin.Top;
            end.X += margin.Left;
            end.Y += margin.Top;
            bitmap.DrawRectangle(begin, end, color);
            //bitmap.DrawRectangle(new Point(begin.X + margin.Left, begin.Y + margin.Top), new Point(end.X + margin.Left, end.Y + margin.Top), color);
        }

        /// <summary>
        /// 四角形を描写します。
        /// </summary>
        /// <param name="bitmap">ﾋﾞｯﾄﾏｯﾌﾟｲﾒｰｼﾞ</param>
        /// <param name="begin">四角形の左上位置</param>
        /// <param name="end">四角形の右下位置</param>
        /// <param name="color">色</param>
        public static void DrawRectangle(this WriteableBitmap bitmap, Point begin, Point end, Color color)
        {
            bitmap.DrawRectangle((int)begin.X, (int)begin.Y, (int)end.X, (int)end.Y, color);
        }

        /// <summary>
        /// ﾄﾞｯﾄ線を描写します。
        /// </summary>
        /// <param name="bitmap">ﾋﾞｯﾄﾏｯﾌﾟｲﾒｰｼﾞ</param>
        /// <param name="margin">余白(左と上を使用)</param>
        /// <param name="begin">開始位置</param>
        /// <param name="end">終了位置</param>
        /// <param name="color">色</param>
        public static void DrawLineDotted(this WriteableBitmap bitmap, Thickness margin, Point begin, Point end, Color color, int dotSpace, int dotLength)
        {
            begin.X += margin.Left;
            begin.Y += margin.Top;
            end.X += margin.Left;
            end.Y += margin.Top;
            bitmap.DrawLineDotted(begin, end, color, dotSpace, dotLength);
            //bitmap.DrawLineDotted(new Point(begin.X + margin.Left, begin.Y + margin.Top), new Point(end.X + margin.Left, end.Y + margin.Top), color, dotSpace, dotLength);
        }

        /// <summary>
        /// ﾄﾞｯﾄ線を描写します。
        /// </summary>
        /// <param name="bitmap">ﾋﾞｯﾄﾏｯﾌﾟｲﾒｰｼﾞ</param>
        /// <param name="begin">開始位置</param>
        /// <param name="end">終了位置</param>
        /// <param name="color">色</param>
        public static void DrawLineDotted(this WriteableBitmap bitmap, Point begin, Point end, Color color, int dotSpace, int dotLength)
        {
            bitmap.DrawLineDotted((int)begin.X, (int)begin.Y, (int)end.X, (int)end.Y, dotSpace, dotLength, color);
        }

        /// <summary>
        /// 連続した線を描写します。
        /// </summary>
        /// <param name="bitmap">ﾋﾞｯﾄﾏｯﾌﾟｲﾒｰｼﾞ</param>
        /// <param name="thickness">線の太さ</param>
        /// <param name="points">連続した線の位置</param>
        /// <param name="color">色</param>
        public static void DrawPolyline(this WriteableBitmap bitmap, int thickness, Point[] points, Color color)
        {
            bitmap.DrawPolyline(
                points
                    .SelectMany(p => new int[] { (int)p.X, (int)p.Y })
                    .ToArray(),
                color,
                thickness
            );
        }

        /// <summary>
        /// 連続した線を描写します。
        /// </summary>
        /// <param name="bitmap">ﾋﾞｯﾄﾏｯﾌﾟｲﾒｰｼﾞ</param>
        /// <param name="points">連続した線のX座標とY座標の配列(x1, y1, x2, y2...の順番で配列を認識する)</param>
        /// <param name="color">線の色</param>
        /// <param name="thickness">線の太さ</param>
        public static void DrawPolyline(this WriteableBitmap bitmap, int[] points, Color color, int thickness)
        {
            // 引数のpointsに設定されたYを起点としてthicknessの分だけY軸を1ﾋﾟｸｾﾙだけずらして太い線に見せる。
            //foreach (var i in Enumerable.Range(1, thickness))
            //{
            //    var adjustment = i - (int)Math.Ceiling(thickness / 2d);
            //    var newPoints = points
            //        .Select((p, index) => index % 2 == 0 ? p : p - adjustment)
            //        .ToArray();
            //    bitmap.DrawPolyline(newPoints, color);
            //}
            var array = Enumerable.Range(1, thickness).ToArray();
            foreach (var xy in array.SelectMany(x => array.Select(y => new { X = x, Y = y })))
            {
                var correction = (int)Math.Ceiling(thickness / 2d);
                var correctionX = xy.X - correction;
                var correctionY = xy.Y - correction;
                var newPoints = points
                    .Select((p, index) => index % 2 == 0 ? p - correctionY : p - correctionX)
                    .ToArray();
                bitmap.DrawPolyline(newPoints, color);
            }
        }
    }
}
