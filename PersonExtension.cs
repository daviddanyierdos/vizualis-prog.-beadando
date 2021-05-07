using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoneBook;
using Microsoft.EntityFrameworkCore;

namespace PhoneBook
{
    public partial class Person
    {
        public string CityName => City.Name;
        public int CityZip => City.Zip;
        public string NumberList => Numbers.Aggregate("", (c, a) => c + (c.Length > 0 ? ", " : " ") + a.NumberString);

        public City PCity => City;
    }
}
