using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bratalian2
{
    public class Attack
    {
        public string Name { get; set; }
        public int Power { get; set; }
        public int Accuracy { get; set; }

        public Attack(string name, int power, int accuracy)
        {
            Name = name;
            Power = power;
            Accuracy = accuracy;
        }
    }
}
