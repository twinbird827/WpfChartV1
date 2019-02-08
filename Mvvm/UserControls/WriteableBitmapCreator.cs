using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfChartV1.Common;
using WpfUtilV2.Common;

namespace WpfChartV1.Mvvm.UserControls
{
    public class WriteableBitmapCreator : IDisposable
    {
        /// <summary>
        /// ｸﾞﾗﾌ以外の領域
        /// </summary>
        private Thickness Margin;

        /// <summary>
        /// X軸に表示する文字列の列挙体
        /// </summary>
        private FormattedText[] HeaderXStrings { get; set; }

        /// <summary>
        /// Y軸に表示する文字列の列挙体
        /// </summary>
        private FormattedText[] HeaderYStrings { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌのX軸拡大縮小の比率
        /// </summary>
        private double ZoomRatioX { get; set; }

        /// <summary>
        /// ｷｬﾝﾊﾞｽの幅
        /// </summary>
        private double CanvasWidth { get; set; }

        /// <summary>
        /// ｷｬﾝﾊﾞｽの高さ
        /// </summary>
        private double CanvasHeight { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ描写領域の幅
        /// </summary>
        private double GraphWidth { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ描写領域の高さ
        /// </summary>
        private double GraphHeight { get; set; }

        /// <summary>
        /// 文字色
        /// </summary>
        private Color ForeColor { get; set; }

        /// <summary>
        /// 文字色
        /// </summary>
        private SolidColorBrush ForeBrush { get; set; }

        /// <summary>
        /// X軸の分割数を取得、または設定します。
        /// </summary>
        private int ScaleSplitCountX { get; set; }

        /// <summary>
        /// Y軸の分割数を取得、または設定します。
        /// </summary>
        private int ScaleSplitCountY { get; set; }

        /// <summary>
        /// 目盛り線の長さを取得、または設定します。
        /// </summary>
        private double ScaleLineLength { get; set; }

        /// <summary>
        /// X軸の右端の基準日を取得、または設定します。
        /// </summary>
        private DateTime EndTimeX { get; set; }

        /// <summary>
        /// X軸の左端の基準日を取得、または設定します。
        /// </summary>
        private DateTime BeginTimeX { get; set; }

        /// <summary>
        /// X軸の表示ﾀｲﾌﾟを取得、または設定します。
        /// </summary>
        private ScaleType ScaleType { get; set; }

        /// <summary>
        /// X軸の書式1(ScaleType=TimeSpan)を取得、または設定します。
        /// </summary>
        private string TimeSpanFormat1 { get; set; }

        /// <summary>
        /// X軸の書式2(ScaleType=TimeSpan)を取得、または設定します。
        /// </summary>
        private string TimeSpanFormat2 { get; set; }

        /// <summary>
        /// X軸の書式1(ScaleType=DateTime)を取得、または設定します。
        /// </summary>
        private string DateTimeFormat1 { get; set; }

        /// <summary>
        /// X軸の書式2(ScaleType=DateTime)を取得、または設定します。
        /// </summary>
        private string DateTimeFormat2 { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾃﾞｰﾀを取得、または設定します。
        /// </summary>
        private IEnumerable<Series> Items { get; set; }

        /// <summary>
        /// ﾌｫﾝﾄﾌｧﾐﾘｰを取得、または設定します。
        /// </summary>
        private FontFamily FontFamily { get; set; }

        /// <summary>
        /// ﾌｫﾝﾄｽﾀｲﾙを取得、または設定します。
        /// </summary>
        private FontStyle FontStyle { get; set; }

        /// <summary>
        /// ﾌｫﾝﾄの太さを取得、または設定します。
        /// </summary>
        private FontWeight FontWeight { get; set; }

        /// <summary>
        /// ﾌｫﾝﾄを縮小、または拡大する度合いを取得、または設定します。
        /// </summary>
        private FontStretch FontStretch { get; set; }

        /// <summary>
        /// ﾌｫﾝﾄｻｲｽﾞを取得、または設定します。
        /// </summary>
        private double FontSize { get; set; }

        /// <summary>
        /// X軸のﾍｯﾀﾞ文字を表示するかどうかを取得、または設定します。
        /// </summary>
        private bool IsVisibleXHeader { get; set; }

        private int DotSpace { get; set; } = 2;

        private int DotLength { get; set; } = 2;

        public static WriteableBitmapCreator CreateInstance(Chart c)
        {
            var color = ((SolidColorBrush)c.Foreground).Color;
            var brush = new SolidColorBrush(color);
            brush.Freeze();

            return new WriteableBitmapCreator()
            {
                BeginTimeX = c.BeginTimeX,
                CanvasHeight = c.ActualHeight,
                CanvasWidth = c.ActualWidth,
                DateTimeFormat1 = c.DateTimeFormat1,
                DateTimeFormat2 = c.DateTimeFormat2,
                EndTimeX = c.EndTimeX,
                ForeColor = color,
                ForeBrush = brush,
                Items = c.Items,
                ScaleLineLength = c.ScaleLineLength,
                ScaleSplitCountX = c.ScaleSplitCountX,
                ScaleSplitCountY = c.ScaleSplitCountY,
                ScaleType = c.ScaleType,
                TimeSpanFormat1 = c.TimeSpanFormat1,
                TimeSpanFormat2 = c.TimeSpanFormat2,
                FontFamily = c.FontFamily,
                FontSize = c.FontSize,
                FontStretch = c.FontStretch,
                FontStyle = c.FontStyle,
                FontWeight = c.FontWeight,
                IsVisibleXHeader = c.IsVisibleXHeader
            };
        }

        public static ImageSource DrawMouseLine(Chart c, MouseButtonEventArgs e)
        {
            using (var creater = WriteableBitmapCreator.CreateInstance(c))
            {
                creater.Initialize();

                // ﾋﾞｯﾄﾏｯﾌﾟの大きさ決定
                var bitmap = Util.CreateWriteableBitmap((int)creater.CanvasWidth, (int)creater.CanvasHeight);

                // ﾋﾞｯﾄﾏｯﾌﾟに線を描写
                using (var context = bitmap.GetBitmapContext())
                {
                    var x = e.GetPosition(e.Source as IInputElement).X;
                    bitmap.DrawLineDotted(
                        new Point(x, creater.Margin.Top + 1),
                        new Point(x, creater.GraphHeight + creater.Margin.Top + 1),
                        creater.ForeColor,
                        4, 4
                    );
                }

                // X軸とY軸の文字を描写
                var dv = new DrawingGroup();
                using (var content = dv.Open())
                {
                    // 作成したﾋﾞｯﾄﾏｯﾌﾟを貼付
                    content.DrawImage(bitmap);

                    // 元々ﾁｬｰﾄに貼り付けていたﾋﾞｯﾄﾏｯﾌﾟを貼付
                    content.DrawImage(c.LastRenderImage);
                }

                // ｱﾝﾁｴｲﾘｱｽ解除
                var returnImage = new DrawingImage(Util.ReleaseAntialiasing(dv));

                // 変更不可にする
                returnImage.Freeze();

                // WriteableBitmapは必要ないので開放
                bitmap = null;

                // 返却
                return returnImage;

            }
        }

        public ImageSource CreateImage(ImageSource original, MouseButtonEventArgs e)
        {
            Initialize();

            // ﾋﾞｯﾄﾏｯﾌﾟの大きさ決定
            var bitmap = Util.CreateWriteableBitmap((int)CanvasWidth, (int)CanvasHeight);

            // ﾋﾞｯﾄﾏｯﾌﾟに線を描写
            using (var context = bitmap.GetBitmapContext())
            {
                var position = e.GetPosition(e.Source as IInputElement);
                var x = position.X;
                var y = position.Y;

                bitmap.DrawLineDotted(
                    new Point(x, Margin.Top + 1),
                    new Point(x, GraphHeight + Margin.Top + 1),
                    Colors.Red,
                    DotSpace,
                    DotLength
                );

                bitmap.DrawLineDotted(
                    new Point(Margin.Left + 1, y),
                    new Point(GraphWidth + Margin.Left + 1, y),
                    Colors.Red,
                    DotSpace,
                    DotLength
                );
            }

            // X軸とY軸の文字を描写
            var dv = new DrawingGroup();
            using (var content = dv.Open())
            {
                // 元々ﾁｬｰﾄに貼り付けていたﾋﾞｯﾄﾏｯﾌﾟを貼付
                content.DrawImage(original);

                // 作成したﾋﾞｯﾄﾏｯﾌﾟを貼付
                content.DrawImage(bitmap);

            }

            // ｱﾝﾁｴｲﾘｱｽ解除
            var returnImage = new DrawingImage(Util.ReleaseAntialiasing(dv));

            // 変更不可にする
            returnImage.Freeze();

            // WriteableBitmapは必要ないので開放
            bitmap = null;

            // 返却
            return returnImage;
        }

        public ImageSource CreateImage()
        {
            try
            {
                // 内部変数の初期化
                Initialize();

                // ﾋﾞｯﾄﾏｯﾌﾟの大きさ決定
                var bitmap = Util.CreateWriteableBitmap((int)CanvasWidth, (int)CanvasHeight);

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
                    DrawPolyline(bitmap);
                }

                if (bitmap.CanFreeze) bitmap.Freeze();

                // X軸とY軸の文字を描写
                var dv = new DrawingGroup();
                using (var content = dv.Open())
                {
                    // WriteableBitmap を貼付
                    //content.DrawImage(bitmap, new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    content.DrawImage(bitmap);

                    // X軸の文字を描写
                    DrawXText(content);

                    // Y軸の文字を描写
                    DrawYText(content);
                }

                // ｱﾝﾁｴｲﾘｱｽ解除
                var returnImage =  new DrawingImage(Util.ReleaseAntialiasing(dv));

                // 変更不可にする
                returnImage.Freeze();

                // WriteableBitmapは必要ないので開放
                bitmap = null;

                // 返却
                return returnImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chart Create Exception: {DateTime.Now.ToString("ss.fff")}");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// 内部変数を初期化します。
        /// </summary>
        private void Initialize()
        {
            var item = Items?.FirstOrDefault();

            // Y軸に表示するﾍｯﾀﾞ文字の生成
            HeaderYStrings = item != null
                ? Util.GetScaleStrings(item.Min, item.Max, ScaleSplitCountY, item.Format)
                    .Select(s => GetFormattedText(s))
                    .ToArray()
                : new FormattedText[] { GetFormattedText("1") };

            // X軸に表示するﾍｯﾀﾞ日付文字の生成
            HeaderXStrings = (
                ScaleType == ScaleType.DateTime
                    ? Util.GetScaleStrings(BeginTimeX, EndTimeX, ScaleSplitCountX, DateTimeFormat1, DateTimeFormat2)
                    : Util.GetScaleStrings(TimeSpan.FromSeconds(0), EndTimeX - BeginTimeX, ScaleSplitCountX, TimeSpanFormat1, TimeSpanFormat2)
                )
                .Select(s => GetFormattedText(s))
                .ToArray();

            // ｸﾞﾗﾌ以外の領域
            Margin.Top = HeaderYStrings[0].Height / 2;                                                      // 上=目盛り文字の半分
            //OtherThanCanvas.Left = Items.FirstOrDefault().GetHeaderWidth(this) - ScaleLineLength;         // 左=最初のY軸目盛り幅                 TODO 多軸表示対応 TODO Itemsが無い場合の考慮
            Margin.Left = 80;                                                                               // 左=固定値                            TODO 多軸表示対応 TODO 全Chartの最大文字にあわせる
            Margin.Right = HeaderXStrings.Last().Width / 2;                                                 // 右=最後のX軸目盛り文字の半分         TODO 多軸表示対応
            Margin.Bottom = (IsVisibleXHeader ? HeaderXStrings.Max(s => s.Height) : 0) + ScaleLineLength;  // 下=X軸目盛り高さの最大値＋目盛り線

            // ｸﾞﾗﾌ領域の大きさ
            GraphHeight = CanvasHeight - Margin.Top - Margin.Bottom;
            GraphWidth = CanvasWidth - Margin.Left - Margin.Right;

            //ｸﾞﾗﾌ内のX軸倍率
            ZoomRatioX = GraphWidth / (EndTimeX - BeginTimeX).Ticks;
        }

        /// <summary>
        /// X軸とY軸の区切り線を描写します。
        /// </summary>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        private void DrawXYAxisLine(WriteableBitmap dc)
        {
            foreach (var i in Enumerable.Range(0, ScaleSplitCountX + 1))
            {
                var x = GraphWidth / ScaleSplitCountX * i;
                dc.DrawLine(
                    Margin,
                    new Point(x, GraphHeight + ScaleLineLength),
                    new Point(x, GraphHeight),
                    ForeColor
                );
            }
            foreach (var i in Enumerable.Range(0, ScaleSplitCountY + 1))
            {
                var y = GraphHeight / ScaleSplitCountY * i;
                dc.DrawLine(
                    Margin,
                    new Point(ScaleLineLength * -1, y),
                    new Point(0, y),
                    ForeColor
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
                var x = GraphWidth / ScaleSplitCountX * i + 1;
                dc.DrawLineDotted(
                    Margin,
                    new Point(x, 1),
                    new Point(x, GraphHeight),
                    ForeColor,
                    DotSpace,
                    DotLength
                );
            }
            foreach (var i in Enumerable.Range(0, ScaleSplitCountY))
            {
                var y = GraphHeight / ScaleSplitCountY * i + 1;
                dc.DrawLineDotted(
                    Margin,
                    new Point(0, y),
                    new Point(GraphWidth, y),
                    ForeColor,
                    DotSpace,
                    DotLength
                );
            }
        }

        /// <summary>
        /// 折れ線ｸﾞﾗﾌを描写します。
        /// </summary>
        /// <param name="bitmap"></param>
        private void DrawPolyline(WriteableBitmap bitmap)
        {
            foreach (var item in Items?.OfType<LineSeries>())
            {
                // ﾗｲﾝが設定されていないものは除外
                if (item.Lines == null || !item.Lines.Any()) return;

                // X, Y座標の倍率
                var zY = GraphHeight / (item.Max - item.Min);
                var zX = ZoomRatioX;

                // 折れ線ｸﾞﾗﾌのﾎﾟｲﾝﾄ配列を作成
                var points = item.Lines
                    .SelectMany(line => 
                    {
                        // 枠外の値は、Max or Min にあわせる
                        var yNow = line.Y > item.Max ? item.Max : line.Y < item.Min ? item.Min : line.Y;
                        var x = (line.X - BeginTimeX).Ticks * zX;
                        var y = (item.Max - (yNow - item.Min)) * zY;
                        return new int[] { (int)(x + Margin.Left), (int)(y + Margin.Top) };
                    })
                    .ToArray();

                bitmap.DrawPolyline(points, item.Foreground, (int)item.Thickness);
            }
        }

        /// <summary>
        /// 枠を描写します。
        /// </summary>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        private void DrawFrame(WriteableBitmap dc)
        {
            dc.DrawRectangle(Margin, new Point(0, 0), new Point(GraphWidth, GraphHeight), ForeColor);
        }

        /// <summary>
        /// 描写用にﾌｫｰﾏｯﾄされたｵﾌﾞｼﾞｪｸﾄを取得します。
        /// </summary>
        /// <param name="text">ｵﾌﾞｼﾞｪｸﾄに描写するﾒｯｾｰｼﾞ</param>
        /// <returns></returns>
        private FormattedText GetFormattedText(string text)
        {
            return Util.GetFormattedText(text, ForeBrush, FontFamily, FontStyle, FontWeight, FontStretch, FontSize);
        }

        /// <summary>
        /// X軸のﾍｯﾀﾞ文字を描写します。
        /// </summary>
        /// <param name="content"></param>
        private void DrawXText(DrawingContext content)
        {
            if (!IsVisibleXHeader) return;
            
            foreach (var i in Enumerable.Range(0, ScaleSplitCountX + 1))
            {
                var text = HeaderXStrings[i];
                var x = GraphWidth / ScaleSplitCountX * i + Margin.Left;
                var y = GraphHeight + ScaleLineLength + Margin.Top;

                // 文字を描写
                content.DrawText(text, new Point(x - text.Width / 2, y));
            }
        }

        /// <summary>
        /// Y軸のﾍｯﾀﾞ文字を描写します。
        /// </summary>
        /// <param name="content"></param>
        private void DrawYText(DrawingContext content)
        {
            var item = Items?.FirstOrDefault();

            if (item == null) return;

            var strings = 
                Util.GetScaleStrings(item.Min, item.Max, ScaleSplitCountY, item.Format)
                .Select(s => GetFormattedText(s))
                .ToArray();

            foreach (var i in Enumerable.Range(0, ScaleSplitCountY + 1))
            {
                var text = strings[i];
                var x = ScaleLineLength * -1 + Margin.Left;
                var y = GraphHeight - GraphHeight / ScaleSplitCountY * i + Margin.Top;
                
                // 文字を描写
                content.DrawText(text, new Point(x - text.Width, y - text.Height / 2));
            }

            // Y軸表題と描写位置設定
            var header = GetFormattedText(item.Title);
            var point = new Point(0, GraphHeight / 2 + Margin.Top + header.Width / 2);

            // Y軸標題の中心で反時計回りに90度回転させて描写
            content.PushTransform(new RotateTransform(-90, point.X, point.Y));
            content.DrawText(header, point);
            content.Pop();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)。
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                //Items.OfType<LineSeries>()
                //    .AsParallel()
                //    .ForAll(item => item.Lines = null);
                Items = null;

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~WriteableBitmapCreator() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
