using MQTT_Testes.Services;
using MR.MQTT.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Testes.Domain
{
    public class Device : DeviceBase
    {

        public Device(string name) : base(name)
        {
            Topic = "homeassistant/binary_sensor/computers/";
            Device_class = "connectivity";
        }

    }
}
