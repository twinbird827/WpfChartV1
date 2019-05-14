using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WpfChartV1.Mvvm.UserControls
{
    /// <summary>
    /// SingleAxisChart.xaml の相互作用ロジック
    /// </summary>
    public partial class SingleAxisChart : UserControl
    {
        public SingleAxisChart()
        {
            InitializeComponent();

            baseContainer.DataContext = this;
        }

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾃﾞｰﾀを取得、または設定します。
        /// </summary>
        public bool IsCustomize
        {
            get { return (bool)GetValue(IsCustomizeProperty); }
            set { SetValue(IsCustomizeProperty, value); }
        }

        public static readonly DependencyProperty IsCustomizeProperty =
            DependencyProperty.Register("IsCustomize",
                typeof(bool),
                typeof(SingleAxisChart),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null)
            );

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾌﾗｸﾞを取得、または設定します。
        /// </summary>
        public bool Redraw
        {
            get { return (bool)GetValue(RedrawProperty); }
            set { SetValue(RedrawProperty, value); }
        }

        public static readonly DependencyProperty RedrawProperty =
            DependencyProperty.Register("Redraw",
                typeof(bool),
                typeof(SingleAxisChart),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSetRedrawCallback))
            );

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾌﾗｸﾞを取得、または設定します。
        /// </summary>
        public TimeSpan XRange
        {
            get { return (TimeSpan)GetValue(XRangeProperty); }
            set { SetValue(XRangeProperty, value); }
        }

        public static readonly DependencyProperty XRangeProperty =
            DependencyProperty.Register("XRange",
                typeof(TimeSpan),
                typeof(SingleAxisChart),
                new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null)
            );

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾌﾗｸﾞを取得、または設定します。
        /// </summary>
        public TimeSpan XStartTime
        {
            get { return (TimeSpan)GetValue(XStartTimeProperty); }
            set { SetValue(XStartTimeProperty, value); }
        }

        public static readonly DependencyProperty XStartTimeProperty =
            DependencyProperty.Register("XStartTime",
                typeof(TimeSpan),
                typeof(SingleAxisChart),
                new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null)
            );

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾌﾗｸﾞを取得、または設定します。
        /// </summary>
        public DateTime XStartDate
        {
            get { return (DateTime)GetValue(XStartDateProperty); }
            set { SetValue(XStartDateProperty, value); }
        }

        public static readonly DependencyProperty XStartDateProperty =
            DependencyProperty.Register("XStartDate",
                typeof(DateTime),
                typeof(SingleAxisChart),
                new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null)
            );

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾃﾞｰﾀを取得、または設定します。
        /// </summary>
        public IEnumerable<LineChart> Charts
        {
            get { return (IEnumerable<LineChart>)GetValue(ChartsProperty); }
            set
            {
                Charts?.SelectMany(c => c.Series).AsParallel()
                    .ForAll(series => series.Lines = null);
                SetValue(ChartsProperty, value);
            }
        }

        public static readonly DependencyProperty ChartsProperty =
            DependencyProperty.Register("Charts", 
                typeof(IEnumerable<LineChart>),
                typeof(SingleAxisChart),
                new UIPropertyMetadata()
            );

        /// <summary>
        /// ﾏｳｽ右ｸﾘｯｸされている状態か
        /// </summary>
        private bool IsMouseRightClicking { get; set; } = false;

        private ImageSource PreviousRender { get; set; }

        /// <summary>
        /// Chartsﾌﾟﾛﾊﾟﾃｨ変更時のｲﾍﾞﾝﾄ
        /// </summary>
        private static void OnSetRedrawCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            var c = sender as SingleAxisChart;
            if (c != null) c.Draw();
        }

        private void Draw()
        {
            Redraw = false;

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

            // 最後のﾁｬｰﾄを取得(X軸のﾀｲﾄﾙ描写用)
            var last = Charts.LastOrDefault();

            // ﾍｯﾀﾞに表示する文字の配列を作成
            var xheaders = last.GetXHeaders(this);

            // X軸をﾚﾝﾀﾞﾘﾝｸﾞする。
            XAxisImage.Source = DrawXAxis(xheaders);
            PreviousRender = XAxisImage.Source;

            if (IsCustomize)
            {
                var c1 = Charts.First();
                var c2 = Charts.Skip(1).FirstOrDefault();
                var c2Height = c2 != null ? 100 : 0;
                c1.Width = ActualWidth;
                c1.Height = (ActualHeight - xheaders.Max(h => h.Height) - c2Height);
                c1.Draw(this, xheaders);

                if (c2 != null)
                {
                    c2.Width = ActualWidth;
                    c2.Height = c2Height;
                    c2.Draw(this, xheaders);
                }
            }
            else
            {
                // 各ﾁｬｰﾄをﾚﾝﾀﾞﾘﾝｸﾞする。
                foreach (var c in Charts)
                {
                    c.Width = ActualWidth;
                    c.Height = (ActualHeight - xheaders.Max(h => h.Height)) / Charts.Count();
                    c.Draw(this, xheaders);
                }
            }
        }

        private ImageSource DrawXAxis(FormattedText[] headers)
        {
            // 高さと幅を取得
            var height = headers.Max(h => h.Height);
            var width = ActualWidth;

            // X軸の文字を描写
            var dv = new DrawingGroup();

            using (var content = dv.Open())
            {
                content.DrawImage(Util.CreateWriteableBitmap((int)width, (int)height));

                // X軸の文字を描写
                DrawXText(content, width, headers);
            }

            // ｱﾝﾁｴｲﾘｱｽ解除
            var returnImage = new DrawingImage(Util.ReleaseAntialiasing(dv));

            // FreezeしたImageSourceを返却
            return returnImage.GetAsFrozen() as ImageSource;
        }

        /// <summary>
        /// X軸のﾍｯﾀﾞ文字を描写します。
        /// </summary>
        /// <param name="content"></param>
        private void DrawXText(DrawingContext content, double width, FormattedText[] headers)
        {
            var gwidth = width - Util.MarginLeft - headers.Last().Width / 2;
            foreach (var i in Enumerable.Range(0, headers.Length))
            {
                var text = headers[i];
                var x = gwidth / (headers.Length - 1) * i + Util.MarginLeft;
                var y = 0;

                // 文字を描写
                content.DrawText(text, new Point(x - text.Width / 2, y));
            }
        }

        private ImageSource DrawWhenMouseClick(Point p)
        {
            // X軸の文字を描写
            var dv = new DrawingGroup();

            // X軸の文字を描写
            var text = Charts.First().GetXHeaderDate(this, p.X);

            // ﾋﾞｯﾄﾏｯﾌﾟの大きさ決定
            var bitmap = Util.CreateWriteableBitmap((int)PreviousRender.Width, (int)PreviousRender.Height);

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
                content.DrawImage(PreviousRender);

                content.DrawImage(bitmap);

                content.DrawText(text, new Point(p.X - text.Width / 2, 0));
            }

            // ｱﾝﾁｴｲﾘｱｽ解除
            var returnImage = new DrawingImage(Util.ReleaseAntialiasing(dv));

            // FreezeしたImageSourceを返却
            return returnImage.GetAsFrozen() as ImageSource;
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
                new Action(() => Redraw = true),
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

            Redraw = true;
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
                XAxisImage.Source = DrawWhenMouseClick(p);
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
            Redraw = true;
        }
    }
}
