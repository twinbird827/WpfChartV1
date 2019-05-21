using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfChartV1.Common;
using WpfUtilV2.Common;
using WpfUtilV2.Extensions;

namespace WpfChartV1.Mvvm.UserControls
{
    /// <summary>
    /// DateOrTimeSingleChart.xaml の相互作用ロジック
    /// </summary>
    public partial class DateOrTimeSingleChart : UserControl
    {
        #region DependencyProperty

        private static DependencyProperty Register<T>(string name, T defaultValue, PropertyChangedCallback callback)
        {
            return DependencyProperty.Register(
                name,
                typeof(T),
                typeof(DateOrTimeSingleChart),
                new FrameworkPropertyMetadata(defaultValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, callback)
            );
        }

        public static readonly DependencyProperty XBeginTimeProperty =
            Register(nameof(XBeginTime), default(TimeSpan), null);

        public static readonly DependencyProperty XBeginDateProperty =
            Register(nameof(XBeginDate), default(DateTime), null);

        public static readonly DependencyProperty XRangeProperty =
            Register(nameof(XRange), default(TimeSpan), null);

        public static readonly DependencyProperty XDateFormat1Property =
            Register(nameof(XDateFormat1), default(string), null);

        public static readonly DependencyProperty XDateFormat2Property =
            Register(nameof(XDateFormat2), default(string), null);

        public static readonly DependencyProperty XDateFormat3Property =
            Register(nameof(XDateFormat3), default(string), null);

        public static readonly DependencyProperty XTimeFormat1Property =
            Register(nameof(XTimeFormat1), default(string), null);

        public static readonly DependencyProperty XTimeFormat2Property =
            Register(nameof(XTimeFormat2), default(string), null);

        public static readonly DependencyProperty XTimeFormat3Property =
            Register(nameof(XTimeFormat3), default(string), null);

        public static readonly DependencyProperty XScaleSplitProperty =
            Register(nameof(XScaleSplit), default(int), null);

        public static readonly DependencyProperty ScaleTypeProperty =
            Register(nameof(ScaleType), default(DateOrTimeType), null);

        public static readonly DependencyProperty ChartsProperty =
            Register(nameof(Charts), default(IEnumerable<DateOrTimeChart>), null);

        public static readonly DependencyProperty RedrawProperty =
            Register(nameof(Redraw), default(object), OnSetRedrawCallback);

        #endregion

        public DateOrTimeSingleChart()
        {
            InitializeComponent();

            ScaleType = DateOrTimeType.TimeSpan;
            XDateFormat1 = @"{0:MM/dd}";
            XDateFormat2 = @"{0:HH:mm}";
            XDateFormat3 = @"{0:MM/dd HH:mm:ss}";
            XTimeFormat1 = @"{0:dd}日";
            XTimeFormat2 = @"{0:hh\:mm}";
            XTimeFormat3 = @"{0:dd'日 'hh':'mm':'ss}";

            baseContainer.DataContext = this;
        }

        /// <summary>
        /// X軸の開始値
        /// </summary>
        public TimeSpan XBeginTime
        {
            get { return (TimeSpan)GetValue(XBeginTimeProperty); }
            set { SetValue(XBeginTimeProperty, value); }
        }

        /// <summary>
        /// X軸の開始値
        /// </summary>
        public DateTime XBeginDate
        {
            get { return (DateTime)GetValue(XBeginDateProperty); }
            set { SetValue(XBeginDateProperty, value); }
        }

        /// <summary>
        /// X軸の終了値
        /// </summary>
        public TimeSpan XRange
        {
            get { return (TimeSpan)GetValue(XRangeProperty); }
            set { SetValue(XRangeProperty, value); }
        }

        /// <summary>
        /// X軸のﾌｫｰﾏｯﾄ1 (時間軸にした場合の日付部分)
        /// </summary>
        public string XDateFormat1
        {
            get { return (string)GetValue(XDateFormat1Property); }
            set { SetValue(XDateFormat1Property, value); }
        }

        /// <summary>
        /// X軸のﾌｫｰﾏｯﾄ2 (時間軸にした場合の時間部分)
        /// </summary>
        public string XDateFormat2
        {
            get { return (string)GetValue(XDateFormat2Property); }
            set { SetValue(XDateFormat2Property, value); }
        }

        /// <summary>
        /// X軸のﾌｫｰﾏｯﾄ3 (右ｸﾘｯｸ時に表示するﾌｫｰﾏｯﾄ)
        /// </summary>
        public string XDateFormat3
        {
            get { return (string)GetValue(XDateFormat3Property); }
            set { SetValue(XDateFormat3Property, value); }
        }

