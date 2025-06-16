using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipMod.Model
{
    internal class AnalogPort
    {
        public ushort IDPort;
        public float value;
        public DateTime TimeFroze;
        public AnalogPort(ushort iDPort, float value, DateTime timeFroze)
        {
            IDPort = iDPort;
            this.value = value;
            TimeFroze = timeFroze;
        }
    }
}
