using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andy.WPF.SimpleMVVM.Interface
{
    interface INotifyModelChanged
    {
        /// <summary>
        /// 通知整个模型的数据已经修改
        /// </summary>
        void NotifyModelChanged();
    }
}
