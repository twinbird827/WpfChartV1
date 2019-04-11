using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfChartV1.Common;
using WpfUtilV2.Mvvm;

namespace WpfChartV1.Mvvm.UserControls
{
    public class LineChart : BindableBase
    {
        public LineChart()
        {
            ScaleType = ScaleType.TimeSpan;
            DateTimeFormat1 = @"{0:MM/dd}";
            DateTimeFormat2 = @"{0:HH:mm:ss}";
            DateTimeFormat3 = @"{0:MM/dd HH:mm:ss}";
            TimeSpanFormat1 = @"{0:dd}日";
            TimeSpanFormat2 = @"{0:hh\:mm\:ss}";
            TimeSpanFormat3 = @"{0:dd'日 'hh':'mm':'ss}";
        }

        /// <summary>
        /// X軸の分割数を取得、または設定します。
        /// </summary>
        public int ScaleSplitCountX { get; set; }

        /// <summary>
        /// Y軸の分割数を取得、または設定します。
        /// </summary>
        public int ScaleSplitCountY { get; set; }

        /// <summary>
        /// X軸の表示ﾀｲﾌﾟを取得、または設定します。
        /// </summary>
        public ScaleType ScaleType { get; set; }

        /// <summary>
        /// X軸の書式1(ScaleType=TimeSpan)を取得、または設定します。
        /// </summary>
        public string TimeSpanFormat1 { get; set; }

        /// <summary>
        /// X軸の書式2(ScaleType=TimeSpan)を取得、または設定します。
        /// </summary>
        public string TimeSpanFormat2 { get; set; }

        /// <summary>
        /// X軸の書式3(ScaleType=TimeSpan)を取得、または設定します。
        /// </summary>
        public string TimeSpanFormat3 { get; set; }

        /// <summary>
        /// X軸の書式1(ScaleType=DateTime)を取得、または設定します。
        /// </summary>
        public string DateTimeFormat1 { get; set; }

        /// <summary>
        /// X軸の書式2(ScaleType=DateTime)を取得、または設定します。
        /// </summary>
        public string DateTimeFormat2 { get; set; }

