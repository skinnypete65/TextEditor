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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FindWindow : Window
    {
        public string SearchText { get; private set; }

        public FindWindow()
        {
            InitializeComponent();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем введенный текст
            SearchText = findTextBox.Text;
            this.DialogResult = true;
            this.Close();
        }
    }

}
