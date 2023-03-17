using MQTT_Testes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Testes.Domain
{
    public class Device
    {
        private bool _state = false;
        private readonly string _topic = "homeassistant/binary_sensor/computers/";
        //private readonly string _topic = "homeassistant/switch/computers/";
        private readonly MQTTService _mqtt;

        public string Unique_id { get; } = "";
        public string Name { get; } = "";
        public string Device_class { get; } = "connectivity";
        //public string Device_class { get; } = "switch";
        public string State_topic { get; } = "";

        //public string Unit_of_measurement { get; } = "";

        public bool State()
        {
            return _state;
        }

        public string Topic() 
        {
            return _topic;
        }

        public Device(string name, MQTTService mqtt)
        {
            Unique_id = name.ToLower().Replace(" ", "_");

            _mqtt = mqtt;
            _topic += Unique_id;

            Name = name;
            State_topic = $"{Topic()}/state";
        }

        public void SetState(bool state)
        {
            _state = state;

            SendMQTTState();
        }

        private void SendMQTTState()
        {
            Task.Run(async () => await _mqtt.Register(this)).Wait();
            Task.Run(async () => await _mqtt.SendState(this)).Wait();
        }
    }
}
