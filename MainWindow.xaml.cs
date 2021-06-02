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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PhoneBook;
using Microsoft.EntityFrameworkCore;

namespace TelefonkonyvDavid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        cnPhoneBook cn;
        public MainWindow()
        {
            InitializeComponent();
            cn = new cnPhoneBook();
            InitializeDB();
        }

        // adatbázis inicializálása
        private void InitializeDB()
        {
            cn.Database.EnsureCreated();
            if (cn.People != null) 
            {
                if (!cn.People.Any())
                {
                    SeedDB();
                }
                // ShowData();
            }
        }

        // ezt a metódust nem használjuk fel, csak "tesztelésképp" készült
        private void ShowData()
        {
            var s = "";
            foreach (var p in cn.People.Include(pe => pe.City).Include(pe => pe.Numbers).ToList())
            {
                s += p.Name + ", " + p.Address + ", " + p.City.Zip + ", " + p.City.Name +
                p.Numbers.Aggregate("", (c, a) => c + ", " + a.NumberString) + "\n";
            }
            MessageBox.Show(s);
        }

        // adatbázis feltöltése adatokkal
        private void SeedDB()
        {
            var c1 = new City { Zip = 65433, Name = "Big City" };
            var c2 = new City { Zip = 65876, Name = "Small City" };
            var p1 = new Person { Name = "John Doe", Address = "10 Big Street", City = c1 };
            var p2 = new Person { Name = "Jane Doe", Address = "25 Small Avenue", City = c2 };
            c1.People.Add(p1);
            c2.People.Add(p2);
            var n1 = new Number { NumberString = "+99-99-9999999" };
            var n2 = new Number { NumberString = "+36-76-9998995" };
            var n3 = new Number { NumberString = "+40-88-9979991" };
            p1.Numbers.Add(n1);
            p1.Numbers.Add(n3);
            p2.Numbers.Add(n2);
            p2.Numbers.Add(n3);
            n1.People.Add(p1);
            n2.People.Add(p2);
            n3.People.Add(p1);
            n3.People.Add(p2);
            cn.People.AddRange(new Person[] { p1, p2 });
            cn.Cities.AddRange(new City[] { c1, c2 });
            cn.Numbers.AddRange(new Number[] { n1, n2, n3 });
            cn.SaveChanges();
        }

        // kilépés az alkalmazásból
        private void mi_ExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // városok lekérdezése
        private void mi_CitiesClick(object sender, RoutedEventArgs e)
        {
            dgNumbers.Visibility = Visibility.Collapsed;
            dgAll.Visibility = Visibility.Collapsed;
            grCity.Visibility = Visibility.Collapsed;
            grNumber.Visibility = Visibility.Collapsed;
            grPerson.Visibility = Visibility.Collapsed;
            dgCities.Visibility = Visibility.Visible;
            dgCities.ItemsSource = cn.Cities.ToList();
        }

        // telefonszámok lekérdezése
        private void mi_NumbersClick(object sender, RoutedEventArgs e)
        {
            dgCities.Visibility = Visibility.Collapsed;
            dgAll.Visibility = Visibility.Collapsed;
            grCity.Visibility = Visibility.Collapsed;
            grNumber.Visibility = Visibility.Collapsed;
            grPerson.Visibility = Visibility.Collapsed;
            dgNumbers.Visibility = Visibility.Visible;
            dgNumbers.ItemsSource = cn.Numbers.ToList();
        }

        // személyek lekérdezése az összes adataikkal - ennek okán hoztuk létre külön állományként a PersonExtension osztályt
        private void mi_AllClick(object sender, RoutedEventArgs e)
        {
            dgNumbers.Visibility = Visibility.Collapsed;
            dgCities.Visibility = Visibility.Collapsed;
            grCity.Visibility = Visibility.Collapsed;
            grNumber.Visibility = Visibility.Collapsed;
            grPerson.Visibility = Visibility.Collapsed;
            dgAll.Visibility = Visibility.Visible;
            dgAll.ItemsSource = cn.People.Include(pe => pe.City).Include(pe => pe.Numbers).ToList();
        }

        // innentől az alsó 7 db metódus ehhez kapcsolódik: város módosítása vagy új város felvitele
        
        // ha a 'New/Modify' menüben a 'Cities' -re kattintunk
        private void mi_NMCitiesClick(object sender, RoutedEventArgs e)
        {
            dgNumbers.Visibility = Visibility.Collapsed; 
            dgCities.Visibility = Visibility.Collapsed;
            dgAll.Visibility = Visibility.Collapsed;
            grNumber.Visibility = Visibility.Collapsed;
            grPerson.Visibility = Visibility.Collapsed;
            grCity.Visibility = Visibility.Visible;
            grCity.DataContext = cn.Cities.ToList();
            cbZip.SelectedIndex = 0;

        }

        // ha a ComboBox -ban az irányítószámot módosítjuk
        private void cbZip_SChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = ((ComboBox)sender).SelectedItem as City;
            tbName.Text = c.Name.ToString();
            tbZip.Text = c.Zip.ToString();
        }

        // ha a ComboBox -ban a városnevet módosítjuk
        private void cbName_SChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = ((ComboBox)sender).SelectedItem as City;
            tbName.Text = c.Name.ToString();
            tbZip.Text = c.Zip.ToString();
        }

        // ha a 'Save' gombra kattintunk
        private void btNMSave_Click(object sender, RoutedEventArgs e)
        {
            if (!NameZipValidate(out int zip)) return;
            var c = cbName.SelectedItem as City;
            c.Name = tbName.Text;
            if (zip != c.Zip)
            {
                var nc = new City { Zip = zip, Name = c.Name };
                foreach (var p in c.People)
                {
                    p.City = nc;
                    nc.People.Add(p);
                }
                cn.Cities.Add(nc);
                cn.Cities.Remove(c);
            }
            cn.SaveChanges();
            grCity.Visibility = Visibility.Collapsed;
        }

        // ha a 'Save as new City' gombra kattintunk
        private void btNMSaveNew_Click(object sender, RoutedEventArgs e)
        {
            if (!NameZipValidate(out int zip)) return;
            if (cn.Cities.Any(c => c.Zip == zip)) 
            {
                MessageBox.Show("The Zip code is already in the database!");
                return;
            }
            cn.Cities.Add(new City { Zip = zip, Name = tbName.Text });
            cn.SaveChanges();
            grCity.Visibility = Visibility.Collapsed;
        }

        // ellenőrizzük az irányítószámot
        bool NameZipValidate(out int zip)
        {
            zip = 0;
            if (tbName.Text.Length == 0)
            {
                MessageBox.Show("Please enter name of the city!");
                return false;
            }
            var res = int.TryParse(tbZip.Text, out zip);
            if (!res || (res && zip < 1))
            {
                MessageBox.Show("Please enter a valid Zip code!");
                return false;
            }
            return true;
        }

        // ha a 'Back' gombra kattintunk
        private void btNMBack_Click(object sender, RoutedEventArgs e)
        {
            grCity.Visibility = Visibility.Collapsed;
        }

        // innentől az alsó 5 db metódus ehhez kapcsolódik: telefonszám módosítása vagy új telefonszám felvitele

        // ha a 'New/Modify' menüben a 'Phone Numbers' -re kattintunk
        private void mi_NMPhoneNumbersClick(object sender, RoutedEventArgs e)
        {
            dgNumbers.Visibility = Visibility.Collapsed;
            dgCities.Visibility = Visibility.Collapsed;
            dgAll.Visibility = Visibility.Collapsed;
            grCity.Visibility = Visibility.Collapsed;
            grPerson.Visibility = Visibility.Collapsed;
            grNumber.Visibility = Visibility.Visible;
            grNumber.DataContext = cn.Numbers.Include(n => n.People).ToList();
            cbNumbers.SelectedIndex = 0;
        }

        // ha a 'Save' gombra kattintunk
        private void btNMSaveNumber_Click(object sender, RoutedEventArgs e)
        {
            // akkor megfelelő a telefonszám, ha legalább 3 karakter hosszú
            if(tbCurrentNumber.Text.Length < 3)
            {
                MessageBox.Show("Number is not valid! It should be at least 3 characters.");
                return;
            }
            var n = cbNumbers.SelectedItem as Number;
            n.NumberString = tbCurrentNumber.Text;
            cn.SaveChanges();
            grNumber.Visibility = Visibility.Collapsed;
        }

        // ha a 'Save as new Number' gombra kattintunk
        private void btNMSaveNewNumber_Click(object sender, RoutedEventArgs e)
        {
            // akkor megfelelő a telefonszám, ha legalább 3 karakter hosszú
            if (tbCurrentNumber.Text.Length < 3)
            {
                MessageBox.Show("Number is not valid! It should be at least 3 characters.");
                return;
            }
            if(cn.Numbers.Any(n => n.NumberString == tbCurrentNumber.Text))
            {
                MessageBox.Show("This number is already stored in the database!");
                return; 
            }
            cn.Numbers.Add(new Number { NumberString = tbCurrentNumber.Text });
            cn.SaveChanges();
            grNumber.Visibility = Visibility.Collapsed;
        }

        // ha a 'Back' gombra kattintunk
        private void btNMBackNumber_Click(object sender, RoutedEventArgs e)
        {
            grNumber.Visibility = Visibility.Collapsed;
        }

        // ha a ComboBox -ban a telefonszámot módosítjuk
        private void cbNumbers_SChanged(object sender, SelectionChangedEventArgs e)
        {
            var n = ((ComboBox)sender).SelectedItem as Number;
            tbCurrentNumber.Text = n.NumberString.ToString();
        }

        // innentől az alsó 11 db metódus ehhez kapcsolódik: személy módosítása vagy új személy felvitele

        // ha a 'New/Modify' menüben a 'People' -re kattintunk
        private void mi_NMPeopleClick(object sender, RoutedEventArgs e)
        {
            dgNumbers.Visibility = Visibility.Collapsed;
            dgCities.Visibility = Visibility.Collapsed;
            dgAll.Visibility = Visibility.Collapsed;
            grCity.Visibility = Visibility.Collapsed;
            grNumber.Visibility = Visibility.Collapsed;
            grPerson.Visibility = Visibility.Visible;
            grPerson.DataContext = cn.People.Include(per => per.City).Include(per => per.Numbers).ToList();
            cbPersonName.SelectedIndex = 0;
        }

        // ha a ComboBox -ban a személy nevét módosítjuk
        private void cbPersonName_SChanged(object sender, SelectionChangedEventArgs e)
        {
            var p = ((ComboBox)sender).SelectedItem as Person;
            tbPersonName.Text = p.Name.ToString();
            tbAddress.Text = p.Address.ToString();
            tbCityZip.Text = p.CityZip.ToString();
            tbCityName.Text = p.CityName.ToString();
            tbNumberList.Text = p.NumberList.ToString();
        }

        // ha a ComboBox -ban a személy címét módosítjuk
        private void cbAddress_SChanged(object sender, SelectionChangedEventArgs e)
        {
            var p = ((ComboBox)sender).SelectedItem as Person;
            tbPersonName.Text = p.Name.ToString();
            tbAddress.Text = p.Address.ToString();
            tbCityZip.Text = p.CityZip.ToString();
            tbCityName.Text = p.CityName.ToString();
            tbNumberList.Text = p.NumberList.ToString();
        }

        // ha a ComboBox -ban az irányítószámot módosítjuk
        private void cbPersonCityZip_SChanged(object sender, SelectionChangedEventArgs e)
        {
            var p = ((ComboBox)sender).SelectedItem as Person;
            tbPersonName.Text = p.Name.ToString();
            tbAddress.Text = p.Address.ToString();
            tbCityZip.Text = p.CityZip.ToString();
            tbCityName.Text = p.CityName.ToString();
            tbNumberList.Text = p.NumberList.ToString();
        }

        // ha a ComboBox -ban a városnevet módosítjuk
        private void cbPersonCityName_SChanged(object sender, SelectionChangedEventArgs e)
        {
            var p = ((ComboBox)sender).SelectedItem as Person;
            tbPersonName.Text = p.Name.ToString();
            tbAddress.Text = p.Address.ToString();
            tbCityZip.Text = p.CityZip.ToString();
            tbCityName.Text = p.CityName.ToString();
            tbNumberList.Text = p.NumberList.ToString();
        }

        // ha a ComboBox -ban a telefonszámot módosítjuk
        private void cbPersonNumberList_SChanged(object sender, SelectionChangedEventArgs e)
        {
            var p = ((ComboBox)sender).SelectedItem as Person;
            tbPersonName.Text = p.Name.ToString();
            tbAddress.Text = p.Address.ToString();
            tbCityZip.Text = p.CityZip.ToString();
            tbCityName.Text = p.CityName.ToString();
            tbNumberList.Text = p.NumberList.ToString();
        }

        // ha a 'Save' gombra kattintunk
        private void btNMSavePerson_Click(object sender, RoutedEventArgs e)
        {
            if (!NameAddressValidate()) return;
            if (!NameZipByPersonValidate(out int zip)) return;
            // akkor megfelelő a telefonszám, ha legalább 3 karakter hosszú
            if (tbNumberList.Text.Length < 3)
            {
                MessageBox.Show("Number is not valid! It should be at least 3 characters.");
                return;
            }

            // a Person objektumból kinyerjük a PCity tulajdonságát, hogy majd utána tudjuk frissíteni ezt az adott várost (új város létrehozásával)
            var p = cbPersonName.SelectedItem as Person;
            var c = p.PCity;
            c.Name = tbCityName.Text;
            if (zip != p.CityZip)
            {
                var nc = new City { Zip = zip, Name = p.CityName };
                foreach (var pe in c.People)
                {
                    pe.City = nc;
                    nc.People.Add(pe);
                }
                cn.Cities.Add(nc);
                cn.Cities.Remove(c);
            }
            p.Name = tbPersonName.Text;
            p.Address = tbAddress.Text;
            p.Numbers.Clear();
            string[] n = tbNumberList.Text.Split(",");
            foreach(string nu in n)
            {
                Number num = new Number { NumberString = nu };
                p.Numbers.Add(num);
                num.People.Add(p);
            }
            cn.SaveChanges();
            grPerson.Visibility = Visibility.Collapsed;
        }

        // ha a 'Save as new Person' gombra kattintunk
        private void btNMSaveNewPerson_Click(object sender, RoutedEventArgs e)
        {
            if (!NameAddressValidate()) return;
            if (!NameZipByPersonValidate(out int zip)) return;
            // akkor megfelelő a telefonszám, ha legalább 3 karakter hosszú
            if (tbNumberList.Text.Length < 3)
            {
                MessageBox.Show("Number is not valid! It should be at least 3 characters.");
                return;
            }
            // ha minden adat ugyanaz marad
            if (cbPersonName.Text == tbPersonName.Text && cbAddress.Text == tbAddress.Text && cbPersonNumberList.Text == tbNumberList.Text && cbPersonCityZip.Text == tbCityZip.Text && cbPersonCityName.Text == tbCityName.Text)
            {
               MessageBox.Show("This Person is already stored in the database!");
               return;
            }
            // ha minden oké, elmentjük az új személyt
            var city = new City { Zip = zip, Name = tbCityName.Text };
            var number = new Number { NumberString = tbNumberList.Text };
            var person = new Person { Name = tbPersonName.Text, Address = tbAddress.Text, City = city };
            person.Numbers.Add(number);
            city.People.Add(person);
            number.People.Add(person);
            cn.People.Add(person);
            cn.Cities.Add(city);
            cn.Numbers.Add(number);
            cn.SaveChanges();
            grPerson.Visibility = Visibility.Collapsed;
        }

        // ellenőrizzük a személynevet és címet
        bool NameAddressValidate() 
        {
            // akkor megfelelő a név, ha legalább 1 karakter hosszú
            if (tbPersonName.Text.Length < 1)
            {
                MessageBox.Show("Name is not valid! It should be at least 1 characters.");
                return false;
            }
            // akkor megfelelő a cím, ha legalább 3 karakter hosszú
            if (tbPersonName.Text.Length < 1)
            {
                MessageBox.Show("Address is not valid! It should be at least 3 characters.");
                return false;
            }
            return true;
        }

        // ellenőrizzük az irányítószámot
        bool NameZipByPersonValidate(out int zip)
        {
            zip = 0;
            if (tbCityName.Text.Length == 0)
            {
                MessageBox.Show("Please enter name of the city!");
                return false;
            }
            var res = int.TryParse(tbCityZip.Text, out zip);
            if (!res || (res && zip < 1))
            {
                MessageBox.Show("Please enter a valid Zip code!");
                return false;
            }
            return true;
        }

        // ha a 'Back' gombra kattintunk
        private void btNMBackPerson_Click(object sender, RoutedEventArgs e)
        {
            grPerson.Visibility = Visibility.Collapsed;
        }
    }
}
