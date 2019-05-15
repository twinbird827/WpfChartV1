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
using WpfUtilV2.Extensions;
using WpfUtilV2.Mvvm;

namespace WpfChartV1.Mvvm.UserControls
{
    public class DateOrTimeChart : BindableBase
    {
        /// <summary>
        /// Y軸の分割数を取得、または設定します。
        /// </summary>
        public int YScaleSplit { get; set; }

        /// <summary>
        /// 高さの割合を取得、または設定します。
        /// </summary>
        public double PercentageHeight { get; set; } = 1;

        /// <summary>
        /// ﾁｬｰﾄの縦軸ﾀｲﾄﾙを設定、または取得します。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// ﾁｬｰﾄの最小値を設定、または取得します。
        /// </summary>
        public double Minimum { get; set; }

        /// <summary>
        /// ﾁｬｰﾄの最大値を設定、または取得します。
        /// </summary>
        public double Maximum { get; set; }

        /// <summary>
        /// ﾁｬｰﾄの表示ﾌｫｰﾏｯﾄを設定、または取得します。
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾃﾞｰﾀを取得、または設定します。
        /// </summary>
        public DateOrTimeSeries[] Series { get; set; }

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
        /// 最後に描写した画像
        /// </summary>
        internal ImageSource PreviousRender { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ領域の幅を取得、または設定します。
        /// </summary>
        internal double Width { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ領域の高さを取得、または設定します。
        /// </summary>
        internal double Height { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ領域の幅を取得、または設定します。
        /// </summary>
        internal double GWidth { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ領域の高さを取得、または設定します。
        /// </summary>
        internal double GHeight { get; set; }

        /// <summary>
        /// 折れ線ｸﾞﾗﾌ以外の色を取得、または設定します。
        /// </summary>
        private Color Color { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ以外の領域を取得、または設定します。
        /// </summary>
        private Thickness Margin;

        /// <summary>
        /// ﾀﾞﾌﾞﾙｸﾘｯｸ時の処理(ｸﾞﾗﾌの表示内容を変更するﾀﾞｲｱﾛｸﾞを表示する)
        /// </summary>
        public ICommand OnDoubleClick
        {
            get { return _OnDoubleClick = _OnDoubleClick ?? new RelayCommand<MouseButtonEventArgs>(OnDoubleClickAction); }
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
        /// ｸﾞﾗﾌを描写します。
        /// </summary>
        /// <param name="container">ｺﾝﾃﾅ</param>
        /// <param name="xheaders">X軸ﾍｯﾀﾞ文字</param>
        internal void Draw(DateOrTimeSingleChart container, FormattedText[] xheaders)
        {
            var series = Series?.FirstOrDefault();

            if (Width <= 0 || Height <= 0 || series == null) return;

            Color = GetForeground(container);

            // Y軸に表示するﾍｯﾀﾞ文字の生成
            var yheaders = series != null
                ? Util.GetScaleStrings(Minimum, Maximum, YScaleSplit, Format)
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
                // X軸とY軸の目盛り線を描写
                DrawXYScale(container, bitmap);

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
                content.DrawImage(bitmap.Frozen());

                // Y軸の文字を描写
                DrawYText(container, content, yheaders);
            }

            // WriteableBitmapは必要ないので開放
            bitmap = null;

            // ｱﾝﾁｴｲﾘｱｽ解除
            var image = new DrawingImage(Util.ReleaseAntialiasing(dv));
            // Freezeして設定
            Render = image.Frozen();
            PreviousRender = Render;

        }

        /// <summary>
        /// ｸﾞﾗﾌの枠色を取得します。
        /// </summary>
        /// <param name="control">ｺﾝﾃﾅ</param>
        /// <returns></returns>
        private Color GetForeground(Control control)
        {
            var brush = control.Foreground.Frozen() as SolidColorBrush;
            return brush?.Color ?? Colors.Black;
        }

        /// <summary>
        /// X軸とY軸の目盛り線を描写します。
        /// </summary>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        private void DrawXYScale(DateOrTimeSingleChart container, WriteableBitmap dc)
        {
            foreach (var i in Enumerable.Range(0, container.XScaleSplit + 1))
            {
                var x = GWidth / container.XScaleSplit * i;

                dc.DrawLineDotted(
                    Margin,
                    new Point(x, 1),
                    new Point(x, GHeight),
                    Color,
                    Util.DotSpace,
                    Util.DotLength
                );

                dc.DrawLine(
                    Margin,
                    new Point(x, GHeight + Util.ScaleLineLength),
                    new Point(x, GHeight),
                    Color
                );
            }

            foreach (var i in Enumerable.Range(0, YScaleSplit + 1))
            {
                var y = GHeight / YScaleSplit * i;

                dc.DrawLineDotted(
                    Margin,
                    new Point(0, y),
                    new Point(GWidth, y),
                    Color,
                    Util.DotSpace,
                    Util.DotLength
                );

                dc.DrawLine(
                    Margin,
                    new Point(Util.ScaleLineLength * -1, y),
                    new Point(0, y),
                    Color
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
                var zY = GHeight / (Maximum - Minimum);
                var zX = xzoom;

                // 折れ線ｸﾞﾗﾌのﾎﾟｲﾝﾄ配列を作成
                var points = item.Lines
                    .SelectMany(line =>
                    {
                        // 枠外の値は、Max or Min にあわせる
                        var yNow = GetInnerY(line.Y, Maximum, Minimum);
                        var x = (line.X - item.XBeginDate).Ticks * zX;
                        var y = (Maximum - Minimum - (yNow - Minimum)) * zY;
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
        private void DrawYText(DateOrTimeSingleChart container, DrawingContext content, FormattedText[] headers)
        {
            var title = Util.GetFormattedText(Title, container);

            foreach (var i in Enumerable.Range(0, YScaleSplit + 1))
            {
                var text = headers[i];
                var x = Util.ScaleLineLength * -1 + Margin.Left;
                var y = GHeight - GHeight / YScaleSplit * i + Margin.Top;

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
        internal bool DrawMouseLine(DateOrTimeSingleChart container, Point p)
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
                    new Point(x, Margin.Top),
                    new Point(x, GHeight + Margin.Top),
                    Util.RightClickColor,
                    Util.DotSpace,
                    Util.DotLength
                );

                bitmap.DrawLineDotted(
                    new Point(Margin.Left, y),
                    new Point(GWidth + Margin.Left, y),
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
            var image = new DrawingImage(Util.ReleaseAntialiasing(dv));

            // Freezeして設定
            Render = image.Frozen();

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
        private FormattedText GetYText(DateOrTimeSingleChart c, double y)
        {
            var value = (Maximum - Minimum) / GHeight * (GHeight - y + Margin.Top) + Minimum;
            return Util.GetFormattedText(string.Format(Format, value), Util.RightClickBrush, c);
        }

    }
}
