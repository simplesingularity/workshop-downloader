using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace workshop_downloader_gui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            timer1.Start();

            Regex my_pattern = new Regex(@"(\d+)");
            Match m = my_pattern.Match(textBox1.Text);
            if(m.Success )
            {
                ThreadPool.QueueUserWorkItem((object o ) => {
#if (DEBUG)
                    string working_path = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\workshop-downloader\\bin\\debug\\");
#else
   string working_path = Environment.CurrentDirectory;
#endif

                    string workshop_downloader_path = Path.Combine(working_path, "workshop-downloader.exe");
                    Process p = new Process();
                    p.StartInfo.FileName = workshop_downloader_path;
                    p.StartInfo.WorkingDirectory = working_path;
                    p.StartInfo.Arguments = m.Groups[1].Value;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.Start();
                    while (p.StandardOutput.Peek() > 0)
                    {
                        string line = p.StandardOutput.ReadLine();
                        InvokeSuccessWindow(line);
                    }
                }, null);

            }
            timer1.Stop();
        }
        delegate  void  DInvokeSuccessWindow(string hello);
        private void InvokeSuccessWindow(string hello  )
        {
            if(this.InvokeRequired )
            {
                this.Invoke(new DInvokeSuccessWindow(InvokeSuccessWindow), hello);
            } else
            {
 
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = Environment.CurrentDirectory;
                sfd.Filter = "GMA *.gma|*.gma";

                MessageBox.Show("Successfully downloaded file!", "Steam Workshop Downloader", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(hello, sfd.FileName, true);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = ((progressBar1.Value + 1) % 100);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://steamcommunity.com/profiles/76561198043640068/");
        }
    }
}
