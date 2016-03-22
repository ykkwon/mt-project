using System;
using System.Windows;

namespace dbApp
{
    /// <summary>
    /// Interaction logic for Confirmation.xaml
    /// </summary>
    public partial class Confirmation : Window
    {
        public Confirmation()
        {
            InitializeComponent();
        }

        private void YesButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NoButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}   
