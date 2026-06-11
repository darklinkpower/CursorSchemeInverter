using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CursorSchemeInverter
{
    public static class FileProcessor
    {
        private const string Suffix = "_Inverted";

        public static void ProcessSingleFile(string filePath, string targetFolder)
        {
            var ext = Path.GetExtension(filePath).ToLower();
            if (ext != ".cur" && ext != ".ani")
            {
                Logger.Warning($"Unsupported extension format skipped: {Path.GetFileName(filePath)}");
                return;
            }

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var outputFilePath = Path.Combine(targetFolder, Path.GetFileNameWithoutExtension(filePath) + Suffix + ext);
            ExecuteConversion(filePath, outputFilePath, ext);
        }

        public static void ProcessDirectory(string sourceDir, string targetFolder)
        {
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            Logger.Info($"Processing Directory: {sourceDir} -> Destination: {targetFolder}");

            // 1. Process Static Cursors
            foreach (var file in Directory.GetFiles(sourceDir, "*.cur"))
            {
                var outFile = Path.Combine(targetFolder, Path.GetFileNameWithoutExtension(file) + Suffix + ".cur");
                ExecuteConversion(file, outFile, ".cur");
            }

            // 2. Process Animated Cursors
            foreach (var file in Directory.GetFiles(sourceDir, "*.ani"))
            {
                var outFile = Path.Combine(targetFolder, Path.GetFileNameWithoutExtension(file) + Suffix + ".ani");
                ExecuteConversion(file, outFile, ".ani");
            }

            // 3. Duplicate Non-Cursor Support Assets
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var ext = Path.GetExtension(file).ToLower();
                if (ext != ".cur" && ext != ".ani" && ext != ".inf" && ext != ".exe")
                {
                    var targetCopy = Path.Combine(targetFolder, Path.GetFileName(file));
                    File.Copy(file, targetCopy, true);
                }
            }

            // 4. Update Installation Manifests
            foreach (var file in Directory.GetFiles(sourceDir, "*.inf"))
            {
                var targetInfPath = Path.Combine(targetFolder, Path.GetFileName(file));
                UpdateInfFile(file, targetInfPath);
            }
        }

        private static void ExecuteConversion(string inputPath, string outputPath, string extension)
        {
            var filename = Path.GetFileName(inputPath);
            try
            {
                var fileBytes = File.ReadAllBytes(inputPath);
                var processedBytes = extension == ".cur"
                    ? InverterEngine.ConvertCurBytes(fileBytes)
                    : InverterEngine.ConvertAniBytes(fileBytes);

                File.WriteAllBytes(outputPath, processedBytes);
                Logger.Success($"{filename} -> {Path.GetFileName(outputPath)}");
            }
            catch (Exception ex)
            {
                Logger.Error($"File IO: {filename}", ex.Message);
            }
        }

        private static void UpdateInfFile(string inputPath, string outputPath)
        {
            var filename = Path.GetFileName(inputPath);
            try
            {
                var infText = File.ReadAllText(inputPath);

                infText = Regex.Replace(infText, @"(?<name>[^""\\]+)\.cur(?="")", "${name}_Inverted.cur", RegexOptions.IgnoreCase);
                infText = Regex.Replace(infText, @"(?<name>[^""\\]+)\.ani(?="")", "${name}_Inverted.ani", RegexOptions.IgnoreCase);
                infText = Regex.Replace(infText, @"(?m)^(CUR_DIR\s*=\s*""[^""]+)", "$1 - Inverted", RegexOptions.IgnoreCase);
                infText = Regex.Replace(infText, @"(?m)^(SCHEME_NAME\s*=\s*""[^""]+)", "$1 - Inverted", RegexOptions.IgnoreCase);

                File.WriteAllText(outputPath, infText, Encoding.ASCII);
                Logger.Info($"Updated Installer Manifest: {filename}");
            }
            catch (Exception ex)
            {
                Logger.Error($"INF Update Failed: {filename}", ex.Message);
            }
        }
    }
}