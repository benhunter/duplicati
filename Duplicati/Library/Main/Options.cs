#region Disclaimer / License
// Copyright (C) 2009, Kenneth Skovhede
// http://www.hexad.dk, opensource@hexad.dk
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// 
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace Duplicati.Library.Main
{
    public class Options
    {
        Dictionary<string, string> m_options;

        public Options(Dictionary<string, string> options)
        {
            m_options = options;
        }

        public IList<Backend.ICommandLineArgument> SupportedCommands
        {
            get
            {
                return new List<Backend.ICommandLineArgument>(new Backend.ICommandLineArgument[] {
                    new Backend.CommandLineArgument("full", Backend.CommandLineArgument.ArgumentType.Boolean, "A flag used to force full backups", "When this flag is specified, Duplicati will make a full backup of all files, and ignore any incremental data."),
                    new Backend.CommandLineArgument("volsize", Backend.CommandLineArgument.ArgumentType.Size, "A size string that limits the size of the volumes", "This option can change the default volume size. Changing the size can be usefull if the backend has a limit on the size of each individual file", "5mb"),
                    new Backend.CommandLineArgument("totalsize", Backend.CommandLineArgument.ArgumentType.Size, "The number of bytes generated by each backup run", "This option can place an upper limit on the total size of each backup. Note that if this flag is specified the backup may not contain all files, even for a full backup."),
                    new Backend.CommandLineArgument("auto-cleanup", Backend.CommandLineArgument.ArgumentType.Boolean, "A flag indiciating that Duplicati should remove unused files", "If a backup is interrupted there will likely be partial files present on the backend. Using this flag, Duplicati will automatically remove such files when encountered."),
                    new Backend.CommandLineArgument("full-if-older-than", Backend.CommandLineArgument.ArgumentType.Timespan, "The max duration between full backups", "If the last full backup is older than the duration supplied here, Duplicati will make a full backup, otherwise an incremental"),

                    new Backend.CommandLineArgument("signature-control-files", Backend.CommandLineArgument.ArgumentType.Path, "A list of control files to embed in the backups", "Supply a list of files seperated with semicolons, that will be added to each backup. The Duplicati GUI program uses this to store the setup database with each backup."),
                    new Backend.CommandLineArgument("signature-cache-path", Backend.CommandLineArgument.ArgumentType.Path, "A path to temporary storage", "If this path is supplied, Duplicati will store all signature files here, so re-downloads can be avoided."),
                    new Backend.CommandLineArgument("skip-file-hash-checks", Backend.CommandLineArgument.ArgumentType.Boolean, "Set this flag to skip hash checks", "If the hash for the volume does not match, Duplicati will refuse to use the backup. Supply this flag to allow Duplicati to proceed anyway."),
                    new Backend.CommandLineArgument("file-to-restore", Backend.CommandLineArgument.ArgumentType.String, "A list of files to restore", "By default, duplicati will restore all files in the backup. Use this option to restore only a subset of the files"),
                    new Backend.CommandLineArgument("restore-time", Backend.CommandLineArgument.ArgumentType.String, "The time to restore files", "By default, Duplicati will restore files from the most recent backup, use this option to select another item. You may use relative times, like \"-2M\" for a backup from two months ago.", "now"),

                    new Backend.CommandLineArgument("disable-filetime-check", Backend.CommandLineArgument.ArgumentType.String, "Disable checks based on file time", "The operating system keeps track of the last time a file was written. Using this information, Duplicati can quickly determine if the file has been modified. If some application deliberately modifies this information, Duplicati won't work correctly unless this flag is set."),
                    new Backend.CommandLineArgument("force", Backend.CommandLineArgument.ArgumentType.String, "Force the removal of files", "When deleting old files, Duplicati will only write out what files are supposed to be deleted. Specify the \"force\" option to actually remove them."),
            });

            }
        }

        /// <summary>
        /// A value indicating if the backup is a full backup
        /// </summary>
        public bool Full { get { return GetBool("full"); } }

        /// <summary>
        /// Gets the size of each volume in bytes
        /// </summary>
        public long VolumeSize
        {
            get
            {
                string volsize = "5mb";
                if (m_options.ContainsKey("volsize"))
                    volsize = m_options["volsize"];

                return Core.Sizeparser.ParseSize(volsize, "mb");
            }
        }

        /// <summary>
        /// Gets the total size in bytes allowed for a single backup run
        /// </summary>
        public long MaxSize
        {
            get
            {
                if (!m_options.ContainsKey("totalsize") || string.IsNullOrEmpty(m_options["totalsize"]))
                    return long.MaxValue;
                else
                    return Core.Sizeparser.ParseSize(m_options["totalsize"]);
            }
        }

        /// <summary>
        /// Gets the time at which a full backup should be performed
        /// </summary>
        /// <param name="offsettime">The time the last full backup was created</param>
        /// <returns>The time at which a full backup should be performed</returns>
        public DateTime FullIfOlderThan(DateTime offsettime)
        {
            if (!m_options.ContainsKey("full-if-older-than") || string.IsNullOrEmpty(m_options["full-if-older-than"]))
                return DateTime.Now.AddYears(1); //We assume that the check will occur in less than one year :)
            else
                return Core.Timeparser.ParseTimeInterval(m_options["full-if-older-than"], offsettime);
        }

        /// <summary>
        /// A value indicating if orphan files are deleted automatically
        /// </summary>
        public bool AutoCleanup { get { return GetBool("auto-cleanup"); } }

        /// <summary>
        /// Gets a list of files to add to the signature volumes
        /// </summary>
        public string SignatureControlFiles
        {
            get
            {
                if (!m_options.ContainsKey("signature-control-files") || string.IsNullOrEmpty(m_options["signature-control-files"]))
                    return null;
                else
                    return m_options["signature-control-files"];
            }
        }

        /// <summary>
        /// Gets a list of files to add to the signature volumes
        /// </summary>
        public string SignatureCachePath
        {
            get
            {
                if (!m_options.ContainsKey("signature-cache-path") || string.IsNullOrEmpty(m_options["signature-cache-path"]))
                    return null;
                else
                    return m_options["signature-cache-path"];
            }
        }

        /// <summary>
        /// A value indicating if file hash checks are skipped
        /// </summary>
        public bool SkipFileHashChecks { get { return GetBool("skip-file-hash-checks"); } }

        /// <summary>
        /// Gets a list of files to restore
        /// </summary>
        public string FileToRestore
        {
            get
            {
                if (!m_options.ContainsKey("file-to-restore") || string.IsNullOrEmpty(m_options["file-to-restore"]))
                    return null;
                else
                    return m_options["file-to-restore"];
            }
        }

        /// <summary>
        /// Gets the backup that should be restored
        /// </summary>
        public DateTime RestoreTime
        {
            get
            {
                if (!m_options.ContainsKey("restore-time") || string.IsNullOrEmpty(m_options["restore-time"]))
                    return DateTime.Now.AddYears(1); //We assume that the check will occur in less than one year :)
                else
                    return Core.Timeparser.ParseTimeInterval(m_options["restore-time"], DateTime.Now);
            }
        }

        /// <summary>
        /// A value indicating if file time checks are skipped
        /// </summary>
        public bool DisableFiletimeCheck { get { return GetBool("disable-filetime-check"); } }

        /// <summary>
        /// A value indicating if file deletes are forced
        /// </summary>
        public bool Force { get { return GetBool("force"); } }

        private bool GetBool(string name)
        {
            if (!m_options.ContainsKey(name))
                return false;
            else
            {
                string v = m_options[name];
                if (string.IsNullOrEmpty(v))
                    return true;
                else
                {
                    v = v.ToLower().Trim();
                    if (v == "false" || v == "no" || v == "off")
                        return false;
                    else
                        return true;
                }

            }
        }

    }
}
