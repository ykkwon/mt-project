using System;
using System.Windows;

namespace WindowsApplication_0._4._5
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        public Popup()
        {
            InitializeComponent();
        }

        public string ResponseText
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        private void submit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void textBox_TextChanged(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}   
