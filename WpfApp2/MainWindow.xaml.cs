using Microsoft.VisualBasic.Logging;
using System;
using System.Management;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;


namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private readonly SystemInfoService _systemInfoService = new SystemInfoService();
        private DispatcherTimer _timer = new DispatcherTimer(); // Add this line to create an instance of DispatcherTimer
        public MainWindow()
        {
            LoadExclusions();
            InitializeComponent();
            UpdateTempFolderSize();
            UpdateCacheFolderSize();
            cpuNameTextBlock.Text = _systemInfoService.GetCpuName();
            gpuNameTextBlock.Text = _systemInfoService.GetGpuName();
            ramNameTextBlock.Text = _systemInfoService.GetTotalRamSize();
            motherboardNameTextBlock.Text = _systemInfoService.GetMotherboardInfo();
            hddStorageTextBlock.Text = _systemInfoService.GetHddInfo();
            sddStorageTextBlock.Text = _systemInfoService.GetSddInfo();
            StartOpacityAnimation();

            // Get system information
            string systemName = Environment.MachineName;
            string userName = Environment.UserName;
            string osName = Environment.OSVersion.VersionString;
            string osVersion = Environment.OSVersion.Version.ToString();
            string resolution = SystemParameters.PrimaryScreenWidth + "x" + SystemParameters.PrimaryScreenHeight;
            string hertz = GetPrimaryScreenRefreshRate() + " Hz"; // Use custom method

            // Update TextBlocks with system information
            systemNameTextBlock.Text = systemName;
            userTextBlock.Text = userName;
            osTextBlock.Text = osName;
            osVerTextBlock.Text = osVersion;
            resolutionTextBlock.Text = resolution;
            hertzTextBlock.Text = hertz;

            // Initiera och starta timern
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Call the method once to update the initial state
            UpdateDefenderStatus();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Uppdatera tempmappen
            UpdateTempFolderSize();

            // Uppdatera cache-mappen
            UpdateCacheFolderSize();

            // Uppdatera statusperiodiskt
            UpdateDefenderStatus();

            // Uppdatera Windows Temp-mappen
            UpdateWindowsTempFolderSize();
        }


        private void UpdateDefenderStatus()
        {
            bool isDefenderEnabled = _systemInfoService.IsWindowsDefenderEnabled();
            windowsDefenderTextBlock.Text = isDefenderEnabled ? "Enabled" : "Disabled";
            windowsDefenderTextBlock.Foreground = new SolidColorBrush(isDefenderEnabled ? Colors.Green : Colors.Red);

            bool isSubmissionEnabled = _systemInfoService.IsWindowsDefenderSubmissionEnabled();
            windowsDefenderSubmissionTextBlock.Text = isSubmissionEnabled ? "Enabled" : "Disabled";
            windowsDefenderSubmissionTextBlock.Foreground = new SolidColorBrush(isSubmissionEnabled ? Colors.Green : Colors.Red);
        }

        private void StartOpacityAnimation()
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(3),
                AutoReverse = true,  // Enables reversing the animation
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTargetName(opacityAnimation, "iconHome");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);

            iconHome.BeginStoryboard(storyboard);
        }

        // Custom method to get primary screen refresh rate
        private int GetPrimaryScreenRefreshRate()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null && source.CompositionTarget != null)
            {
                return (int)(source.CompositionTarget.TransformToDevice.M22 * 60);
            }
            return 60; // Default value
        }

        //AppData Temp Folder
        private void UpdateTempFolderSize()
        {
            string tempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
            long totalSizeBytes = CalculateFolderSize(tempFolderPath);
            double totalSizeMB = totalSizeBytes / (1024.0 * 1024.0);
            tempFolder.Text = $"AppData Temp Folder Size: {totalSizeMB:F2} MB";
        }

        private long CalculateFolderSize(string folderPath)
        {
            long totalSize = 0;

            try
            {
                foreach (string file in Directory.GetFiles(folderPath))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        totalSize += fileInfo.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Unable to access this file, continue to the next one
                    }
                }

                foreach (string subfolder in Directory.GetDirectories(folderPath))
                {
                    try
                    {
                        totalSize += CalculateFolderSize(subfolder);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Unable to access this subfolder, continue to the next one
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Unable to access this folder, skip its contents
            }

            return totalSize;
        }

        private void Temp_Clear(object sender, RoutedEventArgs e)
        {
            ClearTempFiles();
            ClearWindowsUpdateCache();

            MessageBox.Show("Temp files and Windows Update Cache cleared successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearTempFiles()
        {
            string tempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");

            try
            {
                ClearFolder(tempFolderPath);
            }
            catch (Exception ex)
            {
                // Handle the error if necessary
            }
        }

        //Windows Cache
        private void UpdateCacheFolderSize()
        {
            string cacheFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution\\Download");
            long totalSizeBytes = CalculateFolderSize(cacheFolderPath);
            double totalSizeMB = totalSizeBytes / (1024.0 * 1024.0);
            cacheFolder.Text = $"Cache Folder Size: {totalSizeMB:F2} MB";
        }

        private long CalculateCacheFolderSize(string folderPath)
        {
            long totalSize = 0;

            try
            {
                foreach (string file in Directory.GetFiles(folderPath))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        totalSize += fileInfo.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Unable to access this file, continue to the next one
                    }
                }

                foreach (string subfolder in Directory.GetDirectories(folderPath))
                {
                    try
                    {
                        totalSize += CalculateCacheFolderSize(subfolder);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Unable to access this subfolder, continue to the next one
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Unable to access this folder, skip its contents
            }

            return totalSize;
        }

        private void Cache_Clear(object sender, RoutedEventArgs e)
        {
            ClearWindowsUpdateCache();
            UpdateCacheFolderSize(); // Uppdatera storleksinformation efter rensning
            MessageBox.Show("Windows Update cache cleared successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void ClearWindowsUpdateCache()
        {
            string windowsUpdateCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");

            try
            {
                ClearFolder(windowsUpdateCachePath);
            }
            catch (Exception ex)
            {
                // Handle the error if necessary
            }
        }

        private void ClearFolder(string folderPath)
        {
            foreach (string file in Directory.GetFiles(folderPath))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    // Log or handle the error if necessary
                }
            }

            foreach (string subfolder in Directory.GetDirectories(folderPath))
            {
                try
                {
                    Directory.Delete(subfolder, true);
                }
                catch (Exception ex)
                {
                    // Log or handle the error if necessary
                }
            }
        }

        // Windows Temp-mapp
        private void UpdateWindowsTempFolderSize()
        {
            string windowsTempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
            long totalSizeBytes = CalculateFolderSize(windowsTempFolderPath);
            double totalSizeMB = totalSizeBytes / (1024.0 * 1024.0);
            WindowsTempFolder.Text = $"Windows Temp Folder Size: {totalSizeMB:F2} MB";
        }

        private void Windows_Temp(object sender, RoutedEventArgs e)
        {
            ClearWindowsTempFiles();
            UpdateWindowsTempFolderSize(); // Uppdatera storleksinformation efter rensning
            MessageBox.Show("Windows Temp files cleared successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearWindowsTempFiles()
        {
            string windowsTempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
            try
            {
                ClearFolder(windowsTempFolderPath);
            }
            catch (Exception ex)
            {
                // Hantera felet om det behövs
            }
        }
        private void LoadExclusions()
        {
            string command = "Get-MpPreference | select -ExpandProperty ExclusionPath";
            string exclusions = RunCommand(command);
            exclusionView.Text = string.IsNullOrEmpty(exclusions) ? "No exclusions found" : exclusions;
        }

        private string RunCommand(string command)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                return process.StandardOutput.ReadToEnd();
            }
        }
    }
}
