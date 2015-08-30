using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.WPF.SimpleMVVM
{
    /// <summary>
    /// ObservableCollection 不允许非创建的线程修改，如果有非创建线程的修改触发了OnCollectionChanged或OnPropertyChanged事件
    /// 则会应发NotSupportException("cross thread access")
    /// 本类通过重载OnCollectionChanged和OnPropertyChanged，判断如果是非创建线程修改了Collection，则通过通过调用创建线程的
    /// 同步上下文发送触发事件的通知
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        //创建对象的同步上下文
        private SynchronizationContext _createContext = SynchronizationContext.Current;
        #region ModelLock
        private object _modelLock = new object();
        public object ModelLock { get { return _modelLock; } } //模型锁
        #endregion

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (SynchronizationContext.Current == _createContext)
            {
                //在创建对象的同步线程改变
                base.OnCollectionChanged(e);
            }
            else
            {
                //非创建本对象的线程触发修改相应
                _createContext.Post(OnCollectionChangedByOtherContext, e);
            }
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (SynchronizationContext.Current == _createContext)
            {
                //在创建对象的同步线程改变
                base.OnPropertyChanged(e);
            }
            else
            {
                //非创建本对象的线程触发修改相应
                _createContext.Post(OnPropertyChangedByOtherContext, e);
            }
        }
        private void OnPropertyChangedByOtherContext(object state)
        {
            base.OnPropertyChanged((PropertyChangedEventArgs)state);
        }
        private void OnCollectionChangedByOtherContext(object state)
        {
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)state);
        }
    }
}
