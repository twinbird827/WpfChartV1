using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfChartV1.Mvvm.UserControls
{
    public enum ScaleType
    {
        /// <summary>
        /// 横軸を経過時間で描写します。
        /// </summary>
        TimeSpan,

        /// <summary>
        /// 横軸を日時で描写します。
        /// </summary>
        DateTime
    }
}
