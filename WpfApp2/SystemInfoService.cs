using System;
using Microsoft.Win32;
using System.Management;
using System.Windows.Threading;

namespace WpfApp2
{
    class SystemInfoService
    {
        public bool GetWindowsDefenderStatus()
        {
            // Use your existing IsWindowsDefenderEnabled() logic here
            return IsWindowsDefenderEnabled();
        }

        public string GetCpuName()
        {
            string cpuName = "Unknown";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    cpuName = obj["Name"].ToString();
                    break; // Retrieve only the first CPU
                }
            }
            return cpuName;
        }

        public string GetGpuName()
        {
            string gpuName = "Unknown";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    gpuName = obj["Caption"].ToString();
                    break; // Retrieve only the first GPU
                }
            }
            return gpuName;
        }

        public string GetTotalRamSize()
        {
            long totalRamBytes = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    totalRamBytes = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                    break;
                }
            }
            double totalRamGB = totalRamBytes / (1024 * 1024 * 1024.0); // Convert to GB
            return $"{totalRamGB:F2} GB";
        }

        public string GetMotherboardInfo()
        {
            string motherboardInfo = "Unknown";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    motherboardInfo = obj["Product"].ToString();
                    break; // Retrieve only the first motherboard
                }
            }
            return motherboardInfo;
        }

        public string GetHddInfo()
        {
            string hddInfo = "Unknown";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    double sizeBytes = Convert.ToDouble(obj["Size"]);
                    double sizeGB = sizeBytes / (1024 * 1024 * 1024.0); // Convert to GB
                    hddInfo = $"{sizeGB:F2} GB";
                    break; // Retrieve only the first HDD
                }
            }
            return hddInfo;
        }


        public string GetSddInfo()
        {
            string sddInfo = "Unknown";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE MediaType='SSD'"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    sddInfo = $"{obj["Size"]} bytes";
                    break; // Retrieve only the first SSD
                }
            }
            return sddInfo;
        }
        public bool IsWindowsDefenderEnabled()
        {
            const string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection";
            const string valueName = "DisableRealtimeMonitoring";

            int value = (int)Registry.GetValue(keyPath, valueName, -1);
            return value != 1;
        }

        public bool IsWindowsDefenderSubmissionEnabled()
        {
            const string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Spynet";
            const string valueName = "SubmitSamplesConsent";

            int value = (int)Registry.GetValue(keyPath, valueName, -1);
            return value == 1;
        }
    }
}