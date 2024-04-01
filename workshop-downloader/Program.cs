using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace workshop_downloader
{
    internal class Program
    {

        // Success\..*?\"(.*?)\"
        static string steamcmd_dir = Path.Combine(Environment.CurrentDirectory, "steamcmd\\");
        static string steamcmd_file = Path.Combine(steamcmd_dir, "steamcmd.exe");
        static List<string> commands = new List<string>();
        static void Main(string[] args)
        {
            bool should_output_debug = args.Length > 1 && args?[1] == "-debug";

            string itemid = args[0];
            string appid = ExtractAppId(itemid);

            if (should_output_debug)
            {
                Console.WriteLine("Item id: {0}\r\nApp id: {1}", itemid, appid);
            }

            Process p = new Process();
            p.StartInfo.FileName = steamcmd_file;
            p.StartInfo.Arguments = string.Format("+login anonymous +workshop_download_item {0} {1} +quit", appid, itemid);
            p.StartInfo.WorkingDirectory = steamcmd_dir;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();

            int debug_peek = 0;
            while ((debug_peek = p.StandardOutput.Peek()) > 0)
            {
                string cmd = p.StandardOutput.ReadLine();
                commands.Add(cmd);

                if (should_output_debug)
                    Console.WriteLine("Debug: {0}", cmd);

            }
            p.Kill();

            Regex my_pattern = new Regex(@"Success\..*?\""(.*?)\""");
            foreach (string line in commands)
            {
                Match m = my_pattern.Match(line);
                if (m.Success)
                {
                    string workshopfilename = Path.Combine(m.Groups[1].Value, "publish.gma");
                    Console.WriteLine(workshopfilename);
                }
            }

            //Console.ReadLine();

        }

        static string ExtractAppId(string workshopId)
        {
            Regex my_pattern = new Regex(@"https:\/\/steamcommunity.com\/app\/(\d+)");
            HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp("https://steamcommunity.com/sharedfiles/filedetails/?id=" + workshopId);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream))
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();
                    Match m = my_pattern.Match(line);
                    if (m.Success)
                    {
                        return m.Groups[1].Value;
                    }
                }
            }

            return null;
        }
    }
}
