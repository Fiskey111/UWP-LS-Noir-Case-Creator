using CaseCreator.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CaseCreator.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LogScreen : Page
    {
        public LogScreen()
        {
            this.InitializeComponent();

            UpdateLogs();
        }
        public void UpdateLogs()
        {
            string text = string.Empty;
            
            foreach (Log line in Logger.Logs.ToList())
            {
                if (debugCheck.IsChecked == false && line.IsDebug)
                {
                    continue;
                }
                text = text + line.LogData + Environment.NewLine;
            }

            LogBox.Text = text;

            ScrollView.ChangeView(0.0f, Double.MaxValue, 1.0f);
            ScrollView.UpdateLayout();
        }

        private void DebugCheck_Click(object sender, RoutedEventArgs e)
        {
            if (sender != debugCheck) return;

            Logger.AddLog($"DebugCheck_Click {debugCheck.IsChecked}", true);

            UpdateLogs();
        }
    }
}
