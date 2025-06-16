using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipMod.Model
{
    internal class DigitalPort
    {
        public ushort IDPort;
        public bool value;
        public DateTime TimeFroze;


        public DigitalPort(ushort iDPort, bool value, DateTime timeFroze)
        {
            IDPort = iDPort;
            this.value = value;
            TimeFroze = timeFroze;
        }
    }
}