        /// <summary>
        /// X軸の書式3(ScaleType=DateTime)を取得、または設定します。
        /// </summary>
        public string DateTimeFormat3 { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾃﾞｰﾀを取得、または設定します。
        /// </summary>
        public LineSeries[] Series { get; set; }

        /// <summary>
        /// 描写する画像を取得、または設定します。
        /// </summary>
        public ImageSource Render
        {
            get { return _Render; }
            set { SetProperty(ref _Render, value); }
        }
        private ImageSource _Render;

        /// <summary>
        /// 画像の幅を取得、または設定します。
        /// </summary>
        internal double Width { get; set; }

        /// <summary>
        /// 画像の高さを取得、または設定します。
        /// </summary>
        internal double Height { get; set; }

        /// <summary>
        /// 最後に描写した画像
        /// </summary>
        internal ImageSource PreviousRender { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ以外の領域を取得、または設定します。
        /// </summary>
        private Thickness Margin;

        /// <summary>
        /// 折れ線ｸﾞﾗﾌ以外の色を取得、または設定します。
        /// </summary>
        private Color Color { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ領域の幅を取得、または設定します。
        /// </summary>
        private double GWidth { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ領域の高さを取得、または設定します。
        /// </summary>
        private double GHeight { get; set; }

        /// <summary>
        /// ﾀﾞﾌﾞﾙｸﾘｯｸ時の処理(ｸﾞﾗﾌの表示内容を変更するﾀﾞｲｱﾛｸﾞを表示する)
        /// </summary>
        public ICommand OnDoubleClick
        {
            get
            {
                return _OnDoubleClick = _OnDoubleClick ?? new RelayCommand<MouseButtonEventArgs>(OnDoubleClickAction);
            }
        }
        private ICommand _OnDoubleClick;

        /// <summary>
        /// ﾁｬｰﾄ内でﾀﾞﾌﾞﾙｸﾘｯｸした際の処理を実行します。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDoubleClickAction(MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// X軸のﾍｯﾀﾞ文字を取得します。
        /// </summary>
        /// <param name="c">ｺﾝﾃﾅ</param>
        /// <returns></returns>
        internal FormattedText[] GetXHeaders(SingleAxisChart c)
        {
            var xheaders = ScaleType == ScaleType.DateTime
                ? Util.GetScaleStrings(c.XStartDate + c.XStartTime, c.XRange, ScaleSplitCountX, DateTimeFormat1, DateTimeFormat2)
                : Util.GetScaleStrings(c.XStartTime, c.XRange, ScaleSplitCountX, TimeSpanFormat1, TimeSpanFormat2);

            return xheaders
                .Select(s => Util.GetFormattedText(s, c))
                .ToArray();
        }

        /// <summary>
        /// X軸のﾍｯﾀﾞ文字を取得します。
        /// </summary>
        /// <param name="c">ｺﾝﾃﾅ</param>
        /// <returns></returns>
        internal FormattedText GetXHeaderDate(SingleAxisChart c, double x)
        {
            // x地点を表す日付
            var xdate = c.XStartDate + c.XStartTime + TimeSpan.FromTicks((long)(c.XRange.Ticks / GWidth * ((x - Margin.Left))));
            // x地点を表す時刻
            var xtime = ScaleType == ScaleType.TimeSpan ? xdate - c.XStartDate + c.XStartTime : TimeSpan.Zero;

            var xheader = ScaleType == ScaleType.DateTime
                ? $"{string.Format(DateTimeFormat3, xdate)}"
                : $"{string.Format(TimeSpanFormat3, xtime)}";

            return Util.GetFormattedText(xheader, Util.RightClickBrush, c);
        }

        /// <summary>
        /// ｸﾞﾗﾌを描写します。
        /// </summary>
        /// <param name="container">ｺﾝﾃﾅ</param>
        /// <param name="xheaders">X軸のﾍｯﾀﾞ文字</param>
        internal void Draw(SingleAxisChart container, FormattedText[] xheaders)
        {
            var series = Series?.FirstOrDefault();

            if (Width <= 0 || Height <= 0 || series == null) return;

            var foreground = container.Foreground.GetAsFrozen() as SolidColorBrush;
            Color = foreground != null ? foreground.Color : Colors.Black;

            // Y軸に表示するﾍｯﾀﾞ文字の生成
            var yheaders = series != null
                ? Util.GetScaleStrings(series.Min, series.Max, ScaleSplitCountY, series.Format)
                    .Select(s => Util.GetFormattedText(s, container))
                    .ToArray()
                : new FormattedText[] { Util.GetFormattedText("1", container) };

            // ｸﾞﾗﾌ以外の領域
            Margin.Top = yheaders[0].Height / 2;        // 上=目盛り文字の半分
            Margin.Left = Util.MarginLeft;              // 左=固定値
            Margin.Right = xheaders.Last().Width / 2;   // 右=最後のX軸目盛り文字の半分
            Margin.Bottom = Util.ScaleLineLength;       // 下=X軸目盛り高さの最大値＋目盛り線
            // ｸﾞﾗﾌ描写領域
            GWidth = Width - Margin.Left - Margin.Right;
            GHeight = Height - Margin.Top - Margin.Bottom;
            //ｸﾞﾗﾌ内のX軸倍率
            var xzoom = GWidth / container.XRange.Ticks;

            // ﾋﾞｯﾄﾏｯﾌﾟの大きさ決定
            var bitmap = Util.CreateWriteableBitmap((int)Width, (int)Height);

            // ﾋﾞｯﾄﾏｯﾌﾟに線を描写
            using (var context = bitmap.GetBitmapContext())
            {
                // Y軸の表題を描写
                DrawXYAxisLine(bitmap);

                // 目盛りを描写
                DrawXYScale(bitmap);

                // ﾌﾚｰﾑ描写
                DrawFrame(bitmap);

                // 折れ線ｸﾞﾗﾌ描写
                DrawPolyline(bitmap, xzoom);
            }

            // X軸とY軸の文字を描写
            var dv = new DrawingGroup();
            using (var content = dv.Open())
            {
                // WriteableBitmap を貼付
                content.DrawImage(bitmap.GetAsFrozen() as ImageSource);

                // Y軸の文字を描写
                DrawYText(content, yheaders, Util.GetFormattedText(series?.Title, container));
            }

            // WriteableBitmapは必要ないので開放
            bitmap = null;

            // ｱﾝﾁｴｲﾘｱｽ解除
            var returnImage = new DrawingImage(Util.ReleaseAntialiasing(dv));
            // Freezeして設定
            Render = returnImage.GetAsFrozen() as ImageSource;
            PreviousRender = Render;
        }

        /// <summary>
        /// X軸とY軸の区切り線を描写します。
        /// </summary>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        private void DrawXYAxisLine(WriteableBitmap dc)
        {
            foreach (var i in Enumerable.Range(0, ScaleSplitCountX + 1))
            {
                var x = GWidth / ScaleSplitCountX * i;
                dc.DrawLine(
                    Margin,
                    new Point(x, GHeight + Util.ScaleLineLength),
                    new Point(x, GHeight),
                    Color
                );
            }
            foreach (var i in Enumerable.Range(0, ScaleSplitCountY + 1))
            {
                var y = GHeight / ScaleSplitCountY * i;
                dc.DrawLine(
                    Margin,
                    new Point(Util.ScaleLineLength * -1, y),
                    new Point(0, y),
                    Color
                );
            }
        }

        /// <summary>
        /// X軸とY軸の目盛りを描写します。
        /// </summary>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        private void DrawXYScale(WriteableBitmap dc)
        {
            foreach (var i in Enumerable.Range(0, ScaleSplitCountX))
            {
                var x = GWidth / ScaleSplitCountX * i + 1;
                dc.DrawLineDotted(
                    Margin,
                    new Point(x, 1),
                    new Point(x, GHeight),
                    Color,
                    Util.DotSpace,
                    Util.DotLength
                );
            }
            foreach (var i in Enumerable.Range(0, ScaleSplitCountY))
            {
                var y = GHeight / ScaleSplitCountY * i + 1;
                dc.DrawLineDotted(
                    Margin,
                    new Point(0, y),
                    new Point(GWidth, y),
                    Color,
                    Util.DotSpace,
                    Util.DotLength
                );
            }
        }

        /// <summary>
        /// 折れ線ｸﾞﾗﾌを描写します。
        /// </summary>
        /// <param name="bitmap"></param>
        private void DrawPolyline(WriteableBitmap bitmap, double xzoom)
        {
            foreach (var item in Series)
            {
                // ﾗｲﾝが設定されていないものは除外
                if (item.Lines == null || !item.Lines.Any()) continue;

                // X, Y座標の倍率
                var zY = GHeight / (item.Max - item.Min);
                var zX = xzoom;

                // 折れ線ｸﾞﾗﾌのﾎﾟｲﾝﾄ配列を作成
                var points = item.Lines
                    .SelectMany(line =>
                    {
                        // 枠外の値は、Max or Min にあわせる
                        var yNow = GetInnerY(line.Y, item.Max, item.Min); //line.Y > item.Max ? item.Max : line.Y < item.Min ? item.Min : line.Y;
                        var x = (line.X - item.XStartDate).Ticks * zX;
                        var y = ((item.Max - item.Min) - (yNow - item.Min)) * zY;
                        return new int[] { (int)(x + Margin.Left), (int)(y + Margin.Top) };
                    })
                    .ToArray();

                bitmap.DrawPolyline(points, item.Foreground, (int)item.Thickness);
            }
        }

        /// <summary>
        /// 枠内に収まるようにYを補正します。
        /// </summary>
        /// <param name="y">補正前のy</param>
        /// <param name="max">yに設定できる最大値</param>
        /// <param name="min">yに設定できる最小値</param>
        /// <returns></returns>
        private double GetInnerY(double y, double max, double min)
        {
            var tmpMax = min < max ? max : min;
            var tmpMin = min < max ? min : max;
            return tmpMax < y ? tmpMax : y < tmpMin ? tmpMin : y;
        }

        /// <summary>
        /// 枠を描写します。
        /// </summary>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        private void DrawFrame(WriteableBitmap dc)
        {
            dc.DrawRectangle(Margin, new Point(0, 0), new Point(GWidth, GHeight), Color);
        }

        /// <summary>
        /// Y軸のﾍｯﾀﾞ文字を描写します。
        /// </summary>
        /// <param name="content"></param>
        private void DrawYText(DrawingContext content, FormattedText[] headers, FormattedText title)
        {
            var item = Series?.FirstOrDefault();

            if (item == null) return;

            foreach (var i in Enumerable.Range(0, ScaleSplitCountY + 1))
            {
                var text = headers[i];
                var x = Util.ScaleLineLength * -1 + Margin.Left;
                var y = GHeight - GHeight / ScaleSplitCountY * i + Margin.Top;

                // 文字を描写
                content.DrawText(text, new Point(x - text.Width, y - text.Height / 2));
            }

            // Y軸表題と描写位置設定
            var point = new Point(0, GHeight / 2 + Margin.Top + title.Width / 2);

            // Y軸標題の中心で反時計回りに90度回転させて描写
            content.PushTransform(new RotateTransform(-90, point.X, point.Y));
            content.DrawText(title, point);
            content.Pop();
        }

        /// <summary>
        /// ﾏｳｽ右ｸﾘｯｸﾎｰﾙﾄﾞ時に対象位置を十字に区切る点線と、Y軸ﾍｯﾀﾞにその地点の値を表示します。
        /// </summary>
        /// <param name="container">ｺﾝﾃﾅ</param>
        /// <param name="p">表示位置</param>
        /// <returns>描写した：true / していない：false</returns>
        internal bool DrawMouseLine(SingleAxisChart container, Point p)
        {
            if (!(Margin.Left < p.X &&
                    p.X < GWidth + Margin.Left &&
                    Margin.Top < p.Y &&
                    p.Y < GHeight + Margin.Top))
            {
                return false;
            }

            // ﾋﾞｯﾄﾏｯﾌﾟの大きさ決定
            var bitmap = Util.CreateWriteableBitmap((int)Width, (int)Height);
            var x = p.X;
            var y = p.Y;

            var formattedtext = GetYText(container, y);
            var formattedpoint = new Point(Margin.Left - Util.ScaleLineLength - formattedtext.Width, y - formattedtext.Height / 2);

            // ﾋﾞｯﾄﾏｯﾌﾟに線を描写
            using (var context = bitmap.GetBitmapContext())
            {
                bitmap.DrawLineDotted(
                    new Point(x, Margin.Top + 1),
                    new Point(x, GHeight + Margin.Top + 1),
                    Util.RightClickColor,
                    Util.DotSpace,
                    Util.DotLength
                );

                bitmap.DrawLineDotted(
                    new Point(Margin.Left + 1, y),
                    new Point(GWidth + Margin.Left + 1, y),
                    Util.RightClickColor,
                    Util.DotSpace,
                    Util.DotLength
                );

                var x1 = formattedpoint.X;
                var y1 = formattedpoint.Y;
                var x2 = x1 + formattedtext.Width;
                var y2 = y1 + formattedtext.Height;
                bitmap.FillRectangle((int)x1, (int)y1, (int)x2, (int)y2, Util.RightClickBackground);
            }

            // Y軸の文字を描写
            var dv = new DrawingGroup();
            using (var content = dv.Open())
            {
                // 元々ﾁｬｰﾄに貼り付けていたﾋﾞｯﾄﾏｯﾌﾟを貼付
                content.DrawImage(PreviousRender);

                // 作成したﾋﾞｯﾄﾏｯﾌﾟを貼付
                content.DrawImage(bitmap);

                // 縦軸の値を描写
                content.DrawText(formattedtext, formattedpoint);
            }

            // ｱﾝﾁｴｲﾘｱｽ解除
            var returnImage = new DrawingImage(Util.ReleaseAntialiasing(dv));
            
            // Freezeして設定
            Render = returnImage.GetAsFrozen() as ImageSource;

            // WriteableBitmapは必要ないので開放
            bitmap = null;

            return true;
        }

        /// <summary>
        /// 指定した位置を意味する表示文字を取得します。
        /// </summary>
        /// <param name="c">ｺﾝﾃﾅ</param>
        /// <param name="y">位置</param>
        /// <returns><code>FormattedText</code></returns>
        private FormattedText GetYText(SingleAxisChart c, double y)
        {
            var series = Series.First();
            var value = (series.Max - series.Min) / GHeight * (GHeight - y + Margin.Top) + series.Min;
            return Util.GetFormattedText(string.Format(series.Format, value), Util.RightClickBrush, c);
        }
    }
}
