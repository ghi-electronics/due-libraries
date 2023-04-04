using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHIElectronics.DUE {
    public partial class DUEController {

        public class AnalogController {

            SerialInterface serialPort;

            public AnalogController(SerialInterface serialPort) => this.serialPort = serialPort;

            public int Read(int pin) {
                if (pin < 0 || pin >= MAX_IO_ANALOG)
                    throw new ArgumentOutOfRangeException("Invalid pin.");


                var cmd = string.Format("print(aread({0}))", pin.ToString());

                this.serialPort.WriteCommand(cmd);

                var respone = this.serialPort.ReadRespone();

                if (respone.success) {                   
                    try {
                        var value = int.Parse(respone.respone);

                        return value;
                    }
                    catch { }


                }

                return -1;
            }
        }
    }
}