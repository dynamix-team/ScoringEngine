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
            
            if (args.Length > 1 && System.IO.Directory.Exists(args[0]) && System.IO.Directory.Exists(args[1]))
            {
                string inputdir = args[0];
                string outputdir = args[1];
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
                        File.WriteAllText(outfile, TemplateConversion(File.ReadAllText(template.FullName)));
                    }
                    catch
                    {
                        Console.WriteLine("Failed to import: " + template.FullName);
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid file path "  + args[0]);
            }
        }

        static string TemplateConversion(string contents)
        {
            string result = "//cst\r\n";
            contents = contents.Replace(";", ";\r\n"); //Fix any inlining so we can get accurate using defs
            string[] lines = contents.Split('\n', '\r');
            
            foreach(string s in lines)
            {
                if (s.Trim().Length < 1)
                    continue;
                if (s.Trim().Length > 6 && s.Trim().Substring(0, 5) == "using")
                    result += "//?";
                result += s.Trim() + "\r\n";
            }
            return result;
        }
    }
}
