using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Test.Annotations;

namespace Test
{
    class TagInfoModal:INotifyPropertyChanged
    {
        private string Id;
        private string _TagName;
        private string _ServerID;
        private string _Value;
        private string _Quality;
        private string _Timesnamp;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ID
        {
            get => Id;
            set
            {
                Id = value;
                OnPropertyChanged();
            }
        }

        public string TagName
        {
            get => _TagName;
            set { _TagName = value;
                OnPropertyChanged();
            }
        }

        public string ServerID
        {
            get => _ServerID;
            set { _ServerID = value; OnPropertyChanged(); }
        }

        public string Value
        {
            get => _Value;
            set
            {
                _Value = value;
                OnPropertyChanged();
            }
        }

        public string Quality
        {
            get => _Quality;
            set
            {
                _Quality = value;
                OnPropertyChanged();
            }
        }

        public string Timesnamp
        {
            get => _Timesnamp;
            set
            {
                _Timesnamp = value;
                OnPropertyChanged();
            }
        }
    }
}
