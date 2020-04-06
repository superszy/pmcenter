﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using static pmcenter.Methods.H2Helper;

using static pmcenter.Methods;

namespace pmcenter.CommandLines
{
    internal class UpdateCmdLine : ICmdLine
    {
        public string Prefix => "update";
        public bool ExitAfterExecution => true;
        public async Task<bool> Process()
        {
            Log($"Application version: {Vars.AppVer.ToString()}", "CMD");
            Log("Checking for updates...", "CMD");
            Log("Custom update channels and languages are currently unsupported in command line mode, will use \"master\" channel with English.", "CMD");
            var Latest = await Conf.CheckForUpdatesAsync().ConfigureAwait(false);
            if (Conf.IsNewerVersionAvailable(Latest))
            {
                Log($"Newer version found: {Latest.Latest}, main changes:\n{Latest.UpdateCollection[0].Details}", "CMD");
                Log("Updating...", "CMD");
                Log("Starting update download... (pmcenter_update.zip)", "CMD");
                await DownloadFileAsync(
                    new Uri(Vars.UpdateArchiveURL),
                    Path.Combine(Vars.AppDirectory, "pmcenter_update.zip")
                ).ConfigureAwait(false);
                Log("Download complete. Extracting...", "CMD");
                using (ZipArchive Zip = ZipFile.OpenRead(Path.Combine(Vars.AppDirectory, "pmcenter_update.zip")))
                {
                    foreach (ZipArchiveEntry Entry in Zip.Entries)
                    {
                        Log($"Extracting: {Path.Combine(Vars.AppDirectory, Entry.FullName)}", "CMD");
                        Entry.ExtractToFile(Path.Combine(Vars.AppDirectory, Entry.FullName), true);
                    }
                }
                Log("Starting language file update...", "CMD");
                await DownloadFileAsync(
                    new Uri(Vars.CurrentConf.LangURL),
                    Path.Combine(Vars.AppDirectory, "pmcenter_locale.json")
                ).ConfigureAwait(false);
                Log("Cleaning up temporary files...", "CMD");
                File.Delete(Path.Combine(Vars.AppDirectory, "pmcenter_update.zip"));
                Log("Update complete.", "CMD");
            }
            else
            {
                Log($"No newer version found.\nCurrently installed version: {Vars.AppVer.ToString()}\nThe latest version is: {Latest.Latest}", "CMD");
            }
            return true;
        }
    }
}