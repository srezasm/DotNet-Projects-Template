using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using DotNetTemplatesCreator.Models;

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

         _basePath = string.Join(Path.DirectorySeparatorChar, currentPath.Take(currentPath.Length - 1).Append("Templates").ToArray());
         if(!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
      }

      private void RemoveCurrentProjects()
      {
         try {
            var baseDirs = new DirectoryInfo(_basePath).GetDirectories();
            foreach(var directory in baseDirs) {
               Directory.Delete(directory.FullName, true);
            }

            var baseFiles = new DirectoryInfo(_basePath).GetFiles();
            foreach(var file in baseFiles) {
               File.Delete(file.FullName);
            }

            Console.WriteLine("   old projects removed");
         }
         catch(Exception e) {
            Console.WriteLine(e);
            throw;
         }
      }

      private void CreateProject(string projectName, string createProjectCommandName)
      {
         try {
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

            Console.WriteLine("   {0} created", projName);
         }
         catch(Exception e) {
            Console.WriteLine(e);
            throw;
         }
      }

      private List<ProjectTemplateModel> TemplatesList()
      {
         try {
            var templates = new List<ProjectTemplateModel>();
            using PowerShell power = PowerShell.Create();

            power.AddScript("dotnet new --list --type project");
            var result = power.Invoke();
            for(int i = 2; i < result.Count; i++) {
               var template = (string)result[i].BaseObject;

               var splitTemplate = template.Split("  ")
                  .Distinct()
                  .Where(x => !string.IsNullOrEmpty(x)).ToArray();

               if(splitTemplate.Any(c => c.Contains("C#")) || splitTemplate.Any(c => c.Contains("F#"))) {

                  templates.Add(new ProjectTemplateModel() {
                     TemplateName = splitTemplate[0],
                     ShortName = splitTemplate[1].Trim(),
                     Langs = splitTemplate[2],
                     Tags = splitTemplate[3]
                  });

               }
            }

            return templates;
         }
         catch(Exception e) {
            Console.WriteLine(e);
            throw;
         }
      }

      private List<FileInfo> FindAllProjectFiles()
      {
         try {
            var projects = new List<FileInfo>();

            var directories = new DirectoryInfo(_basePath).GetDirectories("*",
                           new EnumerationOptions {
                              AttributesToSkip = FileAttributes.Hidden
                           });

            foreach(var directory in directories) {
               projects.AddRange(directory.GetFiles().Where(f => f.Extension == ".csproj"));
            }

            return projects;
         }
         catch(Exception e) {
            Console.WriteLine(e);
            throw;
         }
      }

      public void AddLicenseInformation()
      {
         try {
            var str = new StringBuilder();
            var projects = FindAllProjectFiles();
            foreach(var proj in projects) {
               str.Append(
                  "<!-- This Project Is Auto Generated By dotnet-cli -->\n<!-- Source Code: https://github.com/srezasm/DotNet-Core-Projects-Template -->\n\n"
                  );

               str.Append(File.ReadAllText(proj.FullName));

               File.WriteAllText(proj.FullName, str.ToString());

               str.Clear();
            }

            Console.WriteLine("   license information added to projects");
         }
         catch(Exception e) {
            Console.WriteLine(e);
            throw;
         }
      }

      public void Execute()
      {
         RemoveCurrentProjects();

         var tasks = new List<Task>();
         foreach(var item in TemplatesList()) {
            tasks.Add(Task.Run(() => CreateProject(item.TemplateName, item.ShortName)));
         }
         Task.WaitAll(tasks.ToArray());

         AddLicenseInformation();
      }
   }
}
