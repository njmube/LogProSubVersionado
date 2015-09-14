using System.ComponentModel;

namespace WpfFront.Common.Query
{
    public class PredefinedSettingsItem : INotifyPropertyChanged
    {
        public string DisplayName
        {
            get
            {
                return m_displayName;
            }
            set
            {
                if (value == m_displayName)
                    return;

                m_displayName = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("DisplayName"));
            }
        }

        public string XmlUri
        {
            get
            {
                return m_xmlUri;
            }
            set
            {
                if (value == m_xmlUri)
                    return;

                m_xmlUri = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("XmlUri"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        private string m_displayName;

        private string m_xmlUri;
    }
}