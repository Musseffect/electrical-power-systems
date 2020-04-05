using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectricalPowerSystems
{
    class MvvmTextEditor:TextEditor, INotifyPropertyChanged
    {
        public new string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }
        public static DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                                        typeof(string), typeof(MvvmTextEditor),
            new PropertyMetadata((obj, args) =>
            {
                MvvmTextEditor target = (MvvmTextEditor)obj;
                if (target.BaseText != (string)args.NewValue) 
                    target.Text = (string)args.NewValue;
            })
        );

        internal string BaseText { get { return base.Text; } set { base.Text = value; } }
      
        protected override void OnTextChanged(EventArgs e)
        {
            SetCurrentValue(TextProperty, base.Text);
            base.OnTextChanged(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
