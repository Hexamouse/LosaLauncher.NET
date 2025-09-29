using System.Windows;
using System.Windows.Input;

namespace GameLauncher
{
    public partial class CustomMessageBox : Window
    {
        private CustomMessageBox(string title, string message)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            TitleText.Text = title.ToUpper();
            MessageText.Text = message;
        }

        public static void Show(string title, string message)
        {
            var msgBox = new CustomMessageBox(title, message);
            msgBox.ShowDialog();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
