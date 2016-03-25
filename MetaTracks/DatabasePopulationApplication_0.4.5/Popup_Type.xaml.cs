using System;
using System.Windows;
using System.Windows.Controls;

namespace dbApp
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup_Type : Window
    {
        public Popup_Type()
        {
            InitializeComponent();
        }

        public string ResponseText
        {
            get { return comboBox.Text; }
        }

        private void submit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
  
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}   
