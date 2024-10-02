using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ServiceCreator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    string file = openFileDialog.FileName;
                    textBox2.Text = file;
                }
                catch (IOException)
                {
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string baseFolderPath = @"C:\Program Files (x86)\Microsoft Dynamics 365 Business Central\" + textBox1.Text;
            string serviceFolderPath = Path.Combine(baseFolderPath, "Service");

            string newFolderName = textBox3.Text;
            string destinationFolderPath = Path.Combine(baseFolderPath, newFolderName);

            if (Directory.Exists(baseFolderPath))
            {
                if (Directory.Exists(serviceFolderPath))
                {
                    try
                    {
                        DirectoryCopy(serviceFolderPath, destinationFolderPath, true);
                        MessageBox.Show("Service folder copied and renamed successfully.");
                        string content = File.ReadAllText(destinationFolderPath + @"\CustomSettings.config");
                        content = content.Replace("<add key=\"DatabaseName\" value=\"NAV Demo Database\"/>", "<add key=\"DatabaseName\" value=\""+textBox3.Text+"\"/>");
                        content = content.Replace("<add key=\"DatabaseServer\" value=\".\"/>", "<add key=\"DatabaseServer\" value=\"SQL\\SQL2019\"/>");
                        content = content.Replace("<add key=\"ServerInstance\" value=\"InstanceName\"/>", "<add key=\"DatabaseName\" value=\""+textBox3.Text+"\"/>");
                        File.WriteAllText(destinationFolderPath + @"\CustomSettings.config", content);
                        RunCommand("sc create MicrosoftDynamicsNAVServer$"+ textBox3.Text + " binpath=\""+ destinationFolderPath + "\\Microsoft.Dynamics.Nav.Server.exe $"+textBox3.Text+"\" DisplayName=\"Microsoft Dynamics NAV Server "+textBox3.Text+"\" start=auto type=own depend=NetTcpPortSharing");
                        RunCommand("netsh http add urlacl url=http://+:7047/"+ textBox3.Text + "/ user=\"NT AUTHORITY\\NETWORK SERVICE\" listen=yes delegate=no sddl=\"D:(A;;GX;;;NS)\"");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error during folder copy: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("The 'Service' folder does not exist.");
                }
            }
            else
            {
                MessageBox.Show("The " + baseFolderPath + " folder does not exist.");
            }
        }
        private void RunCommand(string command)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                MessageBox.Show(output);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
