using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfChartV1.Common;
using WpfUtilV2.Common;

namespace WpfChartV1.Mvvm.UserControls
{
    public class ChartCreator : IDisposable
    {
        internal string Title { get; set; }
        /// <summary>
        /// ｸﾞﾗﾌ以外の領域
        /// </summary>
        internal Thickness OtherThanCanvas;

        /// <summary>
        /// X軸に表示する文字列の列挙体
        /// </summary>
        internal IEnumerable<FormattedText> HeaderXStrings { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌのX軸拡大縮小の比率
        /// </summary>
        internal double ZoomRatioX { get; set; }

        /// <summary>
        /// ｷｬﾝﾊﾞｽの幅
        /// </summary>
        internal double CanvasWidth { get; set; }

        /// <summary>
        /// ｷｬﾝﾊﾞｽの高さ
        /// </summary>
        internal double CanvasHeight { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ描写領域の幅
        /// </summary>
        internal double GraphWidth { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ描写領域の高さ
        /// </summary>
        internal double GraphHeight { get; set; }

        /// <summary>
        /// Freezeした文字色 (ﾊﾟﾌｫｰﾏﾝｽ向上のため)
        /// </summary>
        internal Brush FreezeForeground
        {
            get { return _FreezeForeground; }
            set
            {
                _FreezeForeground = value;
                if (_FreezeForeground != null && _FreezeForeground.CanFreeze) _FreezeForeground.Freeze();
            }
        }
        private Brush _FreezeForeground;

        /// <summary>
        /// Freezeしたﾍﾟﾝ (ﾊﾟﾌｫｰﾏﾝｽ向上のため)
        /// </summary>
        internal Pen FreezePen
        {
            get { return _FreezePen; }
            set
            {
                _FreezePen = value;
                if (_FreezePen != null && _FreezePen.CanFreeze) _FreezePen.Freeze();
            }
        }
        private Pen _FreezePen;

        /// <summary>
        /// X軸の分割数を取得、または設定します。
        /// </summary>
        internal int ScaleSplitCountX { get; set; }

        /// <summary>
        /// Y軸の分割数を取得、または設定します。
        /// </summary>
        internal int ScaleSplitCountY { get; set; }

        /// <summary>
        /// 目盛り線の長さを取得、または設定します。
        /// </summary>
        internal double ScaleLineLength { get; set; }

        /// <summary>
        /// X軸の右端の基準日を取得、または設定します。
        /// </summary>
        internal DateTime EndTimeX { get; set; }

        /// <summary>
        /// X軸の左端の基準日を取得、または設定します。
        /// </summary>
        internal DateTime BeginTimeX { get; set; }

        /// <summary>
        /// X軸の表示ﾀｲﾌﾟを取得、または設定します。
        /// </summary>
        internal ScaleType ScaleType { get; set; }

        /// <summary>
        /// X軸の書式1(ScaleType=TimeSpan)を取得、または設定します。
        /// </summary>
        internal string TimeSpanFormat1 { get; set; }

        /// <summary>
        /// X軸の書式2(ScaleType=TimeSpan)を取得、または設定します。
        /// </summary>
        internal string TimeSpanFormat2 { get; set; }

        /// <summary>
        /// X軸の書式1(ScaleType=DateTime)を取得、または設定します。
        /// </summary>
        internal string DateTimeFormat1 { get; set; }

        /// <summary>
        /// X軸の書式2(ScaleType=DateTime)を取得、または設定します。
        /// </summary>
        internal string DateTimeFormat2 { get; set; }

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾃﾞｰﾀを取得、または設定します。
        /// </summary>
        internal IEnumerable<Series> Items { get; set; }

        /// <summary>
        /// ﾌｫﾝﾄﾌｧﾐﾘｰを取得、または設定します。
        /// </summary>
        internal FontFamily FontFamily { get; set; }

        /// <summary>
        /// ﾌｫﾝﾄｽﾀｲﾙを取得、または設定します。
        /// </summary>
        internal FontStyle FontStyle { get; set; }

        /// <summary>
        /// ﾌｫﾝﾄの太さを取得、または設定します。
        /// </summary>
        internal FontWeight FontWeight { get; set; }

        /// <summary>
        /// ﾌｫﾝﾄを縮小、または拡大する度合いを取得、または設定します。
        /// </summary>
        internal FontStretch FontStretch { get; set; }

        /// <summary>
        /// ﾌｫﾝﾄｻｲｽﾞを取得、または設定します。
        /// </summary>
        internal double FontSize { get; set; }

        /// <summary>
        /// ｷｬﾝﾊﾞｽを取得、または設定します。
        /// </summary>
        internal RenderTargetBitmap Canvas { get; set; }

        // ****************************************************************************************************
        // ｸﾞﾗﾌ描写
        // ****************************************************************************************************

        /// <summary>
        /// ｷｬﾝﾊﾞｽを作成します。
        /// </summary>
        internal ImageSource DrawCanvas()
        {
            // 内部変数の初期化
            Initialize();

            // ｷｬﾝﾊﾞｽの大きさ決定
            var dpi = WpfUtil.GetDpi(Orientation.Horizontal);
            Canvas?.Clear();
            Canvas = new RenderTargetBitmap((int)CanvasWidth, (int)CanvasHeight, dpi, dpi, PixelFormats.Default);

            // ﾚﾝﾀﾞｰ作成
            var dv = new DrawingVisual();

            using (var dc = dv.RenderOpen())
            {
                // ｸﾞﾗﾌ表示領域を原点にする。
                dc.PushTransform(new TranslateTransform(OtherThanCanvas.Left, OtherThanCanvas.Top));

                // X軸の表題を描写
                DrawXAxis(dc);

                // Y軸の表題を描写
                DrawYAxis(dc);

                // 目盛りを描写
                DrawScale(dc);

                // 折れ線ｸﾞﾗﾌ描写のため、左下を原点にする。
                dc.PushTransform(new ScaleTransform() { CenterY = GraphHeight / 2, ScaleY = -1 });

                //ﾌﾚｰﾑからはみ出た描写を切り捨てる設定を追加
                dc.PushClip(new RectangleGeometry(new Rect(0, 0, GraphWidth, GraphHeight)));

                // 折れ線ｸﾞﾗﾌ描写
                foreach (var item in Items)
                {
                    item.DrawSeries(this, dc);
                }

                // 左下原点、はみ出し設定解除
                dc.Pop();
                dc.Pop();

                // ﾌﾚｰﾑ描写
                DrawFrame(dc);
            }

            // ｱﾝﾁｴｲﾘｱｽ解除してｷｬﾝﾊﾞｽにﾚﾝﾀﾞｰ
            Canvas.Render(Util.SetRenderOptions(dv));
            if (Canvas.CanFreeze) Canvas.Freeze();

            return ConvertToBitmap(Canvas);
        }

        /// <summary>
        /// 内部変数を初期化します。
        /// </summary>
        private void Initialize()
        {
            // Y軸に表示する目盛り文字のｻﾝﾌﾟﾙ
            var ft = GetFormattedText("1");

            // X軸に表示するﾍｯﾀﾞ日付文字の生成
            HeaderXStrings = (
                ScaleType == ScaleType.DateTime
                    ? Util.GetScaleStrings(BeginTimeX, EndTimeX, ScaleSplitCountX, DateTimeFormat1, DateTimeFormat2)
                    : Util.GetScaleStrings(TimeSpan.FromSeconds(0), EndTimeX - BeginTimeX, ScaleSplitCountX, TimeSpanFormat1, TimeSpanFormat2)
                )
                .Select(s => GetFormattedText(s));

            // ｸﾞﾗﾌ以外の領域
            OtherThanCanvas.Top = ft.Height / 2;                                                    // 上=目盛り文字の半分
            OtherThanCanvas.Left = Items.FirstOrDefault().GetHeaderWidth(this) - ScaleLineLength;   // 左=最初のY軸目盛り幅                  TODO 多軸表示対応 TODO Itemsが無い場合の考慮
            OtherThanCanvas.Right = HeaderXStrings.Last().Width / 2;                                // 右=最後のX軸目盛り文字の半分          TODO 多軸表示対応
            OtherThanCanvas.Bottom = HeaderXStrings.Select(s => s.Height).Max() + ScaleLineLength;  // 下=X軸目盛り高さの最大値＋目盛り線

            // ｸﾞﾗﾌ領域の大きさ
            GraphHeight = CanvasHeight - OtherThanCanvas.Top - OtherThanCanvas.Bottom;
            GraphWidth = CanvasWidth - OtherThanCanvas.Left - OtherThanCanvas.Right;

            // ｸﾞﾗﾌ領域の幅でﾃﾞｰﾀ点数を間引く
            Items.Select(item =>
            {
                item.ThinningOut(GraphWidth);
                return item;
            }).ToArray();

            //ｸﾞﾗﾌ内のX軸倍率
            ZoomRatioX = GraphWidth / (EndTimeX - BeginTimeX).Ticks;
        }

        /// <summary>
        /// X軸を描写します。
        /// </summary>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        private void DrawXAxis(DrawingContext dc)
        {
            HeaderXStrings
                .Select((s, index) =>
                {
                    var x = GraphWidth / ScaleSplitCountX * index;
                    var y = GraphHeight + ScaleLineLength;

                    // 文字を描写
                    dc.DrawText(s, new Point(x - s.Width / 2, y));

                    // 目盛り線を描写
                    dc.DrawLine(
                        FreezePen,
                        new Point(x, GraphHeight),
                        new Point(x, GraphHeight + ScaleLineLength)
                    );
                    return s;
                })
                .ToArray();
        }

        /// <summary>
        /// Y軸を描写します。
        /// </summary>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        private void DrawYAxis(DrawingContext dc)
        {
            Items
                .First()
                .DrawHeader(this, dc, 0);
        }

        /// <summary>
        /// 目盛りを描写します。
        /// </summary>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        private void DrawScale(DrawingContext dc)
        {
            var pen = FreezePen.CloneCurrentValue();
            pen.DashStyle = DashStyles.Dot;
            if (pen.CanFreeze) pen.Freeze();

            Enumerable.Range(0, ScaleSplitCountX)
                .Select(i =>
                {
                    var x = GraphWidth / ScaleSplitCountX * i;
                    dc.DrawLine(
                        pen,
                        new Point(x, 0),
                        new Point(x, GraphHeight)
                    );
                    return i;
                })
                .ToArray();

            Enumerable.Range(0, ScaleSplitCountY)
                .Select(i =>
                {
                    var y = GraphHeight / ScaleSplitCountY * i;
                    dc.DrawLine(
                        pen,
                        new Point(0, y),
                        new Point(GraphWidth, y)
                    );
                    return i;
                })
                .ToArray();
        }

        /// <summary>
        /// 枠を描写します。
        /// </summary>
        /// <param name="dc">ﾚﾝﾀﾞｰ</param>
        private void DrawFrame(DrawingContext dc)
        {
            dc.DrawGeometry(null, FreezePen, new PathGeometry(new[]
            {
                Util.CreateLine(new Point[]
                {
                    new Point(0, 0),
                    new Point(0, GraphHeight),
                    new Point(GraphWidth, GraphHeight),
                    new Point(GraphWidth, 0),
                    new Point(0, 0)
                })
            }));
        }

        /// <summary>
        /// 描写用にﾌｫｰﾏｯﾄされたｵﾌﾞｼﾞｪｸﾄを取得します。
        /// </summary>
        /// <param name="text">ｵﾌﾞｼﾞｪｸﾄに描写するﾒｯｾｰｼﾞ</param>
        /// <returns></returns>
        internal FormattedText GetFormattedText(string text)
        {
            return Util.GetFormattedText(text, FreezeForeground, FontFamily, FontStyle, FontWeight, FontStretch, FontSize);
        }

        /// <summary>
        /// ｷｬﾝﾊﾞｽをﾋﾞｯﾄﾏｯﾌﾟﾃﾞｰﾀに変換します。
        /// </summary>
        /// <param name="target">ｷｬﾝﾊﾞｽ</param>
        /// <returns></returns>
        private BitmapImage ConvertToBitmap(BitmapSource target)
        {
            using (var outStream = new WrappingStream(new MemoryStream()))
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(target));
                enc.Save(outStream);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = outStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                if (bitmap.CanFreeze)
                {
                    bitmap.Freeze();
                }
                return bitmap;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Items.Cast<LineSeries>()
                        .Select(i => { i.Lines = null; return i; })
                        .ToArray();
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)。
                    DateTimeFormat1 = null;
                    DateTimeFormat2 = null;
                    FreezeForeground = null;
                    FreezePen = null;
                    Items = null;
                    TimeSpanFormat1 = null;
                    TimeSpanFormat2 = null;
                    FontFamily = null;
                    Canvas?.Clear();
                    Canvas = null;
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~ChartParameter() {
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
