using System;
using DotNetTemplatesCreator.Utils;

namespace DotNetTemplatesCreator
{
   class Program
   {
      public static void WriteLine(string buffer, ConsoleColor foreground = ConsoleColor.DarkGreen, ConsoleColor backgroundColor = ConsoleColor.Black)
      {
         Console.ForegroundColor = foreground;
         Console.BackgroundColor = backgroundColor;
         Console.WriteLine(buffer);
         Console.ResetColor();
      }

      public static string ReadLine()
      {
         var line = Console.ReadLine();
         return line ?? string.Empty;
      }


      static void Main(string[] args)
      {
         WriteLine("creating projects operation started");
         WriteLine("this operation could take some while...");
         WriteLine("");
         new CreateProjectsUtility().Execute();
         WriteLine("");
         WriteLine("operation completed ^_^");
      }
   }
}
