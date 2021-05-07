using PhoneBook;
using System.Linq;
using System.Windows;

namespace TelefonkonyvDavid
{
    /// <summary>
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        cnPhoneBook context;
        public LoginScreen()
        {
            context = new cnPhoneBook();
            InitializeComponent();
        }

        // ha a 'Submit' gombra kattintunk a 'LoginScreen'-es ablakon
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // itt lekérjük a Logins táblából az első sort, ahol szerepel a "tesztfelhasználó"
            // a "tesztfelhasználó" adatai: Username: User1 , Password: 123
            var user = context.Logins.FirstOrDefault();

            // itt ellenőrizzük, hogy amit begépelünk a Username-hez és a Password-höz, azok megegyeznek-e az adatbázisban lévő értékekkel --> ha igen, képernyőt váltunk, azaz
            // "beengedjük" a felhasználót az alkalmazásba
            if(user.Username.Equals(tbUsername.Text) && user.Password.Equals(pbPassword.Password))
            {
                MainWindow dashboard = new MainWindow();
                dashboard.Show();
                this.Close();
            // ha nem egyeznek meg a Username-hez és Password-höz beírt adatok az adatbázisban lévővel, felugró ablakban írunk ki üzenetet erről a "hibáról" a felhasználónak
            } else 
            {
                MessageBox.Show("Username or password (or both) is incorrect!");
            }
        }
    }
}
