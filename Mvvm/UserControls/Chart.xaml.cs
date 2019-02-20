using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Chart.xaml の相互作用ロジック
    /// </summary>
    public partial class Chart : UserControl
    {
        /// <summary>
        /// ｺﾝｽﾄﾗｸﾀ
        /// </summary>
        public Chart()
        {
            InitializeComponent();

            Loaded += (sender, e) => LoadedCharts.Add(this);
            Unloaded += (sender, e) =>
            {
                LoadedCharts.Remove(this);
                Items = null;
                LastRenderImage = null;
            };
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // 最初の描画が終了後、Draw を呼ぶ
            Dispatcher.BeginInvoke(
                new Action(() => Draw()),
                DispatcherPriority.Loaded
            );
        }

        // ****************************************************************************************************
        // 公開ﾌﾟﾛﾊﾟﾃｨ定義
        // ****************************************************************************************************

        /// <summary>
        /// X軸の分割数を取得、または設定します。
        /// </summary>
        public int ScaleSplitCountX
        {
            get { return (int)GetValue(ScaleSplitCountXProperty); }
            set { SetValue(ScaleSplitCountXProperty, value); }
        }

        public static readonly DependencyProperty ScaleSplitCountXProperty =
            DependencyProperty.Register("ScaleSplitCountX", typeof(int), typeof(Chart), new PropertyMetadata(5));

        /// <summary>
        /// Y軸の分割数を取得、または設定します。
        /// </summary>
        public int ScaleSplitCountY
        {
            get { return (int)GetValue(ScaleSplitCountYProperty); }
            set { SetValue(ScaleSplitCountYProperty, value); }
        }

        public static readonly DependencyProperty ScaleSplitCountYProperty =
            DependencyProperty.Register("ScaleSplitCountY", typeof(int), typeof(Chart), new PropertyMetadata(5));

        /// <summary>
        /// 目盛り線の長さを取得、または設定します。
        /// </summary>
        public double ScaleLineLength
        {
            get { return (double)GetValue(ScaleLineLengthProperty); }
            set { SetValue(ScaleLineLengthProperty, value); }
        }

        public static readonly DependencyProperty ScaleLineLengthProperty =
            DependencyProperty.Register("ScaleLineLength", typeof(double), typeof(Chart), new PropertyMetadata(5d));

        /// <summary>
        /// X軸の右端の基準日を取得、または設定します。
        /// </summary>
        public DateTime EndTimeX
        {
            get { return (DateTime)GetValue(EndTimeXProperty); }
            set { SetValue(EndTimeXProperty, value); }
        }

        public static readonly DependencyProperty EndTimeXProperty =
            DependencyProperty.Register("EndTimeX", typeof(DateTime), typeof(Chart), new PropertyMetadata(default(DateTime)));

        /// <summary>
        /// X軸の左端の基準日を取得、または設定します。
        /// </summary>
        public DateTime BeginTimeX
        {
            get { return (DateTime)GetValue(BeginTimeXProperty); }
            set { SetValue(BeginTimeXProperty, value); }
        }

        public static readonly DependencyProperty BeginTimeXProperty =
            DependencyProperty.Register("BeginTimeX", typeof(DateTime), typeof(Chart), new PropertyMetadata(default(DateTime)));

        /// <summary>
        /// X軸の表示ﾀｲﾌﾟを取得、または設定します。
        /// </summary>
        public ScaleType ScaleType
        {
            get { return (ScaleType)GetValue(ScaleTypeProperty); }
            set { SetValue(ScaleTypeProperty, value); }
        }

        public static readonly DependencyProperty ScaleTypeProperty =
            DependencyProperty.Register("ScaleType", typeof(ScaleType), typeof(Chart), new PropertyMetadata(default(ScaleType)));

        /// <summary>
        /// X軸の書式1(ScaleType=TimeSpan)を取得、または設定します。
        /// </summary>
        public string TimeSpanFormat1
        {
            get { return (string)GetValue(TimeSpanFormat1Property); }
            set { SetValue(TimeSpanFormat1Property, value); }
        }

        public static readonly DependencyProperty TimeSpanFormat1Property =
            DependencyProperty.Register("TimeSpanFormat1", typeof(string), typeof(Chart), new PropertyMetadata(default(string)));

        /// <summary>
        /// X軸の書式2(ScaleType=TimeSpan)を取得、または設定します。
        /// </summary>
        public string TimeSpanFormat2
        {
            get { return (string)GetValue(TimeSpanFormat2Property); }
            set { SetValue(TimeSpanFormat2Property, value); }
        }

        public static readonly DependencyProperty TimeSpanFormat2Property =
            DependencyProperty.Register("TimeSpanFormat2", typeof(string), typeof(Chart), new PropertyMetadata(default(string)));

        /// <summary>
        /// X軸の書式1(ScaleType=DateTime)を取得、または設定します。
        /// </summary>
        public string DateTimeFormat1
        {
            get { return (string)GetValue(DateTimeFormat1Property); }
            set { SetValue(DateTimeFormat1Property, value); }
        }

        public static readonly DependencyProperty DateTimeFormat1Property =
            DependencyProperty.Register("DateTimeFormat1", typeof(string), typeof(Chart), new PropertyMetadata(default(string)));

        /// <summary>
        /// X軸の書式2(ScaleType=DateTime)を取得、または設定します。
        /// </summary>
        public string DateTimeFormat2
        {
            get { return (string)GetValue(DateTimeFormat2Property); }
            set { SetValue(DateTimeFormat2Property, value); }
        }

        public static readonly DependencyProperty DateTimeFormat2Property =
            DependencyProperty.Register("DateTimeFormat2", typeof(string), typeof(Chart), new PropertyMetadata(default(string)));

        /// <summary>
        /// X軸を表示するかどうかを取得、または設定します。
        /// </summary>
        public bool IsVisibleXHeader
        {
            get { return (bool)GetValue(IsVisibleXHeaderProperty); }
            set { SetValue(IsVisibleXHeaderProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleXHeaderProperty =
            DependencyProperty.Register("IsVisibleXHeader", typeof(bool), typeof(Chart), new PropertyMetadata(default(bool)));

        /// <summary>
        /// ｸﾞﾗﾌ表示用ﾃﾞｰﾀを取得、または設定します。
        /// </summary>
        public IEnumerable<Series> Items
        {
            get { return (IEnumerable<Series>)GetValue(ItemsProperty); }
            set
            {
                Items?.OfType<LineSeries>().AsParallel()
                    .ForAll(i => i.Lines = null);

                SetValue(ItemsProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<Series>), typeof(Chart), 
                new FrameworkPropertyMetadata(
                    new PropertyChangedCallback(OnItemsChanged)
                ));

        // ****************************************************************************************************
        // 内部ﾌﾟﾛﾊﾟﾃｨ定義
        // ****************************************************************************************************

        /// <summary>
        /// 最後に描写したｲﾒｰｼﾞｿｰｽ
        /// </summary>
        internal ImageSource LastRenderImage { get; set; }

        internal bool IsMouseRightDown { get; set; } = false;

        internal static List<Chart> LoadedCharts { get; set; } = new List<Chart>();

        // ****************************************************************************************************
        // 内部ﾒｿｯﾄﾞ定義
        // ****************************************************************************************************

        /// <summary>
        /// Itemsﾌﾟﾛﾊﾟﾃｨ変更時のｲﾍﾞﾝﾄ
        /// </summary>
        private static void OnItemsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var c = sender as Chart;
            if (c != null) c.Draw();
        }

        /// <summary>
        /// ｲﾒｰｼﾞを描写します。
        /// </summary>
        public void Draw()
        {
            if (WpfUtil.IsDesignMode())
            {
                return;
            }
            if (Items == null)
            {
                return;
            }
            if (ActualHeight <= 0 || ActualWidth <= 0)
            {
                return;
            }
            if (IsMouseRightDown)
            {
                return;
            }
            using (var creator = WriteableBitmapCreator.CreateInstance(this))
            {
                baseImage.Source = creator.CreateImage().GetAsFrozen() as ImageSource;
                LastRenderImage = baseImage.Source;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            Draw();
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            foreach (var c in LoadedCharts)
            {
                using (var creator = WriteableBitmapCreator.CreateInstance(c))
                {
                    c.baseImage.Source = creator.CreateImage(c.LastRenderImage, e).GetAsFrozen() as ImageSource;
                    c.IsMouseRightDown = true;
                }
            }
        }

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonUp(e);

            foreach (var c in LoadedCharts)
            {
                c.baseImage.Source = c.LastRenderImage;
                c.IsMouseRightDown = false;
            }
        }
    }
}
