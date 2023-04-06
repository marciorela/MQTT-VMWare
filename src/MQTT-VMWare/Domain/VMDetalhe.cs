using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Testes.Domain
{
    public class VMDetalhe
    {
        [JsonIgnore]
        public string? Name { get; set; }
        public string? Ip { get; set; }
    }
}
