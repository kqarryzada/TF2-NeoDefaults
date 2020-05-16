﻿using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeoDefaults_Installer.warning_dialog;

namespace NeoDefaults_Installer {
    /**
     * This class stores helpful methods that are unrelated to UI elements.
     */
    public class Utilities {
        // Stores the location of the folder containing this tool (e.g., the .exe file) on the
        // user's machine.
        private readonly String basePath;

        // Stores the location of the "Team Fortress 2/tf/" folder on the user's machine. If 
        // unknown, this will be null.
        public String tfPath = null;

        // TF-path related parameters. You must use String.Format() and specify a drive name to use these.
        private readonly String[] defaultInstallLocations = {
            @"{0}Program Files (x86)\Steam\SteamApps\common\Team Fortress 2\hl2.exe",
            @"{0}Steam\SteamApps\common\Team Fortress 2\hl2.exe",
            @"{0}SteamLibrary\SteamApps\common\Team Fortress 2\hl2.exe",
        };

        // The name of the folder inside of tf/cfg/ that holds the installer config files.
        private readonly String configFolderName = "NeoDefaults";

        // The name of the config file to be installed.
        private readonly String sourceCfgName = "NeoDefaults-v1.0.0-SNAPSHOT.cfg";

        // The name of the config file once it has been installed on the user's machine.
        private readonly String destCfgName = "neodefaults.cfg";

        // The name of the custom config file, which allows a user to override any values that are
        // set by the NeoDefaults config.
        private readonly String customCfgName = "custom.cfg";

        private static readonly Utilities singleton = new Utilities();

        private readonly Logger log = Logger.GetInstance();

        // An installation of TF2 should take up around 21 GB of space. Thus, if a drive on the
        // filesystem is less than 16 GB, don't bother searching it for a TF2 install.
        private readonly long MIN_DRIVE_SIZE = 16 * ((long) 1 << 30);

        // Return codes for installations. These help report whether an install failed, 
        // succeeded, etc.
        public enum InstallStatus {
            FAIL,
            SUCCESS,
            OPT_OUT
        };

        private Utilities() {
            // When developing, the base filepath is two parent directories above the
            // executable.
            String parentPath = (Main.DEVELOP_MODE) ? @"..\.." : @".";

            String relativeBasePath = AppDomain.CurrentDomain.BaseDirectory;
            relativeBasePath = Path.Combine(relativeBasePath, parentPath);
            basePath = new FileInfo(relativeBasePath).FullName;

            // On startup, try to determine the path to the TF2 installation on the machine.
            SearchForTF2Install();
        }

        public static Utilities GetInstance() {
            return singleton;
        }


        /**
         * Searches for a TF2 installation in the most common locations on the machine.
         *
         * This method obtains all the drives available on the system, then searches for a TF2 path
         * under each drive until either a valid install is found, or until all possibilities have
         * been exhausted.
         */
        private async void SearchForTF2Install() {
            // Obtain the list of drive names on the system.
            DriveInfo[] systemDrives = null;
            await Task.Run(() => {
                try {
                    systemDrives = DriveInfo.GetDrives();
                }
                catch (Exception e) {
                    log.WriteErr("An issue occurred in trying to obtain the list of drives on the machine.", 
                                    e.ToString());

                    // It should be safe to at least check the C: drive before completely bailing
                    DriveInfo[] c = new DriveInfo[1];
                    c[0] = new DriveInfo("C");
                    systemDrives = c;
                }
            });
            // Will hold the location to a TF2 install, if not null.
            String hl2Path = null;

            // Placeholder for paths being checked. Allocated here for use by debug messages.
            String path = null;

            // Search each common installation path for files under all drives on the system.
            await Task.Run(() => {
                try {
                    log.PrintDivider();
                    log.Write("Beginning automatic filepath check..." + Environment.NewLine);
                    foreach (DriveInfo drive in systemDrives) {
                        long size = drive.TotalSize;
                        if (size <= MIN_DRIVE_SIZE) {
                            log.Write("Skipping over the " + drive.Name + " drive since it has size '"
                                      + size + "', which is less than the threshold of " 
                                      + MIN_DRIVE_SIZE + ".");
                            log.Write();
                            continue;
                        }

                        foreach (String _path in defaultInstallLocations) {
                            path = String.Format(_path, drive.Name);

                            log.Write("Checking if the path exists: " + path);
                            if (File.Exists(path)) {
                                hl2Path = path;
                                log.Write();
                                log.Write("Found install at: " + path);

                                // It's a nested loop. Stop being judgemental.
                                goto EndOfLoop;
                            }
                        }
                        log.Write();
                    }

                EndOfLoop:
                    log.PrintDivider();
                    log.Write();
                }
                catch (Exception e) {
                    log.WriteErr("An error occurred when trying to search the system for a TF2 install:",
                                    e.ToString());
                }
            });

            if (hl2Path != null) {
                // Strip "hl2.exe" from the path to obtain the folder path
                var TeamFortressPath = CanonicalizePath(Path.GetDirectoryName(hl2Path));
                tfPath = Path.Combine(TeamFortressPath, "tf");
            }
        }