        /// <summary>
        /// X軸のﾌｫｰﾏｯﾄ1 (時間軸にした場合の日付部分)
        /// </summary>
        public string XTimeFormat1
        {
            get { return (string)GetValue(XTimeFormat1Property); }
            set { SetValue(XTimeFormat1Property, value); }
        }

        /// <summary>
        /// X軸のﾌｫｰﾏｯﾄ2 (時間軸にした場合の時間部分)
        /// </summary>
        public string XTimeFormat2
        {
            get { return (string)GetValue(XTimeFormat2Property); }
            set { SetValue(XTimeFormat2Property, value); }
        }

        /// <summary>
        /// X軸のﾌｫｰﾏｯﾄ3 (右ｸﾘｯｸ時に表示するﾌｫｰﾏｯﾄ)
        /// </summary>
        public string XTimeFormat3
        {
            get { return (string)GetValue(XTimeFormat3Property); }
            set { SetValue(XTimeFormat3Property, value); }
        }

        /// <summary>
        /// X軸の分割数
        /// </summary>
        public int XScaleSplit
        {
            get { return (int)GetValue(XScaleSplitProperty); }
            set { SetValue(XScaleSplitProperty, value); }
        }

        /// <summary>
        /// X軸の書式ﾀｲﾌﾟ
        /// </summary>
        public DateOrTimeType ScaleType
        {
            get { return (DateOrTimeType)GetValue(ScaleTypeProperty); }
            set { SetValue(ScaleTypeProperty, value); }
        }

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾃﾞｰﾀを取得、または設定します。
        /// </summary>
        public IEnumerable<DateOrTimeChart> Charts
        {
            get { return (IEnumerable<DateOrTimeChart>)GetValue(ChartsProperty); }
            set
            {
                Charts?.SelectMany(c => c.Series).AsParallel()
                    .ForAll(series => series.Lines = null);
                SetValue(ChartsProperty, value);
            }
        }

        /// <summary>
        /// X軸のﾌｫｰﾏｯﾄ3 (右ｸﾘｯｸ時に表示するﾌｫｰﾏｯﾄ)
        /// </summary>
        public object Redraw
        {
            get { return GetValue(RedrawProperty); }
            set { SetValue(RedrawProperty, value); }
        }

        /// <summary>
        /// Chartsﾌﾟﾛﾊﾟﾃｨ変更時のｲﾍﾞﾝﾄ
        /// </summary>
        private static void OnSetRedrawCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            var c = sender as DateOrTimeSingleChart;
            if (c != null) c.Draw();
        }

        /// <summary>
        /// ﾏｳｽ右ｸﾘｯｸされている状態か
        /// </summary>
        private bool IsMouseRightClicking { get; set; } = false;

        /// <summary>
        /// ﾏｳｽ右ｸﾘｯｸされた時点のﾚﾝﾀﾞ
        /// </summary>
        private ImageSource PreviousXAxisImage { get; set; }

        /// <summary>
        /// 短軸ﾁｬｰﾄを描写します。
        /// </summary>
        private void Draw()
        {
            if (WpfUtil.IsDesignMode())
            {
                return;
            }
            if (Charts == null || !Charts.Any())
            {
                return;
            }
            if (ActualHeight <= 0 || ActualWidth <= 0)
            {
                return;
            }
            if (IsMouseRightClicking)
            {
                return;
            }

            // X軸の文字
            var xheaders = GetXHeaders();

            // X軸をﾚﾝﾀﾞﾘﾝｸﾞする。
            XAxisImage.Source = DrawXAxis(xheaders);
            PreviousXAxisImage = XAxisImage.Source;

            // 各ﾁｬｰﾄ描写の割合合計
            var percentage = Charts.Sum(c => c.PercentageHeight);

            // 各ﾁｬｰﾄをﾚﾝﾀﾞﾘﾝｸﾞする。
            foreach (var c in Charts)
            {
                c.Width = ActualWidth;
                c.Height = (ActualHeight - XAxisImage.Source.Height) / percentage * c.PercentageHeight;
                c.Draw(this, xheaders);
            }

        }

