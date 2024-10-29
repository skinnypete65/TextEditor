using System;
using System.Collections.Generic;
using System.Linq;
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

namespace TextEditor
{
    /// <summary>
    /// Interaction logic for ReplaceWindow.xaml
    /// </summary>
    public partial class ReplaceWindow : Window
    {
        private readonly MainWindow _mainWindow;

        public ReplaceWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            string findText = FindTextBox.Text;
            string replaceText = ReplaceTextBox.Text;

            _mainWindow.ReplaceText(findText, replaceText);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