        /**
         * Returns the canonicalized filepath for 'path', or 'null' if there was a problem.
         */
        public String CanonicalizePath(String path) {
            log.Write("Attempting to canonicalize the path of: " + path);
            String testPath = null;

            try {
                testPath = Path.GetFullPath(path);
            }
            catch (Exception e) {
                path = path ?? "<null>";
                log.WriteErr("Could not obtain the canonical path of '" + path + "'. Aborting.",
                            e.ToString());
            }

            if (testPath != null) {
                log.Write("The path was found to be: " + testPath);
                log.Write();
            }
            return testPath;
        }


        /**
         * Copies a 'sourceFile' to a destination. The filepath of the destination is given by
         * 'destFile', which includes the filename of the file (allowing for a move-and-rename
         * operation to be executed at once). This method will give a few attempts at copying the file
         * over before reporting an issue.
         * 
         * sourceFile: The file to be copied.
         * destFile: The resulting file after the copy is complete.
         * overwrite: If true, overwrite the existing file. If not, simply skip the operation.
         *
         * Throws an Exception if an unexpected error occurs.
         */
        private void CopyFile(String sourceFile, String destFile, bool overwrite) {
            int numRetries = 3;

            // To be certain that any IOException isn't related to an existing file, first check
            // that the resulting file doesn't already exist.
            if (!overwrite && File.Exists(destFile)) {
                var msg = "Attempted to create '" + destFile + "' from '" + sourceFile
                            + "', but the file already exists. Skipping.";
                throw new IOException(msg);
            }

            // Allow a few retry attempts in case of transient issues.
            for (int i = 0; i < numRetries; i++) {
                try {
                    // Attempt a copy. If it is successful, leave the loop. Otherwise, the exception
                    // is caught.
                    File.Copy(sourceFile, destFile, overwrite);
                    break;
                }
                catch (Exception) when (i < numRetries - 1) {
                    // As long as there are remaining attempts allowed, wait and try again.
                    // Otherwise, throw the error.
                    Thread.Sleep(500);
                }
            }
        }


