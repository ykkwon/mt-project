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

namespace WindowsApplication_0._4._5
{
    /// <summary>
    /// Interaction logic for TableWindow.xaml
    /// </summary>
    public partial class TableWindow : Window
    {
        
        public TableWindow()
        {
            InitializeComponent();
        }

        public void setTableItems(string[] items)
        {
            listBox.ItemsSource = items;
        }

        public void ReturnAndClose(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var selectedMovie = listBox.SelectedItem.ToString();
                MainWindow.SetSelectedMovie(selectedMovie);
                Close();
            }
        }
    }
}
