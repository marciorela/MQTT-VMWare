# MQTT-VMWare
Serviço do windows que avisa ao Home Assistant através do MQTT, quais as máquinas virtuais estão ativas.
O serviço utiliza o programa vmrun.exe da pasta VMWare.  
Cada máquina virtual é criada como uma entidade de domínio input_boolean dentro do Home Assistant.

## Configuração
appsettings.json:

```json
{
  "Folders": {
    "VMs": "<pasta onde estão as VMs>",
  },
  "Service": {
    "Delay": "<tempo de espera entre as chamadas (default 30000)>"
  }
  "MQTT": {
    "Server": "<IP do servidor MQTT>",
    "Username": "<Usuario do MQTT>",
    "Password": "<Senha do MQTT>"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```
