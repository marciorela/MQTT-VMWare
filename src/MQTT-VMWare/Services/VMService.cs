using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Testes.Services
{
    public class VMService
    {
        private readonly IConfiguration _config;

        public VMService(IConfiguration config)
        {
            _config = config;
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
                Console.WriteLine($"Diretório {path} não existe.");
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
            }

            return result;
        }

        private static string GetVMName(string file)
        {
            const string displayNameKey = "displayName =";

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

        public List<string> ListRunningVMs()
        {
            var result = new List<string>();

            // Start the child process.
            Process p = new();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/C \"C:\\Program Files (x86)\\VMware\\VMware Workstation\\vmrun\" list";
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            //Console.WriteLine(output);

            var files = output.Split("\r\n").ToList();
            files.RemoveAt(0); // A PRIMEIRA LINHA É O "CABEÇALHO"
            foreach (var file in files)
            {
                if (!string.IsNullOrWhiteSpace(file))
                {
                    var name = GetVMName(file);
                    result.Add(name);
                }
            }

            return result;
        }
    }
}
