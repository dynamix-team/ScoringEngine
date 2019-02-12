using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateTranslation
{
    class Program
    {
        static void Main(string[] args)
        {
            
            if (args.Length > 2)
            {
                if(args[0].ToLower().Contains("/t") && System.IO.Directory.Exists(args[1]) && System.IO.Directory.Exists(args[2]))
                {
                    string inputdir = args[1];
                    string outputdir = args[2];
                    DirectoryInfo TemplateDir = new DirectoryInfo(inputdir);
                    foreach (FileInfo template in TemplateDir.GetFiles("*template.cs", SearchOption.AllDirectories))
                    {
                        string outfile = Path.Combine(outputdir, template.Name.Replace(".cs", ".cst"));
                        try
                        {
                            if (File.Exists(outfile))
                            {
                                File.Delete(outfile);
                            }
                            File.WriteAllText(outfile, Engine.Installer.Core.Templates.Translator.MakeCST(File.ReadAllText(template.FullName)));
                        }
                        catch
                        {
                            Console.WriteLine("Failed to import: " + template.FullName);
                        }
                    }
                }
#if DEBUG
                else if(args[0].ToLower().Contains("/e"))
                {
                    try
                    {
                        File.WriteAllText(args[1], Engine.Installer.Core.Templates.Translator.BuildDebuggingEngine(File.ReadAllText(args[1]), args[2], Engine.Installer.Core.Templates.Translator.ParseDebuggingChecklist(File.ReadAllText(args[3])), false));
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.StackTrace.ToString());
                        Console.WriteLine(e.Message);
                        Environment.Exit(2);
                    }
                }
                else if(args[0].ToLower().Contains("/i"))
                {
                    try
                    {
                        //1: in xml
                        //2: out filename
                        File.WriteAllBytes(args[2], Engine.Installer.Core.InstallationPackage.MakeDebugInstall(File.ReadAllText(args[1])));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace.ToString());
                        Console.WriteLine(e.Message);
                        Environment.Exit(2);
                    }
                }
#endif
            }
            else
            {
                Environment.Exit(1);
            }
        }
    }
}
