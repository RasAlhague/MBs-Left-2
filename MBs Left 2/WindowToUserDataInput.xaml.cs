using System.Windows;

namespace MBs_Left_2
{
    /// <summary>
    /// Interaction logic for WindowToUserDataInput.xaml
    /// </summary>
    public partial class WindowToUserDataInput : Window
    {
        public WindowToUserDataInput()
        {
            InitializeComponent();
        }

        
        private bool CheckUserPhone(string userPhone)
        {
            const int etalonUserPhoneLength = 9;
            if (userPhone.Length == etalonUserPhoneLength && !userPhone.Contains(" "))
            {
                int parseResult;
                if (int.TryParse(userPhone, out parseResult))
                {
                    return true;
                }
                MessageBox.Show(Properties.Resources.IncorrectPhoneMessage);
                return false;
            }
            MessageBox.Show(Properties.Resources.IncorrectPhoneMessage);
            return false;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            bool checkResult = CheckUserPhone(TextBoxForUserPhone.Text);
            if (checkResult)
            {
                Properties.Settings.Default.UserPass = TextBoxForUserPass.Password;
                Properties.Settings.Default.UserPhoneNamber = TextBoxForUserPhone.Text;
                //MessageBox.Show(TextBoxForUserPass.Password);

                Properties.Settings.Default.Save();

                Close();
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
