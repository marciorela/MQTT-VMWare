using MQTT_Testes.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Testes.Services
{
    public class VMService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<VMService> _logger;

        public VMService(IConfiguration config, ILogger<VMService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public List<string> ListAllVMs()
        {
            // LISTA TODOS OS ARQUIVOS DE MÁQUINAS VIRTUAIS (*.vmx) DA PASTA CONFIGURADA
            // AS MÁQUINAS COMEÇANDO COM "TPL-" NÃO DEVEM SER MONITORADAS, POIS NUNCA SÃO EXECUTADAS
            // SERVEM SOMENTE DE BASE PARA OUTRAS MÁQUINAS VIRTUAIS

            var result = new List<string>();
            var path = _config.GetValue<string>("Folders:VMs");
            if (!Directory.Exists(path))
            {
                _logger.LogWarning("Diretório {path} não existe.", path);
            }
            else
            {
                var files = new DirectoryInfo(path).GetFiles("*.vmx", new EnumerationOptions() { RecurseSubdirectories = true });
                foreach (var file in files)
                {
                    //Console.WriteLine($"Nome: {file}");
                    var name = GetVMName(file.FullName);
                    if (name.Length < 4 || name.Length >= 4 && name[..4] != "TPL-")
                    {
                        result.Add(name);
                    }
                }
                _logger.LogInformation("Máquinas virtuais disponíveis em {path}: {total}", path, files.Length.ToString());
            }

            return result;
        }

        private string GetVMName(string file)
        {
            const string displayNameKey = "displayName =";

            file = GetRealVMFileName(file);

            var conteudo = File.ReadAllLines(file);
            var name = conteudo.Where(x => x.StartsWith(displayNameKey)).FirstOrDefault("");
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Replace(displayNameKey, "").Trim().Replace("\"", "");
            }
            else
            {
                name = Path.GetFileNameWithoutExtension(file).ToUpper();
            }

            return name;
        }

        private string GetRealVMFileName(string file)
        {
            var path = _config.GetValue<string>("Folders:VMs");
            return Path.Combine(path, Path.GetFileNameWithoutExtension(file), Path.GetFileName(file));
        }

        public List<VMDetalhe> ListRunningVMs()
        {
            var result = new List<VMDetalhe>();

            var files = GetVMWareCommandResult("list");

            files.RemoveAt(0); // A PRIMEIRA LINHA É O "CABEÇALHO"
            foreach (var file in files)
            {
                if (!string.IsNullOrWhiteSpace(file))
                {
                    //var ip = String.Join(",", GetVMWareCommandResult($"getGuestIPAddress {GetRealVMFileName(file)}"));
                    var ip = String.Join(",", GetVMWareCommandResult($"getGuestIPAddress {file}"));
                    
                    var name = GetVMName(file);
                    result.Add(new VMDetalhe()
                    {
                        Name = name,
                        Ip = ip
                    });
                }
            }

            _logger.LogInformation("Máquinas virtuais executando: {total}.", result.Count);

            return result;
        }

        private List<string> GetCommandResult(string cmd, string args)
        {
            // Start the child process.
            Process p = new();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = cmd;
            p.StartInfo.Arguments = args;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            //Console.WriteLine(output);

            return output.Split("\r\n").ToList();
        }

        private List<string> GetVMWareCommandResult(string args) 
        {
            var output = GetCommandResult("cmd.exe", "/C \"C:\\Program Files (x86)\\VMware\\VMware Workstation\\vmrun\" " + args);

            if (output.Last() == "")
            {
                output.Remove(output.Last());
            }
            
            _logger.LogInformation("Output do comando {args}:\n{output}", args, String.Join(",", output));

            return output;
        }
    }
}
