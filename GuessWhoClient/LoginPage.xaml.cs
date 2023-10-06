using GuessWhoClient.Utils;
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

namespace GuessWhoClient
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnLoginClick(object sender, RoutedEventArgs e)
        {
            string email = tbEmail.Text;
            string password = Authentication.HashPassword(pbPassword.Password);

            GameServices.UserServiceClient userServiceClient = new GameServices.UserServiceClient();
            GameServices.Profile profile = userServiceClient.Login(email,password);

            if(profile != null)
            {
                MessageBox.Show("Bienvenido/a " + profile.FullName + " a " + "Adivina quién");
            }
            else
            {
                MessageBox.Show("No se encontró usuario");
            }
        }

        private void BtnSignUpClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
