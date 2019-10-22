using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HideezClient.ViewModels
{
    /// <summary>
    /// Interaction logic for DeleteCredentialsPrompt.xaml
    /// </summary>
    public partial class DeleteCredentialsPrompt : MetroWindow
    {
        public DeleteCredentialsPrompt()
        {
            InitializeComponent();
            this.Title = $"Hideez Client ({Assembly.GetExecutingAssembly().GetName().Version.ToString()})";
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
