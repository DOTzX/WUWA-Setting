using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WUWA_Setting {
    internal class Utilities {
        private static string registryPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache";
        private static string patternKey = @"(Client-Win64-Shipping\.exe\.FriendlyAppName|launcher\.exe\.FriendlyAppName)$";
        private static string expectedValue = "Wuthering Waves";
        private static string processName = "Client-Win64-Shipping";
        private static string updateUrl = "https://raw.githubusercontent.com/DOTzX/WUWA-Setting/refs/heads/master/Properties/AssemblyInfo.cs";

        #region Core (Private)
        private static async Task<string> GetLatestVersion () {
            try {
                using (HttpClient client = new HttpClient()) {
                    string fileContent = await client.GetStringAsync(updateUrl);
                    Match match = Regex.Match(fileContent, @"\[assembly:\s*AssemblyFileVersion\(""(\d+\.\d+\.\d+\.\d+)""\)\]");
                    return match.Success ? match.Groups[1].Value : null;
                }
            } catch {
                return null;
            }
        }
        private static bool IsNewerVersion (string currentVersion, string latestVersion) {
            if (Version.TryParse(currentVersion, out Version curVer) && Version.TryParse(latestVersion, out Version newVer)) {
                return newVer > curVer;
            }
            return false;
        }
        private static string GetGPUName () {
            try {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController")) {
                    foreach (ManagementObject mo in searcher.Get()) {
                        string name = mo["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name)) {
                            return name;
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error detecting GPU: {ex.Message}");
            }
            return string.Empty;
        }
        #endregion

        #region Core (Internal)
        internal static bool IsGpuSupportedByOfficial () {
            string gpuName = GetGPUName().ToLower();
            return ( gpuName.Contains("rtx 40") || gpuName.Contains("rtx 50") );
        }
        internal static string GetWuwaDir () {
            List<string> detectedPath = new List<string>();

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath)) {
                if (key != null) {
                    foreach (string valueName in key.GetValueNames()) {
                        if (Regex.IsMatch(valueName, patternKey, RegexOptions.IgnoreCase)) {
                            string value = key.GetValue(valueName).ToString();
                            if (value.ToLower() == expectedValue.ToLower()) {
                                string path = valueName.Replace(".FriendlyAppName", "");
                                if (Directory.Exists(Path.GetDirectoryName(path))) {
                                    Console.WriteLine($"DETECTED: {path}");
                                    detectedPath.Add(path);
                                }
                            }
                        }
                    }
                }
            }

            foreach (string path in detectedPath) {
                if (path.ToLower().EndsWith("client-win64-shipping.exe")) {
                    return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), "..", "..", ".."));
                } else if (path.ToLower().EndsWith("launcher.exe")) {
                    return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), "Wuthering Waves Game"));
                }
            }

            return string.Empty;
        }
        internal static bool IsGameRunning () {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }
        internal static bool? ReadLocalStorage (string WUWA_DIR, ref SettingDataList settingDataList, ref LocalStorageRepository localStorageRepository) {
            string gameDbPath = Path.Combine(WUWA_DIR, "Client", "Saved", "LocalStorage", "LocalStorage.db");

            string folderTmpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".tmp");
            string localDbPath = Path.Combine(folderTmpPath, Path.GetFileName(gameDbPath));

            try {
                if (File.Exists(gameDbPath)) {
                    if (!Directory.Exists(folderTmpPath)) Directory.CreateDirectory(folderTmpPath);
                    File.Copy(gameDbPath, localDbPath, true);

                    localStorageRepository = new LocalStorageRepository(localDbPath);

                    List<LocalStorageEntry> allEntries = localStorageRepository.GetAll();
                    foreach (LocalStorageEntry ety in allEntries) {
                        List<SettingStructure> _btn = settingDataList.Where(stc => stc.Name == ety.Key).ToList();
                        if (_btn.Count > 0) {
                            if (int.TryParse(ety.Value, out int valueInt)) {
                                Console.WriteLine($"DB_READ: {ety.Key} = {ety.Value} | int => {valueInt}");
                                _btn[0].Value = valueInt;
                                _btn[0].OldValue = valueInt;
                            } else if (float.TryParse(ety.Value, out float valueFloat)) {
                                Console.WriteLine($"DB_READ: {ety.Key} = {ety.Value} | float => {valueFloat}");
                                _btn[0].Value = valueFloat;
                                _btn[0].OldValue = valueFloat;
                            } else {
                                Console.WriteLine($"DB_READ: {ety.Key} = {ety.Value}");
                                _btn[0].Value = ety.Value;
                                _btn[0].OldValue = ety.Value;
                            }
                        }
                    }
                }

                return File.Exists(gameDbPath);
            } catch (Exception ex) {
                Console.WriteLine($"Error reading settings: {ex.Message}");
            }

            return null;
        }
        internal static void ApplyLocalStorage (string WUWA_DIR) {
            string gameDbPath = Path.Combine(WUWA_DIR, "Client", "Saved", "LocalStorage", "LocalStorage.db");

            string folderTmpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".tmp");
            string localDbPath = Path.Combine(folderTmpPath, Path.GetFileName(gameDbPath));

            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string folderBackupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".backup");
            string backupDbPath = Path.Combine(folderBackupPath, Path.GetFileName(gameDbPath) + "-" + unixTimestamp.ToString());

            if (Directory.Exists(Path.GetDirectoryName(gameDbPath)) && File.Exists(localDbPath)) {
                if (!Directory.Exists(folderBackupPath)) Directory.CreateDirectory(folderBackupPath);
                if (File.Exists(gameDbPath)) File.Copy(gameDbPath, backupDbPath, true);

                try {
                    File.Copy(localDbPath, gameDbPath, true);
                } catch (Exception e) {
                    MessageBox.Show($"Unable to apply local storage: {e.Message}");
                }
            }
        }
        #endregion

        #region Non Core
        internal static string GetFileVersion () {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        }
        internal static async Task CheckForUpdate () {
            string currentVersion = GetFileVersion();
            string latestVersion = await GetLatestVersion();

            if (latestVersion == null) {
                MessageBox.Show("Failed to check for updates.", "Update Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (IsNewerVersion(currentVersion, latestVersion)) {
                MessageBox.Show($"A new version {latestVersion} is available! (Current: {currentVersion})",
                                "Update Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else {
                MessageBox.Show("You are using the latest version.", "Update Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        internal static void OpenUrl (string url) {
            try {
                Process.Start(new ProcessStartInfo {
                    FileName = url,
                    UseShellExecute = true
                });
            } catch (Exception ex) {
                MessageBox.Show($"Failed to open repository: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
