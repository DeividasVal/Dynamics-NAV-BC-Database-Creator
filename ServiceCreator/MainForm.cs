using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.ServiceProcess;
using System.Web.Services.Description;

namespace ServiceCreator
{
    public partial class MainForm : Form
    {
        public MainForm()
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
                    bakFilePath.Text = file;
                }
                catch (IOException)
                {
                }
            }
        }

        private void selectButtonLicenseBC_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    string file = openFileDialog.FileName;
                    licensePathBC.Text = file;
                }
                catch (IOException)
                {
                }
            }
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            statusBox.Clear();
            string version = ExtractAndFormat(databaseName.Text);
            string newFolderName = databaseName.Text;
            string baseFolderPathBC = Path.Combine(@"C:\Program Files (x86)\Microsoft Dynamics 365 Business Central", version);
            string baseFolderPathNAV = Path.Combine(@"C:\Program Files (x86)\Microsoft Dynamics NAV", version);
            string baseFolderPath, serviceFolderPath, destinationFolderPath;

            if (NAVRadioButton.Checked)
            {
                statusBox.AppendText("Starting...\n");
                baseFolderPath = baseFolderPathNAV;
                if (Directory.Exists(baseFolderPathNAV))
                {
                    serviceFolderPath = Path.Combine(baseFolderPath, "Service");
                    destinationFolderPath = Path.Combine(baseFolderPath, newFolderName);
                    if (Directory.Exists(serviceFolderPath))
                    {
                        try
                        {
                            statusBox.AppendText("Copying Service folder...\n");
                            DirectoryCopy(serviceFolderPath, destinationFolderPath, true);
                            statusBox.AppendText("Service folder copied and renamed successfully.\n");
                            try
                            {
                                statusBox.AppendText("Changing CustomSettings.config...\n");
                                string content = File.ReadAllText(destinationFolderPath + @"\CustomSettings.config");
                                content = content.Replace("<add key=\"DatabaseName\" value=\"NAV Demo Database\"/>", "<add key=\"DatabaseName\" value=\"" + this.databaseName.Text + "\"/>");
                                content = content.Replace("<add key=\"DatabaseServer\" value=\".\"/>", "<add key=\"DatabaseServer\" value=\"DESKTOP-JQNIOGT\\BCDEMO\"/>");
                                content = content.Replace("<add key=\"ServerInstance\" value=\"InstanceName\"/>", "<add key=\"ServerInstance\" value=\"" + this.databaseName.Text + "\"/>");
                                File.WriteAllText(destinationFolderPath + @"\CustomSettings.config", content);
                                statusBox.AppendText("CustomSettings.config has been changed successfully.\n");
                            }
                            catch (Exception ex)
                            {
                                statusBox.AppendText(ex.ToString());
                            }
                            statusBox.AppendText("Creating service...\n");
                            RunCommand("sc create MicrosoftDynamicsNAVServer$" + this.databaseName.Text + " binpath=\"" + destinationFolderPath + "\\Microsoft.Dynamics.Nav.Server.exe $" + this.databaseName.Text + "\" DisplayName=\"Microsoft Dynamics NAV Server " + this.databaseName.Text + "\" start=auto type=own depend=NetTcpPortSharing");
                            RunCommand("netsh http add urlacl url=http://+:7047/" + this.databaseName.Text + "/ user=\"NT AUTHORITY\\NETWORK SERVICE\" listen=yes delegate=no sddl=\"D:(A;;GX;;;NS)\"");

                            string connectionString = "Server=DESKTOP-JQNIOGT\\BCDEMO;Database=master;Integrated Security=True;";
                            string backupFilePath = bakFilePath.Text;
                            string databaseName = Path.GetFileNameWithoutExtension(backupFilePath);

                            string newDataFilePath = $@"C:\Program Files\Microsoft SQL Server\MSSQL15.BCDEMO\MSSQL\DATA\{this.databaseName.Text}_Data.mdf"; // New data file name
                            string newLogFilePath = $@"C:\Program Files\Microsoft SQL Server\MSSQL15.BCDEMO\MSSQL\DATA\{this.databaseName.Text}_Log.ldf";   // New log file name

                            string restoreCommand = $@"
                        RESTORE DATABASE [{databaseName}] 
                        FROM DISK = '{backupFilePath}' 
                        WITH MOVE N'{databaseName}_Data' TO '{newDataFilePath}', 
                        MOVE N'{databaseName}_Log' TO '{newLogFilePath}', 
                        RECOVERY, REPLACE, STATS = 10";

                            string newDatabaseName = this.databaseName.Text;
                            string renameCommand = $@"
                        ALTER DATABASE [{databaseName}]
                        MODIFY NAME = [{newDatabaseName}]";

                            string takeOfflineCommand = $@"
                        ALTER DATABASE [{newDatabaseName}]
                        SET OFFLINE WITH ROLLBACK IMMEDIATE";

                            string bringOnlineCommand = $@"
                        ALTER DATABASE [{newDatabaseName}]
                        SET ONLINE";

                            string modifyFileCommand = $@"
                        ALTER DATABASE [{newDatabaseName}]
                        MODIFY FILE (NAME = N'{databaseName}_Data', FILENAME = '{newDataFilePath}');
                        ALTER DATABASE [{newDatabaseName}]
                        MODIFY FILE (NAME = N'{databaseName}_Log', FILENAME = '{newLogFilePath}');";

                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                try
                                {
                                    connection.Open();

                                    using (SqlCommand restoreDbCommand = new SqlCommand(restoreCommand, connection))
                                    {
                                        restoreDbCommand.ExecuteNonQuery();
                                        statusBox.AppendText("Database restored successfully.\n");
                                    }

                                    using (SqlCommand renameDbCommand = new SqlCommand(renameCommand, connection))
                                    {
                                        renameDbCommand.ExecuteNonQuery();
                                        statusBox.AppendText($"Database renamed to {newDatabaseName} successfully.\n");
                                    }

                                    using (SqlCommand takeOfflineCommandCmd = new SqlCommand(takeOfflineCommand, connection))
                                    {
                                        takeOfflineCommandCmd.ExecuteNonQuery();
                                        statusBox.AppendText("Database taken offline to rename files.\n");
                                    }

                                    string currentDataFilePath = $@"C:\Program Files\Microsoft SQL Server\MSSQL15.BCDEMO\MSSQL\DATA\{databaseName}_Data.mdf";
                                    string currentLogFilePath = $@"C:\Program Files\Microsoft SQL Server\MSSQL15.BCDEMO\MSSQL\DATA\{databaseName}_Log.ldf";

                                    if (File.Exists(currentDataFilePath))
                                    {
                                        File.Move(currentDataFilePath, newDataFilePath);
                                    }

                                    if (File.Exists(currentLogFilePath))
                                    {
                                        File.Move(currentLogFilePath, newLogFilePath);
                                    }

                                    using (SqlCommand modifyFileCommandCmd = new SqlCommand(modifyFileCommand, connection))
                                    {
                                        modifyFileCommandCmd.ExecuteNonQuery();
                                        statusBox.AppendText("Database file paths updated in SQL Server.\n");
                                    }

                                    using (SqlCommand bringOnlineCommandCmd = new SqlCommand(bringOnlineCommand, connection))
                                    {
                                        bringOnlineCommandCmd.ExecuteNonQuery();
                                        statusBox.AppendText("Database brought back online.\n");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    statusBox.AppendText($"Error: {ex.Message}");
                                    MessageBox.Show($"Error: {ex.Message}");
                                }
                            }
                            string connectionString2 = "Server=DESKTOP-JQNIOGT\\BCDEMO;Database=" + this.databaseName.Text + ";Integrated Security=True;"; // Connect to the restored database
                            string addUsersCommand = @"
                        -- Create NT AUTHORITY\SYSTEM user
                        CREATE USER [NT AUTHORITY\SYSTEM] FOR LOGIN [NT AUTHORITY\SYSTEM];

                        -- Assign role memberships to NT AUTHORITY\SYSTEM
                        ALTER ROLE db_accessadmin ADD MEMBER [NT AUTHORITY\SYSTEM];
                        ALTER ROLE db_owner ADD MEMBER [NT AUTHORITY\SYSTEM];
                        ALTER ROLE db_securityadmin ADD MEMBER [NT AUTHORITY\SYSTEM];

                        -- Assign schema ownership to NT AUTHORITY\SYSTEM
                        ALTER AUTHORIZATION ON SCHEMA::db_owner TO [NT AUTHORITY\SYSTEM];

                        -- Create NT AUTHORITY\NETWORK SERVICE user
                        CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE];

                        -- Assign role memberships to NT AUTHORITY\NETWORK SERVICE
                        ALTER ROLE db_accessadmin ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
                        ALTER ROLE db_owner ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
                        ALTER ROLE db_securityadmin ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
                        ALTER AUTHORIZATION ON SCHEMA::db_securityadmin TO [NT AUTHORITY\NETWORK SERVICE];
                        ALTER AUTHORIZATION ON SCHEMA::db_accessadmin TO [NT AUTHORITY\NETWORK SERVICE];
                        ";

                            using (SqlConnection connection = new SqlConnection(connectionString2))
                            {
                                try
                                {
                                    connection.Open();

                                    using (SqlCommand command = new SqlCommand(addUsersCommand, connection))
                                    {
                                        command.ExecuteNonQuery();
                                        statusBox.AppendText("Users created and permissions assigned successfully.\n");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    statusBox.AppendText($"Error: {ex.Message}\n");
                                }
                            }

                            try
                            {
                                using (ServiceController service = new ServiceController($@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text))
                                {
                                    if (service.Status != ServiceControllerStatus.Running)
                                    {
                                        statusBox.AppendText($"Starting service {$@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text}...\n");

                                        service.Start();
                                        service.WaitForStatus(ServiceControllerStatus.Running);

                                        statusBox.AppendText($"Service {$@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text} started successfully.\n");
                                    }
                                    else
                                    {
                                        statusBox.AppendText($"Service {$@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text} is already running.\n");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                statusBox.AppendText($"Error starting service: {ex.Message}\n");
                            }

                        }
                        catch (Exception ex)
                        {
                            statusBox.AppendText($"Error during folder copy: {ex.Message}\n");
                        }
                    }
                    else
                    {
                        statusBox.AppendText("The 'Service' folder does not exist.\n");
                    }
                }
                else
                {
                    statusBox.AppendText("The " + baseFolderPath + " folder does not exist.\n");
                }
            }
            else if (BCRadioButton.Checked)
            {
                statusBox.AppendText("Starting...\n");
                baseFolderPath = baseFolderPathBC;
                if (Directory.Exists(baseFolderPathBC))
                {
                    serviceFolderPath = Path.Combine(baseFolderPath, "Service");
                    if (Directory.Exists(serviceFolderPath))
                    {
                        statusBox.AppendText("Copying Service folder...\n");
                        try
                        {
                            baseFolderPath = baseFolderPathBC;
                            destinationFolderPath = Path.Combine(baseFolderPath, newFolderName);
                            DirectoryCopy(serviceFolderPath, destinationFolderPath, true);
                            statusBox.AppendText("Service folder copied and renamed successfully.\n");
                            try
                            {
                                string content = File.ReadAllText(destinationFolderPath + @"\CustomSettings.config");
                                statusBox.AppendText("Changing CustomSettings.config...\n");
                                content = content.Replace("<add key=\"DatabaseName\" value=\"NAV Demo Database\"/>", "<add key=\"DatabaseName\" value=\"" + this.databaseName.Text + "\"/>");
                                content = content.Replace("<add key=\"DatabaseServer\" value=\".\"/>", "<add key=\"DatabaseServer\" value=\"DESKTOP-JQNIOGT\\BCDEMO\"/>");
                                content = content.Replace("<add key=\"ServerInstance\" value=\"InstanceName\"/>", "<add key=\"ServerInstance\" value=\"" + this.databaseName.Text + "\"/>");
                                content = content.Replace("<add key=\"DeveloperServicesEnabled\" value=\"false\"/>", "<add key=\"DeveloperServicesEnabled\" value=\"true\"/>");
                                content = content.Replace("<add key=\"PublicWebBaseUrl\" value=\"\" />", "<add key=\"PublicWebBaseUrl\" value=\"http://localhost:8080/" + this.databaseName.Text + "\"/>");
                                File.WriteAllText(destinationFolderPath + @"\CustomSettings.config", content);
                                statusBox.AppendText("CustomSettings.config has been changed successfully.\n");
                                try
                                {
                                    statusBox.AppendText("Creating service...\n");
                                    RunCommand("sc create MicrosoftDynamicsNAVServer$" + this.databaseName.Text + " binpath=\"" + destinationFolderPath + "\\Microsoft.Dynamics.Nav.Server.exe $" + this.databaseName.Text + "\" DisplayName=\"Microsoft Dynamics NAV Server " + this.databaseName.Text + "\" start=auto type=own depend=NetTcpPortSharing");
                                    RunCommand("netsh http add urlacl url=http://+:7047/" + this.databaseName.Text + "/ user=\"NT AUTHORITY\\NETWORK SERVICE\" listen=yes delegate=no sddl=\"D:(A;;GX;;;NS)\"");

                                    string connectionString = "Server=DESKTOP-JQNIOGT\\BCDEMO;Database=master;Integrated Security=True;";
                                    string backupFilePath = bakFilePath.Text;
                                    string databaseName = Path.GetFileNameWithoutExtension(backupFilePath);

                                    string newDataFilePath = $@"C:\Program Files\Microsoft SQL Server\MSSQL15.BCDEMO\MSSQL\DATA\{this.databaseName.Text}_Data.mdf"; // New data file name
                                    string newLogFilePath = $@"C:\Program Files\Microsoft SQL Server\MSSQL15.BCDEMO\MSSQL\DATA\{this.databaseName.Text}_Log.ldf";   // New log file name

                                    string restoreCommand = $@"
                        RESTORE DATABASE [{databaseName}] 
                        FROM DISK = '{backupFilePath}' 
                        WITH MOVE N'{databaseName}_Data' TO '{newDataFilePath}', 
                        MOVE N'{databaseName}_Log' TO '{newLogFilePath}', 
                        RECOVERY, REPLACE, STATS = 10";

                                    string newDatabaseName = this.databaseName.Text;
                                    string renameCommand = $@"
                        ALTER DATABASE [{databaseName}]
                        MODIFY NAME = [{newDatabaseName}]";

                                    string takeOfflineCommand = $@"
                        ALTER DATABASE [{newDatabaseName}]
                        SET OFFLINE WITH ROLLBACK IMMEDIATE";

                                    string bringOnlineCommand = $@"
                        ALTER DATABASE [{newDatabaseName}]
                        SET ONLINE";

                                    string modifyFileCommand = $@"
                        ALTER DATABASE [{newDatabaseName}]
                        MODIFY FILE (NAME = N'{databaseName}_Data', FILENAME = '{newDataFilePath}');
                        ALTER DATABASE [{newDatabaseName}]
                        MODIFY FILE (NAME = N'{databaseName}_Log', FILENAME = '{newLogFilePath}');";

                                    using (SqlConnection connection = new SqlConnection(connectionString))
                                    {
                                        try
                                        {
                                            connection.Open();

                                            using (SqlCommand restoreDbCommand = new SqlCommand(restoreCommand, connection))
                                            {
                                                restoreDbCommand.ExecuteNonQuery();
                                                statusBox.AppendText("Database restored successfully.\n");
                                            }

                                            using (SqlCommand renameDbCommand = new SqlCommand(renameCommand, connection))
                                            {
                                                renameDbCommand.ExecuteNonQuery();
                                                statusBox.AppendText($"Database renamed to {newDatabaseName} successfully.\n");
                                            }

                                            using (SqlCommand takeOfflineCommandCmd = new SqlCommand(takeOfflineCommand, connection))
                                            {
                                                takeOfflineCommandCmd.ExecuteNonQuery();
                                                statusBox.AppendText("Database taken offline to rename files.\n");
                                            }

                                            string currentDataFilePath = $@"C:\Program Files\Microsoft SQL Server\MSSQL15.BCDEMO\MSSQL\DATA\{databaseName}_Data.mdf";
                                            string currentLogFilePath = $@"C:\Program Files\Microsoft SQL Server\MSSQL15.BCDEMO\MSSQL\DATA\{databaseName}_Log.ldf";

                                            if (File.Exists(currentDataFilePath))
                                            {
                                                File.Move(currentDataFilePath, newDataFilePath);
                                            }

                                            if (File.Exists(currentLogFilePath))
                                            {
                                                File.Move(currentLogFilePath, newLogFilePath);
                                            }

                                            using (SqlCommand modifyFileCommandCmd = new SqlCommand(modifyFileCommand, connection))
                                            {
                                                modifyFileCommandCmd.ExecuteNonQuery();
                                                statusBox.AppendText("Database file paths updated in SQL Server.\n");
                                            }

                                            using (SqlCommand bringOnlineCommandCmd = new SqlCommand(bringOnlineCommand, connection))
                                            {
                                                bringOnlineCommandCmd.ExecuteNonQuery();
                                                statusBox.AppendText("Database brought back online.\n");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            statusBox.AppendText($"Error: {ex.Message}");
                                            MessageBox.Show($"Error: {ex.Message}");
                                        }
                                    }
                                    string connectionString2 = "Server=DESKTOP-JQNIOGT\\BCDEMO;Database=" + this.databaseName.Text + ";Integrated Security=True;"; // Connect to the restored database
                                    string addUsersCommand = @"
                        -- Create NT AUTHORITY\SYSTEM user
                        CREATE USER [NT AUTHORITY\SYSTEM] FOR LOGIN [NT AUTHORITY\SYSTEM];

                        -- Assign role memberships to NT AUTHORITY\SYSTEM
                        ALTER ROLE db_accessadmin ADD MEMBER [NT AUTHORITY\SYSTEM];
                        ALTER ROLE db_owner ADD MEMBER [NT AUTHORITY\SYSTEM];
                        ALTER ROLE db_securityadmin ADD MEMBER [NT AUTHORITY\SYSTEM];

                        -- Assign schema ownership to NT AUTHORITY\SYSTEM
                        ALTER AUTHORIZATION ON SCHEMA::db_owner TO [NT AUTHORITY\SYSTEM];

                        -- Create NT AUTHORITY\NETWORK SERVICE user
                        CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE];

                        -- Assign role memberships to NT AUTHORITY\NETWORK SERVICE
                        ALTER ROLE db_accessadmin ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
                        ALTER ROLE db_owner ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
                        ALTER ROLE db_securityadmin ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
                        ALTER AUTHORIZATION ON SCHEMA::db_securityadmin TO [NT AUTHORITY\NETWORK SERVICE];
                        ALTER AUTHORIZATION ON SCHEMA::db_accessadmin TO [NT AUTHORITY\NETWORK SERVICE];
                        ";

                                    using (SqlConnection connection = new SqlConnection(connectionString2))
                                    {
                                        try
                                        {
                                            connection.Open();

                                            using (SqlCommand command = new SqlCommand(addUsersCommand, connection))
                                            {
                                                command.ExecuteNonQuery();
                                                statusBox.AppendText("Users created and permissions assigned successfully.\n");
                                                statusBox.AppendText("Done!\n");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            statusBox.AppendText($"Error: {ex.Message}");
                                        }
                                    }

                                    try
                                    {
                                        using (ServiceController service = new ServiceController($@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text))
                                        {
                                            if (service.Status != ServiceControllerStatus.Running)
                                            {
                                                statusBox.AppendText($"Starting service {$@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text}...\n");

                                                service.Start();
                                                service.WaitForStatus(ServiceControllerStatus.Running);

                                                statusBox.AppendText($"Service {$@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text} started successfully.\n");
                                            }
                                            else
                                            {
                                                statusBox.AppendText($"Service {$@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text} is already running.\n");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        statusBox.AppendText($"Error starting service: {ex.Message}");
                                    }

                                    try
                                    {
                                        ExecutePowerShellScript("Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process");
                                        string publishFolderPath = $@"{baseFolderPathBC}\Web Client\WebPublish";
                                        string modulePathWebClient = $@"{baseFolderPathBC}\Web Client\Modules\NAVWebClientManagement\NAVWebClientManagement.psm1";
                                        string modulePathServiceManagement = $@"{baseFolderPathBC}\Service\Microsoft.Dynamics.Nav.Management.psm1";
                                        string licensePath = licensePathBC.Text;

                                        ExecutePowerShellScript($@"Import-Module '{modulePathWebClient}'");
                                        ExecutePowerShellScript($@"New-NAVWebServerInstance -PublishFolder '{publishFolderPath}' -WebServerInstance '{this.databaseName.Text}' -Server localhost -ServerInstance '{this.databaseName.Text}' -ClientServicesCredentialType Windows -WebSitePort 8080");
                                        ExecutePowerShellScript($@"Import-Module '{modulePathServiceManagement}'");
                                        ExecutePowerShellScript($@"Import-NAVServerLicense {this.databaseName.Text} -LicenseData ([Byte[]]$(Get-Content -Path '{licensePath}' -Encoding Byte)) -Database 2");
                                        statusBox.AppendText("Web Service Instance and License Import done.\n");

                                        using (ServiceController service = new ServiceController($@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text))
                                        {
                                            if (service.Status == ServiceControllerStatus.Running)
                                            {
                                                statusBox.AppendText($"Stopping service {$@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text}...\n");
                                                service.Stop();
                                                service.WaitForStatus(ServiceControllerStatus.Stopped);

                                                statusBox.AppendText($"Service {$@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text} stopped successfully.\n");
                                            }

                                            statusBox.AppendText($"Starting service {$@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text}...");
                                            service.Start();
                                            service.WaitForStatus(ServiceControllerStatus.Running);
                                            statusBox.AppendText($"Service {$@"MicrosoftDynamicsNAVServer$" + this.databaseName.Text} restarted successfully.\n");
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        statusBox.AppendText("Error during PowerShell: " + ex.ToString()+ '\n');
                                    }
                                }
                                catch (Exception ex)
                                {
                                    statusBox.AppendText("Error during DB restoration: " + ex.ToString() + '\n');
                                }
                            }
                            catch (Exception ex)
                            {
                                statusBox.AppendText(ex.ToString()+ '\n');
                            }
                        }
                        catch (Exception ex)
                        {
                            statusBox.AppendText($"Error during folder copy: {ex.Message}\n");
                        }
                    }
                    else
                    {
                        statusBox.AppendText("The 'Service' folder does not exist.\n");
                    }
                }
                else
                {
                    statusBox.AppendText("The " + baseFolderPath + " folder does not exist.\n");
                }
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
                statusBox.AppendText(output);
            }
        }

        private void ExecuteSqlScript(string connectionString, string script)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(script, connection);
                connection.Open();
                command.ExecuteNonQuery();
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
        static string ExtractAndFormat(string input)
        {
            string pattern = @"_(\d{2,4})(CU\d{1,2})?";

            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                string numberPart = match.Groups[1].Value;
                string cuPart = match.Groups[2].Value;

                if (numberPart.Length == 4 && numberPart.EndsWith("0"))
                {
                    numberPart = numberPart.Substring(0, 3);
                }
                else
                {
                    numberPart = int.Parse(numberPart).ToString();
                }

                if (!string.IsNullOrEmpty(cuPart))
                {
                    if (cuPart == "CU00" || cuPart == "CU0")
                    {
                        return numberPart;
                    }
                    return $"{numberPart}.{cuPart}";
                }

                return numberPart;
            }

            return string.Empty;
        }

        public static void ExecutePowerShellScript(string command)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                Process process = new Process
                {
                    StartInfo = processInfo,
                    EnableRaisingEvents = true
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                using (StreamWriter streamWriter = process.StandardInput)
                {
                    if (streamWriter.BaseStream.CanWrite)
                    {
                        streamWriter.WriteLine(command);
                    }
                }

                process.WaitForExit();
                process.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
