
using System.Collections;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace GHIElectronics.DUE {
    public partial class DUEController {
        const int MAX_IO = 21;
        const int MAX_IO_ANALOG = 11;

        SerialInterface serialPort = default!;

        public AnalogController Analog { get; internal set; }
        public DigitalController Digital { get; internal set; }
        public I2cController I2c { get; internal set; }
        public ServoMotoController ServoMoto { get; internal set; }
        public SpiController Spi { get; internal set; }

        public FrequencyController Frequency { get; internal set; }

        public InfraredController Infrared { get; internal set; }

        public NeoController Neo { get; internal set; }

        public PwmController PWM { get; internal set; }

        public SystemController System { get; internal set; }

        public UartController Uart { get; internal set; }
        public ButtonController Button { get; internal set; }
        public DistanceSensorController DistanceSensor { get; internal set; }
        public SoundController Sound { get; internal set; }
        public DisplayController Display { get; internal set; }

        public TouchController Touch { get; internal set; }

        public LedController Led { get; internal set; }

        public ScriptController Script { get; internal set; }

        public string Version { get; internal set; } = string.Empty;

        public DUEController(string comPort) {
            if (comPort == null)
                throw new ArgumentNullException("Invalid comport");

            this.Connect(comPort);

            this.Analog = new AnalogController(this.serialPort);
            this.Digital = new DigitalController(this.serialPort);
            this.I2c = new I2cController(this.serialPort);
            this.ServoMoto = new ServoMotoController(this.serialPort);
            this.Frequency = new FrequencyController(this.serialPort);
            this.Spi = new SpiController(this.serialPort);
            this.Infrared = new InfraredController(this.serialPort);
            this.Neo = new NeoController(this.serialPort);
            this.PWM = new PwmController(this.serialPort);
            this.System = new SystemController(this.serialPort);
            this.Uart = new UartController(this.serialPort);
            this.Button = new ButtonController(this.serialPort);
            this.DistanceSensor = new DistanceSensorController(this.serialPort);
            this.Sound = new SoundController(this.serialPort);
            this.Display = new DisplayController(this.serialPort);
            this.Touch = new TouchController(this.serialPort);
            this.Led = new LedController(this.serialPort);
            this.Script = new ScriptController(this.serialPort);
        }

        private static IEnumerable<RegistryKey> GetSubKeys(RegistryKey key) {
            foreach (var keyName in key.GetSubKeyNames())
                using (var subKey = key.OpenSubKey(keyName))
                    yield return subKey;
        }
        static public string GetConnectionPort() {
            var vid = "VID_1B9F";
            var pid = "PID_F300";


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                var serialports = new ArrayList();

                using (var enumUsbKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB")) {
                    if (enumUsbKey != null) {
                        foreach (var devBaseKey in GetSubKeys(enumUsbKey)) {
                            foreach (var devFnKey in GetSubKeys(devBaseKey)) {
                                using (var devParamsKey = devFnKey.OpenSubKey("Device Parameters")) {
                                    var portName = (string)devParamsKey?.GetValue("PortName");
                                    if (portName != null) {

                                        if (devFnKey.ToString().IndexOf(vid.ToUpper()) >= 0
                                            || devFnKey.ToString().IndexOf(vid.ToLower()) >= 0) {

                                            if (devFnKey.ToString().IndexOf(pid.ToUpper()) >= 0
                                           || devFnKey.ToString().IndexOf(pid.ToLower()) >= 0) {
                                                serialports.Add(portName);
                                            }


                                        }


                                    }
                                }
                            }
                        }
                    }
                }

                try {
                    var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
                    if (key != null) {
                        foreach (var name in key.GetValueNames()) {
                            if (name != null) {
                                var val = (string)key.GetValue(name);

                                if (val != null && val != string.Empty) {

                                    foreach (var p in serialports) {
                                        if (p.ToString().CompareTo(val) == 0)
                                            return val;
                                    }
                                }
                            }
                        }
                    }
                }

                catch {
                }
            }
            else {
                var ports = SerialPort.GetPortNames();

                if (ports != null) {
                    foreach (var port in ports) {
                        if (port.Contains("usbmodemDUE_SC131"))
                            return "/dev/tty.usbmodemDUE_SC131";
                        else if (port.Contains("usbmodemDUE_SC0071"))
                            return  "/dev/tty.usbmodemDUE_SC0071";
                    }
                }                
            }

            return string.Empty;
        }
        public void Connect(string comPort) {
            this.serialPort = new SerialInterface(comPort);
            this.serialPort.Connect();

            this.Version = this.serialPort.GetVersion().Substring(0);
        }


        public void Disconnect() => this.serialPort.Disconnect();

    }
}