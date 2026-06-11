namespace CursorSchemeInverter
{
    class Program
    {
        private const string DefaultFolderName = "Converted";

        static void Main(string[] args)
        {
            Console.Title = "Cursor Scheme Inverter";

            var customOutputDir = (string?)null;
            var targets = new List<string>();
            var showHelp = false;

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-h":
                    case "--help":
                        showHelp = true;
                        break;
                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length)
                        {
                            customOutputDir = args[++i];
                        }
                        else
                        {
                            Logger.Error("CLI Parser", "--output switch requires a directory path.");
                            return;
                        }
                        break;
                    default:
                        targets.Add(args[i]);
                        break;
                }
            }

            if (showHelp)
            {
                PrintHelp();
                return;
            }

            // Fallback check if execution context contains actionable items
            if (targets.Count == 0)
            {
                var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                var hasWork = Directory.EnumerateFiles(currentDir, "*.cur").Any() ||
                              Directory.EnumerateFiles(currentDir, "*.ani").Any();

                if (!hasWork)
                {
                    Logger.Warning("No cursor files (.cur/.ani) found in the local directory.");
                    Logger.BlankLine();
                    PrintHelp();
                    return;
                }
            }

            Logger.Header("Cursor Scheme Inverter");
            Logger.BlankLine();

            try
            {
                if (targets.Count > 0)
                {
                    Logger.Info($"Targeted processing ({targets.Count} inputs detected).");
                    Logger.BlankLine();

                    foreach (var target in targets)
                    {
                        if (Directory.Exists(target))
                        {
                            // If user specified -o, use it. Otherwise, clean and build default "\Converted" inside target folder
                            var destination = customOutputDir
                                ?? Path.Combine(target, DefaultFolderName);
                            if (customOutputDir is null && Directory.Exists(destination))
                            {
                                Directory.Delete(destination, true);
                            }

                            FileProcessor.ProcessDirectory(target, destination);
                        }
                        else if (File.Exists(target))
                        {
                            // Single file target: defaults to a local "\Converted" folder next to that file
                            var destination = customOutputDir
                                ?? Path.Combine(
                                    Path.GetDirectoryName(target) ?? "",
                                    DefaultFolderName);
                            FileProcessor.ProcessSingleFile(target, destination);
                        }
                        else
                        {
                            Logger.Warning($"Path not found, skipped: {target}");
                        }
                    }
                }
                else
                {
                    var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                    Logger.Info($"Default execution path. Scanning local directory:\n{currentDir}");
                    Logger.BlankLine();

                    var destination = customOutputDir 
                        ?? Path.Combine(currentDir, DefaultFolderName);
                    if (customOutputDir is null && Directory.Exists(destination))
                    {
                        Directory.Delete(destination, true);
                    }

                    FileProcessor.ProcessDirectory(currentDir, destination);
                }
            }
            catch (Exception ex)
            {
                Logger.Critical(ex.Message);
            }

            Logger.BlankLine();
            Logger.HorizontalLine();
            Logger.Text("Processing Complete.");
            Logger.Text("Press any key to exit...");
            Console.ReadKey(true);
        }

        static void PrintHelp()
        {
            Logger.Text("""
            Cursor Scheme Inverter CLI Help
            =======================================================================
            Usage Modes:
                1. Double-Click:         Converts all files inside the program's folder.
                2. Drag & Drop:          Drop files or folders directly onto the executable.
                3. Command Line (CLI):   Execute via terminal using advanced options.
            
            Syntax:
                CursorSchemeInverter.exe [targets...] [-o <output_directory>] [-h]
            
            Arguments & Switches:
                [targets...]             Optional list of file paths (.cur/.ani) or folders.
                                         If blank, defaults to current working directory.
                -o, --output <dir>       Redirects output to a custom folder path.
                                         Defaults to creating a '\Converted' subfolder.
                -h, --help               Displays this terminal help dialogue.
            
            Examples:
                CursorSchemeInverter.exe C:\\MyCursorTheme
                CursorSchemeInverter.exe pointer.cur spinner.ani -o D:\\OutputFolder
            =======================================================================
            Press any key to exit...
            """);

            Console.ReadKey(true);
        }
    }
}