        /// <summary>
        /// X軸のﾍｯﾀﾞ文字を取得します。
        /// </summary>
        /// <param name="c">ｺﾝﾃﾅ</param>
        /// <returns></returns>
        private FormattedText[] GetXHeaders()
        {
            var xheaders = ScaleType == DateOrTimeType.DateTime
                ? Util.GetScaleStrings(XBeginDate + XBeginTime, XRange, XScaleSplit, XDateFormat1, XDateFormat2)
                : Util.GetScaleStrings(XBeginTime, XRange, XScaleSplit, XTimeFormat1, XTimeFormat2);

            return xheaders
                .Select(s => Util.GetFormattedText(s, this))
                .ToArray();
        }

        private FormattedText GetXHeader(double x)
        {
            var first = Charts.First();
            // x地点を表す時刻
            var xtime = TimeSpan.FromTicks((long)(XRange.Ticks / first.GWidth * (x - Util.MarginLeft)));
            // x地点を表す日付
            var xdate = XBeginDate + XBeginTime + xtime;

            var xheader = ScaleType == DateOrTimeType.DateTime
                ? $"{string.Format(XDateFormat3, xdate)}"
                : $"{string.Format(XTimeFormat3, xtime)}";

            return Util.GetFormattedText(xheader, Util.RightClickBrush, this);
        }


        private ImageSource DrawXAxis(FormattedText[] xheaders)
        {
            // 高さと幅
            var height = xheaders.Max(h => h.Height);
            var width = ActualWidth;
            // X軸描写領域
            var gwidth = width - Util.MarginLeft - xheaders.Last().Width / 2;
            // X軸の文字を描写
            var dv = new DrawingGroup();

            using (var content = dv.Open())
            {
                // X軸の全体枠
                content.DrawImage(Util.CreateWriteableBitmap((int)width, (int)height));

                // X軸の文字を描写
                foreach (var i in Enumerable.Range(0, xheaders.Length))
                {
                    var text = xheaders[i];
                    var x = gwidth / (xheaders.Length - 1) * i + Util.MarginLeft;
                    var y = 0;

                    // 文字を描写
                    content.DrawText(text, new Point(x - text.Width / 2, y));
                }
            }

            // ｱﾝﾁｴｲﾘｱｽ解除
            var image = new DrawingImage(Util.ReleaseAntialiasing(dv));

            // FreezeしたImageSourceを返却
            return image.Frozen();
        }

        private ImageSource DrawXAxisWhenMouseClick(Point p)
        {
            // X軸の文字を描写
            var dv = new DrawingGroup();

            // X軸の文字を描写
            var text = GetXHeader(p.X);

            // ﾋﾞｯﾄﾏｯﾌﾟの大きさ決定
            var bitmap = Util.CreateWriteableBitmap((int)PreviousXAxisImage.Width, (int)PreviousXAxisImage.Height);

            // ﾋﾞｯﾄﾏｯﾌﾟに線を描写
            using (var context = bitmap.GetBitmapContext())
            {
                var x1 = p.X - text.Width / 2;
                var y1 = 0;
                var x2 = p.X + text.Width / 2;
                var y2 = text.Height;
                bitmap.FillRectangle((int)x1, (int)y1, (int)x2, (int)y2, Util.RightClickBackground);
            }

            using (var content = dv.Open())
            {
                content.DrawImage(PreviousXAxisImage);

                content.DrawImage(bitmap);

                content.DrawText(text, new Point(p.X - text.Width / 2, 0));
            }

            // ｱﾝﾁｴｲﾘｱｽ解除
            var image = new DrawingImage(Util.ReleaseAntialiasing(dv));

            // FreezeしたImageSourceを返却
            return image.Frozen();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // 最初の描画が終了後、Draw を呼ぶ
            Dispatcher.BeginInvoke(
                new Action(() => Redraw = new object()),
                DispatcherPriority.Loaded
            );
        }

        /// <summary>
        /// ﾚﾝﾀﾞﾘﾝｸﾞｻｲｽﾞ変更時
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            Redraw = new object();
        }

        /// <summary>
        /// ﾏｳｽ右ｸﾘｯｸ時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            IsMouseRightClicking = true;

            var p = e.GetPosition(e.OriginalSource as IInputElement);
            var draw = true;

            foreach (var c in Charts)
            {
                draw = draw && c.DrawMouseLine(this, p);
            }
            if (draw)
            {
                XAxisImage.Source = DrawXAxisWhenMouseClick(p);
            }

        }

        /// <summary>
        /// ｺﾝﾄﾛｰﾙ上でのﾏｳｽｶｰｿﾙ移動時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        /// <summary>
        /// ﾏｳｽ右ｸﾘｯｸ終了時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonUp(e);

            // ﾌﾗｸﾞ解除
            IsMouseRightClicking = false;

            // 再描写
            Redraw = new object();
        }

    }
}
