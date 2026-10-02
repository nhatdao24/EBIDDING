
using Hangfire;
using SMO.Core.Entities;
using SMO.Repository.Implement.AD;
using SMO.Repository.Implement.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Management;
using SMO.Models;

namespace SMO.Service.AD
{
    

    public class SystemConfigService : GenericService<T_AD_SYSTEM_CONFIG, SystemConfigRepo>
    {
        public SystemConfigService() : base()
        {
            
        }

        public void GetConfig()
        {
            this.GetAll();
            this.ObjDetail = this.ObjList[0];
            var connection = this.ObjList[0].Connection;

            //  if (this.ObjList == null || this.ObjList.Count == 0)
            //     {
            //         throw new InvalidOperationException("ObjList is empty or not initialized.");
            //     }

            // this.ObjDetail = this.ObjList[0];
            // var connection = this.ObjList[0].Connection;

            //if (this.ObjList.Count == 0 && this.State)
            //{
            //    this.ObjDetail = new T_AD_SYSTEM_CONFIG();
            //    this.ObjDetail.PKID = Guid.NewGuid().ToString();
            //    this.Create();
            //}
            //else
            //{
            //    this.ObjDetail = this.ObjList[0];
            //    var connection = this.ObjList[0].Connection;
            //}
        }

       

        public SystemInfoModel GetSystemInfo()
        {
            try
            {
                var model = new SystemInfoModel
                {
                    CpuInfo = GetCpuInfo(),
                    MemoryInfo = GetMemoryInfo(),
                    DiskInfo = GetDiskInfo()
                };
                return model;
            }
            catch (Exception ex)
            {
                return new SystemInfoModel();
            }
            
        }


        private string GetCpuInfo()
        {
            var searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor");
            StringBuilder cpuInfo = new StringBuilder();
            foreach (ManagementObject obj in searcher.Get())
            {
                cpuInfo.AppendLine($"CPU: {obj["LoadPercentage"]}%/100%");
            }
            return cpuInfo.ToString();
        }

        private string GetMemoryInfo()
        {
            var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            StringBuilder memoryInfo = new StringBuilder();
            foreach (ManagementObject obj in searcher.Get())
            {
                ulong totalMemory = (ulong)obj["TotalVisibleMemorySize"] / (1024 * 1024); // Giải phóng sang GB
                ulong availableMemory = (ulong)obj["FreePhysicalMemory"] / (1024 * 1024); // Giải phóng sang GB
                ulong usedMemory = totalMemory - availableMemory;

                memoryInfo.AppendLine($"RAM: {usedMemory}G/{totalMemory}G ({(usedMemory * 100.0 / totalMemory):F1}%)");
            }
            return memoryInfo.ToString();
        }

        private List<string> GetDiskInfo()
        {
            List<string> diskInfo = new List<string>();

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                long usedSpace = drive.TotalSize - drive.TotalFreeSpace;
                long totalSizeGB = drive.TotalSize / (1024 * 1024 * 1024);
                long usedSpaceGB = usedSpace / (1024 * 1024 * 1024);

                using (var diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total"))
                {
                    diskCounter.NextValue();
                    System.Threading.Thread.Sleep(1000); // Chờ 1 giây
                    float activeTime = diskCounter.NextValue();

                    diskInfo.Add($"Drive {drive.Name}:");
                    diskInfo.Add($"  Used Space: {usedSpaceGB}G/{totalSizeGB}G ({(usedSpace * 100.0 / drive.TotalSize):F1}%)");
                    diskInfo.Add($"  Active Time: {activeTime:F1}%");
                }
            }
            return diskInfo;
        }


        public override void Update()
        {
            try
            {
                base.Update();
                if (this.State)
                {
                    SAPINT.SAPDestitination.Init(this.ObjDetail.SAP_HOST, this.ObjDetail.SAP_CLIENT, this.ObjDetail.SAP_NUMBER);
                }
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
            
        }
    }
}
