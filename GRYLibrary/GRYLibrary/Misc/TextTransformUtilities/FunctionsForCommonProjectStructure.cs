using GRYLibrary.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GRYLibrary.Core.Misc.TextTransformUtilities
{
    public static class FunctionsForCommonProjectStructure
    {
        public static string RunProgramAndGetOutput(string repositoryFolder, string codeUnitName, string program, string workingDirectoryRelative, string arguments)
        {
            string entireWorkingDirectory = Path.Combine(repositoryFolder, codeUnitName, workingDirectoryRelative);
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = program,
                Arguments = arguments,
                WorkingDirectory = entireWorkingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new Process { StartInfo = psi };
            process.Start();
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            int exitCode = process.ExitCode;
            if (exitCode == 0)
            {
                return stdout;
            }
            else
            {
                throw new UnexpectedExitCodeException($"Running \"{entireWorkingDirectory}>{program} {arguments}\" had exitcode {exitCode}. StdOut: \"{stdout}\"; StdErr: \"{stderr}\"");
            }
        }
        public static string GenerateConstants(string repositoryFolder, string codeUnitName, bool addDebugInformation)
        {
            string constantsFolder = Path.Combine(repositoryFolder, codeUnitName, "Other", "Resources", "Constants");
            List<string> contentLines = [];
            bool constantsFolderExists = Directory.Exists(constantsFolder);
            List<string> constantsfiles = [];
            if (constantsFolderExists)
            {
                constantsfiles = [.. Directory.GetFiles(constantsFolder)];
                foreach (string file in constantsfiles)
                {
                    if (file.EndsWith(".constant.xml"))
                    {
                        IDictionary<string, string> constantProperties = GetConstantProperties(file);
                        List<string> constantDocumentationSummaryLines = [];
                        if (constantProperties["documentationsummary"].Contains('\n'))
                        {
                            constantDocumentationSummaryLines.AddRange(constantProperties["documentationsummary"].Split('\n'));
                        }
                        else
                        {
                            constantDocumentationSummaryLines.Add(constantProperties["documentationsummary"]);
                        }
                        contentLines.Add(string.Empty);
                        if (constantDocumentationSummaryLines.Count > 0)
                        {
                            contentLines.Add("/// <summary>");
                            foreach (string line in constantDocumentationSummaryLines)
                            {
                                if (!string.IsNullOrEmpty(line))
                                {
                                    contentLines.Add($"        /// {line}");
                                }
                            }
                            contentLines.Add("/// </summary>");
                        }
                        string constantValue = constantProperties["value"];
                        string constantValueEscaped = constantValue.Replace("\"", "\"\"");
                        string constantName = constantProperties["name"];
                        contentLines.Add($"        internal const string {constantName} = @\"{constantValueEscaped}\";");
                    }
                }
            }
            string constants = string.Join(Environment.NewLine, contentLines);
            if (addDebugInformation)
            {
                string debugInformation = $"Debug-information:" +
                    $"{nameof(repositoryFolder)}: {repositoryFolder}," +
                    $"{nameof(codeUnitName)}: {codeUnitName},\n" +
                    $"{nameof(constantsFolderExists)}: {constantsFolderExists},\n" +
                    $"{nameof(constantsfiles)}: [{string.Join(", ", constantsfiles)}]," +
                    $"{nameof(contentLines)}: [{string.Join(", ", contentLines)}]," +
                    string.Empty;
                constants = $"{constants}{Environment.NewLine}/*{Environment.NewLine}{debugInformation}{Environment.NewLine}*/";
            }
            return constants;
        }
        private static IDictionary<string, string> GetConstantProperties(string file)
        {
            //TODO validate against xsd
            IDictionary<string, string> result = new Dictionary<string, string>();
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            string folder = Path.GetDirectoryName(file);
            doc.Load(file);
            System.Xml.XmlNamespaceManager nsmgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("cps", "https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/tree/main/Conventions/RepositoryStructure/CommonProjectStructure");
            result["name"] = doc.DocumentElement.SelectSingleNode("/cps:constant/cps:name", nsmgr).InnerText;
            result["documentationsummary"] = doc.DocumentElement.SelectSingleNode("/cps:constant/cps:documentationsummary", nsmgr).InnerText.Replace("\r", string.Empty);
            string path = doc.DocumentElement.SelectSingleNode("/cps:constant/cps:path", nsmgr).InnerText;
            string absolutePath = Path.GetFullPath(new Uri(Path.Combine(folder, path)).LocalPath);
            result["value"] = File.ReadAllText(absolutePath, new UTF8Encoding(false));
            return result;
        }
    }
}