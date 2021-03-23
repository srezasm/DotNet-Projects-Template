using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace DotNetTemplatesCreator.Utils
{
    public class CreateProjectsUtility
    {
        private readonly string _basePath;

        public CreateProjectsUtility()
        {
            var currentPath = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal))
               .Split("\\");

            // remove last empty index of array
            currentPath = currentPath.Take(currentPath.Length - 1).ToArray();

            _basePath = string.Join('\\', currentPath.Take(currentPath.Length - 1).Append("Templates").ToArray());
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        private void RemoveCurrentProjects()
        {
            var baseDirs = new DirectoryInfo(_basePath).GetDirectories();
            foreach (var directory in baseDirs)
            {
                Directory.Delete(directory.FullName, true);
            }

            var baseFiles = new DirectoryInfo(_basePath).GetFiles();
            foreach (var file in baseFiles)
            {
                File.Delete(file.FullName);
            }
        }

        private void CreateProject(string projectName, string createProjectCommandName)
        {
            try
            {
                using var powerShell = PowerShell.Create();
                var projName = projectName;
                var projPath = $"{_basePath}\\{projName}";

                powerShell.AddScript($"cd \"{_basePath}\"");
                powerShell.AddScript($"mkdir \"{projName}\"");
                powerShell.AddScript($"cd \"{projPath}\"");
                powerShell.AddScript("dotnet new sln");
                powerShell.AddScript($"dotnet new {createProjectCommandName}");
                powerShell.AddScript($"dotnet sln \"{projName}.sln\" add \"{projName}.csproj\"");
                powerShell.Invoke();

                Console.WriteLine("{0} created", projName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Execute()
        {
            RemoveCurrentProjects();
            //first way
            //CreateProject("Console Application", "console");
            //CreateProject("Class library", "classlib");
            //CreateProject("WPF Application", "wpf");
            //CreateProject("WPF Class library", "wpflib");
            //CreateProject("WPF Custom Control Library", "wpfcustomcontrollib");
            //CreateProject("WPF User Control Library", "wpfusercontrollib");
            //CreateProject("Windows Forms App", "winforms");
            //CreateProject("Windows Forms Control Library", "winformscontrollib");
            //CreateProject("Windows Forms Class Library", "winformslib");
            //CreateProject("Worker Service", "worker");
            //CreateProject("Unit Test Project", "mstest");
            //CreateProject("NUnit 3 Test Project", "nunit");
            //CreateProject("NUnit 3 Test Item", "nunit-test");
            //CreateProject("xUnit Test Project", "xunit");
            //CreateProject("Blazor Server App", "blazorserver");
            //CreateProject("Blazor WebAssembly App", "blazorwasm");
            //CreateProject("ASP.NET Core Empty", "web");
            //CreateProject("ASP.NET Core Web App MVC", "mvc");
            //CreateProject("ASP.NET Core Web App", "webapp");
            //CreateProject("ASP.NET Core with Angular", "angular");
            //CreateProject("ASP.NET Core with React.js", "react");
            //CreateProject("ASP.NET Core with React.js and Redux", "reactredux");
            //CreateProject("Razor Class Library", "razorclasslib");
            //CreateProject("ASP.NET Core Web API", "webapi");
            //CreateProject("ASP.NET Core gRPC Service", "grpc");

            //second way
            List<Task> tasks = new List<Task>();
            foreach(var item in TemplatesList())
            {
                tasks.Add(Task.Run(() => CreateProject(item.TemplateName, item.ShortName)));
                
            }
            Task.WaitAll(tasks.ToArray());
        }

        public List<ProjectTamplate> TemplatesList()
        {
            List<ProjectTamplate> templates = new List<ProjectTamplate>();
            using (PowerShell power = PowerShell.Create())
            {
                power.AddScript("dotnet new --list");
                var result = power.Invoke();
                for (int i = 2; i < result.Count; i++)
                {
                    var template = (string)result[i].BaseObject;
                    var splitTemplate = template.Split("  ").Distinct().ToList();
                    if (splitTemplate.Any(c => c.Contains("C#")) || splitTemplate.Any(c => c.Contains("F#")))
                        templates.Add(new ProjectTamplate()
                        {
                            TemplateName = splitTemplate[0],
                            ShortName = splitTemplate[2],
                            Langs = splitTemplate[3],
                            Tags = splitTemplate[4]
                        });
                }

            }
            return templates;
        }
    }
}