        /**
         * In order for neodefaults.cfg to be run when TF2 is launched, autoexec.cfg must
         * execute the file. This method adds the needed lines to the autoexec file in order
         * to accomplish this. If the autoexec.cfg file does not exist, it will be created.
         *
         * neodefaultsPath: The path to the newly-installed neodefaults.cfg file
         *
         * Throws an Exception if an unexpected error occurs.
         */
        private void AppendLinesToAutoExec(String neodefaultsPath) {
            String autoexec;
            String defaultLocation = Path.Combine(tfPath, @"cfg\", "autoexec.cfg");
            String mastercomfigLocation = Path.Combine(tfPath, @"cfg\user", "autoexec.cfg");

            // Check for a Mastercomfig (https://mastercomfig.com/) install. This is a popular
            // plugin used by many players, and it expects autoexec.cfg to be stored in cfg/user/
            // instead of the usual cfg/ directory. In order to fully support these users, the
            // required execution lines must be added to the correct file.
            String[] filePaths = Directory.GetFiles(Path.Combine(tfPath, "custom"),
                                                    "mastercomfig*preset.vpk",
                                                     SearchOption.TopDirectoryOnly);
            autoexec = (filePaths.Length == 0) ? defaultLocation : mastercomfigLocation;

            // File has been found, append lines.
            StringBuilder sb = new StringBuilder();
            if (File.Exists(autoexec)) {
                sb.Append(Environment.NewLine);
            }
            sb.AppendLine("//--------Added by the NeoDefaults Installer--------//");
            sb.Append("exec ");
            sb.Append(configFolderName);
            sb.Append("/");
            sb.AppendLine(Path.GetFileNameWithoutExtension(neodefaultsPath));
            sb.AppendLine("//--------------------------------------------------//");

            File.AppendAllText(autoexec, sb.ToString());
        }


        /**
         * Attempts to unzip a source file into a target directory. If the install files already
         * exist on the user's machine, the user is asked whether we should overwrite the existing files
         * or skip this component.
         *
         * source:          The location of the zip file that is to be installed.
         * destination:     The path to the expected resulting folder. For example, if a component is
         *                  being installed under 'custom', this would be 
         *                  "<full-path-to-custom>\component-name".
         * name:            The nickname for the component being installed.
         *
         * Throws an Exception if an unexpected error occurs.
         */
        private InstallStatus InstallZip(String source, String destination, String name) {
            // First check if the zip file is already installed. If so, overwrite the existing files
            // with the user's permission.
            if (Directory.Exists(destination)) {
                StringBuilder sb = new StringBuilder();
                sb.Append("An install of the ");
                sb.Append(name);
                sb.Append(" was detected at '");
                sb.Append(destination);
                sb.Append("'. Would you like to continue and overwrite the existing files,");
                sb.Append(" or skip the installation of this component?");

                // Display the prompt and record the user's request.
                var dialog = new WarningDialog();
                var result = dialog.Display(sb.ToString());
                if (result != DialogResult.OK) {
                    log.Write("HUD was already installed, and the user opted out of re-installing.");
                    return InstallStatus.OPT_OUT;
                }
                else {
                    log.Write("'" + destination + "' was found to already exist. Deleting in"
                              + " preparation for re-install.");
                    Directory.Delete(destination, true);
                }
            }
            log.Write("Installing " + name + " from '" + source + "' to '" + destination + "'.");

            // Specify the parent of the destination to avoid a nested folder, e.g.,
            // '<path-to-parent>\component-name\component-name'.
            var destinationParent = Path.Combine(destination, "..");
            ZipFile.ExtractToDirectory(source, destinationParent);
            log.Write(name + " installation complete.");
            return InstallStatus.SUCCESS;
        }


        /**
         * Installs the custom hitsound. This is executed on a background thread to avoid locking 
         * the UI.
         */
        public async Task<InstallStatus> InstallHitsound() {
            return await Task.Run(() => {
                try {
                    String zipFilepath = Path.Combine(basePath, @"resource\NeoDefaults-hitsound.zip");
                    String destination = Path.Combine(tfPath, @"custom\NeoDefaults-hitsound");

                    return InstallZip(zipFilepath, destination, "hitsound");

                }
                catch (Exception e) {
                    log.WriteErr("An error occurred when trying to install the hitsound:", e.ToString());
                    return InstallStatus.FAIL;
                }
            });
        }


        /**
         * Installs idHUD in the custom/ directory. This is executed on a background thread to avoid
         * locking  the UI.
         */
        public async Task<InstallStatus> InstallHUD() {
            return await Task.Run(() => {
                try {
                    String zipFilepath = Path.Combine(basePath, @"resource\idhud-master.zip");
                    String destination = Path.Combine(tfPath, @"custom\idhud-master");

                    return InstallZip(zipFilepath, destination, "HUD");

                }
                catch (Exception e) {
                    log.WriteErr("An error occurred when trying to install the HUD:", e.ToString());
                    return InstallStatus.FAIL;
                }
            });
        }


        /**
         * In order for idhud to work properly, some fonts need to be installed, which are provided
         * in idhud's zip file. This method installs the fonts on the user's machine. If a font is
         * already installed, the existing one is accepted. This is executed on a background thread
         * to avoid locking  the UI.
         */
        public async Task<InstallStatus> InstallHUDFonts() {
            return await Task.Run(() => {
                String fontsPath = "";
                String windowsFontsPath = "";
                try {
                    fontsPath = Path.Combine(tfPath, @"custom\idhud-master\resource\fonts");
                    windowsFontsPath = Path.Combine(Environment.GetEnvironmentVariable("windir"), "Fonts");
                }
                catch (Exception e) {
                    log.WriteErr("Failed to prepare the fonts for installation.", e.ToString());
                    return InstallStatus.FAIL;
                }

                // Copy each file over to the windows fonts directory to install them.
                string[] fontsToInstall;
                try {
                    fontsToInstall = Directory.GetFiles(fontsPath);
                }
                catch (Exception e) {
                    log.WriteErr("Was unable to retrieve the fonts that need to be installed:",
                                    e.ToString());
                    return InstallStatus.FAIL;
                }

                foreach (string font in fontsToInstall) {
                    try {
                        String destFile = Path.Combine(windowsFontsPath, Path.GetFileName(font));
                        if (File.Exists(destFile)) {
                            log.Write("The font, " + destFile + ", is already installed. Skipping.");
                            continue;
                        }

                        log.Write("Installing '" + fontsPath + "' font to '" + destFile + "'.");
                        CopyFile(font, destFile, false);
                    }
                    catch (Exception e) {
                        log.WriteErr("An error occurred when trying to install the fonts for the HUD.",
                                        e.ToString());
                        return InstallStatus.FAIL;
                    }
                }

                log.Write("Font installation complete.");
                return InstallStatus.SUCCESS;
            });
        }


        /**
         * Installs the neodefaults.cfg file. If there is an existing install, it will be
         * overwritten. This is executed on a background thread to avoid locking  the UI.
         */
        public async Task<InstallStatus> InstallConfig() {
            return await Task.Run(() => {
                // First create the tf/cfg/NeoDefaults folder.
                String configFolderPath = "";
                try {
                    configFolderPath = Path.Combine(tfPath, "cfg", configFolderName);
                    Directory.CreateDirectory(configFolderPath);
                }
                catch (Exception e) {
                    log.WriteErr("An error occurred when trying to create the base folder for the config.",
                                    e.ToString());
                    return InstallStatus.FAIL;
                }


                // Create the config file.
                String sourceCfg = "";
                String destCfg = "";
                try {
                    sourceCfg = Path.Combine(basePath, "resource", sourceCfgName);
                    destCfg = Path.Combine(configFolderPath, destCfgName);

                    if (File.Exists(destCfg)) {
                        File.SetAttributes(destCfg, FileAttributes.Normal);
                    }
                    log.Write("Installing config file from '" + sourceCfg + "' to '" + destCfg + "'.");
                    CopyFile(sourceCfg, destCfg, true);

                    // Set file as read-only to encourage using the custom.cfg file instead.
                    File.SetAttributes(destCfg, FileAttributes.ReadOnly);
                }
                catch (Exception e) {
                    var logMsg = "An error occurred when trying to create '" + destCfg + "' from '"
                                    + sourceCfg + "'.";
                    log.WriteErr(logMsg, e.ToString());
                    return InstallStatus.FAIL;
                }


                // Modify autoexec.cfg so that the newly installed config will execute when TF2
                // is launched.
                try {
                    AppendLinesToAutoExec(destCfg);
                }
                catch (Exception e) {
                    log.WriteErr("An error occurred when trying to append to autoexec.cfg.",
                                    e.ToString());

                    // Notify the user that they should modify this manually
                    String message = "Tried to modify 'autoexec.cfg' and failed. To fix this,"
                                     + " try re-running the installation. If the problem"
                                     + " persists, check the FAQ for advice on dealing with errors.";
                    var dialog = new WarningDialog();
                    dialog.Display(message);
                }


                // Create the custom file, if it does not already exist.
                try {
                    String sourceCustom = Path.Combine(basePath, "resource", customCfgName);
                    String destCustom = Path.Combine(configFolderPath, customCfgName);

                    // If there's already a custom file on the machine, then the user already
                    // has settings defined, so they should not be overridden.
                    if (!File.Exists(destCustom)) {
                        CopyFile(sourceCustom, destCustom, false);
                    }

                }
                catch (Exception e) {
                    log.WriteErr("Failed to create the " + customCfgName + " file.", e.ToString());
                    return InstallStatus.FAIL;
                }

                log.Write("Config installation complete.");
                return InstallStatus.SUCCESS;
            });
        }
    }
}