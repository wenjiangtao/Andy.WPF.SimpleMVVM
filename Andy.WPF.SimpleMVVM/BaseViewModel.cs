using Andy.WPF.SimpleMVVM.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.WPF.SimpleMVVM
{
    public abstract class BaseViewModel:INotifyPropertyChanged,INotifyPropertyChanging,INotifyModelChanged
    {
        private PropertyInfo[] _properties; //属性
        private SynchronizationContext _createModelContext = SynchronizationContext.Current;//创建对象的同步上下文

        public BaseViewModel()
        {
            _properties = this.GetType().GetProperties();
            ModelLock = new object();
        }

        #region ModelChanging
        private volatile bool _modelChanging;
        public bool ModelChanging { get { return _modelChanging; } set { _modelChanging = value; } }
        #endregion
        #region ModelLock
        public object ModelLock { get; set; } //模型锁
        #endregion
        #region Property
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="PropertyName">属性名，大小写一致</param>
        /// <param name="Value">新值</param>
        public void SetPropertyValue(string PropertyName, object Value)
        {
            foreach (var property in _properties)
            {
                if(property.Name == PropertyName)
                {
                    property.SetValue(this, Value, null);
                    NotifyPropertyChanged(PropertyName);
                    break;
                }
            }
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="PropertyName">属性名大小写一致</param>
        /// <returns></returns>
        public object GetPropertyValue(string PropertyName)
        {
            foreach (var property in _properties)
            {
                if (property.Name == PropertyName)
                {
                    return property.GetValue(this, null);
                }
            }
            return null;
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the app that a property has changed.
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                if(_createModelContext == SynchronizationContext.Current)
                {
                    PropertyChanged(this, args);
                }else
                {
                    _createModelContext.Post(NotifyPropertyChangedByOtherThread,args);
                }
                ModelChanging = false;
            }
        }
        private void NotifyPropertyChangedByOtherThread(object state)
        {
            PropertyChanged(this, (PropertyChangedEventArgs)state);
        }
        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        public void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                var args = new PropertyChangingEventArgs(propertyName);
                if (_createModelContext == SynchronizationContext.Current)
                {
                    PropertyChanging(this, args);
                }else
                {
                    _createModelContext.Post(NotifyPropertyChangingByOtherThread, args);
                }
                ModelChanging = true;
            }
        }
        private void NotifyPropertyChangingByOtherThread(object state)
        {
            PropertyChanging(this, (PropertyChangingEventArgs)state);
        }
        #endregion

        #region INotifyModelChanged
        
        public void NotifyModelChanged()
        {
            foreach (var property in _properties)
            {
                NotifyPropertyChanged(property.Name);
            }
        }
        #endregion
    }
}
