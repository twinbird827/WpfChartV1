﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        /// ｸﾞﾗﾌ表示用ﾃﾞｰﾀを取得、または設定します。
        /// </summary>
        public IEnumerable<Series> Items
        {
            get { return (IEnumerable<Series>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<Series>), typeof(Chart), 
                new FrameworkPropertyMetadata(
                    new PropertyChangedCallback(Chart.OnItemsChanged)
                ));

        // ****************************************************************************************************
        // 内部ﾒｿｯﾄﾞ定義
        // ****************************************************************************************************

        /// <summary>
        /// Itemsﾌﾟﾛﾊﾟﾃｨ変更時のｲﾍﾞﾝﾄ
        /// </summary>
        private static void OnItemsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var c = sender as Chart;
            if (c != null) c.Draw().ConfigureAwait(false);
        }

        /// <summary>
        /// ｲﾒｰｼﾞを描写します。
        /// </summary>
        private async Task Draw()
        {
            if (WpfUtil.IsDesignMode())
            {
                return;
            }
            if (Items == null || !Items.Any())
            {
                return;
            }

            // ﾁｬｰﾄﾊﾟﾗﾒｰﾀに必要なﾊﾟﾗﾒｰﾀはUIﾊﾟﾗﾒｰﾀも必要なのでｶﾚﾝﾄｽﾚｯﾄﾞで作成
            using (ChartCreator param = new ChartCreator()
            {
                Title = this.Name,
                BeginTimeX = this.BeginTimeX,
                CanvasHeight = this.ActualHeight,
                CanvasWidth = this.ActualWidth,
                DateTimeFormat1 = this.DateTimeFormat1,
                DateTimeFormat2 = this.DateTimeFormat2,
                EndTimeX = this.EndTimeX,
                FreezeForeground = this.Foreground,
                FreezePen = new Pen(Foreground, 1),
                Items = this.Items,
                ScaleLineLength = this.ScaleLineLength,
                ScaleSplitCountX = this.ScaleSplitCountX,
                ScaleSplitCountY = this.ScaleSplitCountY,
                ScaleType = this.ScaleType,
                TimeSpanFormat1 = this.TimeSpanFormat1,
                TimeSpanFormat2 = this.TimeSpanFormat2,
                FontFamily = this.FontFamily,
                FontSize = this.FontSize,
                FontStretch = this.FontStretch,
                FontStyle = this.FontStyle,
                FontWeight = this.FontWeight,
            })
            {
                await Task.Run(() =>
                {
                    // ｲﾒｰｼﾞはﾊﾞｯｸｸﾞﾗｳﾝﾄﾞで作成
                    return param.DrawCanvas();
                })
                .ContinueWith(
                    image => baseImage.Source = image.Result,
                    TaskScheduler.FromCurrentSynchronizationContext()
                );
            }
        }
    }

}
