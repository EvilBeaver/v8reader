using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace V8Reader.Utils
{
    /// <summary>
    /// Interaction logic for UpdatesWnd.xaml
    /// </summary>
    public partial class UpdatesWnd : Window
    {
        internal UpdatesWnd()
        {
            InitializeComponent();
        }

        internal UpdateLog Updates
        {
            get
            {
                return m_UpdLog;
            }

            set
            {
                m_UpdLog = value;

                if (m_UpdLog != null && m_UpdLog.Count > 0)
                {
                    Version MaxVersion = Version.Parse(m_UpdLog.First<UpdateDefinition>().Version);

                    StringBuilder sb = new StringBuilder();
                    foreach (var UpdateDef in m_UpdLog)
                    {
                        sb.AppendFormat("Версия {0}:\n", UpdateDef.Version);
                        sb.AppendFormat("\t{0}\n\n", UpdateDef.News);

                        Version currVer = Version.Parse(UpdateDef.Version);
                        if (currVer >= MaxVersion)
                        {
                            MaxVersion = currVer;
                            m_MaxVersionUrl = UpdateDef.Url;
                        }

                    }

                    txtData.Text = sb.ToString();
                    btnLoad.IsEnabled = true;
                }
                else
                {
                    txtData.Text = "";
                    btnLoad.IsEnabled = false;
                }
            }
        }

        UpdateLog m_UpdLog;
        string m_MaxVersionUrl;

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            if (m_MaxVersionUrl != String.Empty)
            {
                System.Diagnostics.Process.Start(m_MaxVersionUrl);
            }
        }

    }
}
