using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Engine.Installer.Core.Templates
{
    /// <summary>
    /// A template translation framework for building cross platform engines securely
    /// </summary>
    public static class Translator
    {
        /// <summary>
        /// Convert CS source to a CST
        /// </summary>
        /// <param name="CSInput">The c# source code</param>
        /// <returns>A CST formatted string</returns>
        public static string MakeCST(string CSInput)
        {
            string result = "//cst\r\n";
            CSInput = CSInput.Replace(";", ";\r\n"); //Fix any inlining so we can get accurate using defs
            string[] lines = CSInput.Split('\n', '\r');
            bool EndUsings = false;
            foreach (string s in lines)
            {
                if (s.Trim().Length < 1)
                    continue;
                if (!EndUsings && s.Trim().Length > 6 && s.Trim().Substring(0, 5) == "using")
                    result += "//?";
                else
                    EndUsings = true;
                result += s.Trim() + "\r\n";
            }
            return result;
        }
#if DEBUG       
        /// <summary>
        /// Build a debugging engine from the installation framework
        /// </summary>
        /// <param name="EngineBase">The base engine string</param>
        /// <param name=""></param>
        /// <returns></returns>
        public static string BuildDebuggingEngine(string EngineBase, string TemplatesDirectory, CheckDefinition[] checks, bool Online)
        {
            EngineBase = EngineBase.Replace("//?debug.online", Online ? "#define ONLINE" : "#undef ONLINE");
            Dictionary<CheckTypes, string> RuntimeTemplates = new Dictionary<CheckTypes, string>();
            try
            {
                System.IO.DirectoryInfo TemplateDir = new System.IO.DirectoryInfo(TemplatesDirectory);
                foreach (System.IO.FileInfo template in TemplateDir.GetFiles("*.cst", System.IO.SearchOption.AllDirectories))
                {
                    string templatename = template.Name.ToLower().Replace(".cst", "");
                    if (Enum.TryParse(templatename, true, out CheckTypes checktype))
                    {
                        RuntimeTemplates[checktype] = template.FullName;
                    }
                }
            }
            catch
            {
            }

            List<CheckPreWrapper> Wrappers = new List<CheckPreWrapper>();

            foreach(var check in checks)
            {
                try
                {
                    CSTemplate template = System.IO.File.ReadAllLines(RuntimeTemplates[(CheckTypes)check.CheckKey]);
                    Wrappers.Add(new CheckPreWrapper() { Definition = check, Template = template });
                }
                catch
                {
                }
            }
            CSTemplate _checktemplate = System.IO.File.ReadAllLines(RuntimeTemplates[CheckTypes.CheckTemplate]);
            Wrappers.Add(new CheckPreWrapper() { Definition = null, Template = _checktemplate });

            return BuildEngine(EngineBase, Wrappers.ToArray());
        }

        /// <summary>
        /// Returns a debugging check list from an xml formatted input string
        /// </summary>
        /// <param name="input">The xml to parse into debugging checks</param>
        /// <returns></returns>
        public static CheckDefinition[] ParseDebuggingChecklist(string input)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(input);
            List<CheckDefinition> checks = new List<CheckDefinition>();
            foreach(XmlNode node in doc.GetElementsByTagName("check"))
            {
                try
                {
                    Enum.TryParse(node["key"].InnerText, true, out CheckTypes checktype);
                    string[] args = new string[node["arguments"].ChildNodes.Count];
                    for(int i = 0; i < args.Length; i++)
                    {
                        args[i] = node["arguments"].ChildNodes[i].InnerText;
                    }
                    CheckDefinition d = CheckDefinition.DebugCheck(checktype, Convert.ToUInt16(node["id"].InnerText), Convert.ToInt16(node["points"].InnerText), Convert.ToByte(node["flags"].InnerText), uint.Parse(node["answer"].InnerText, System.Globalization.NumberStyles.HexNumber), args);
                    if (d != null)
                        checks.Add(d);
                }
                catch(Exception e)
                {
#if DEBUG
                    Console.WriteLine(e.StackTrace.ToString());
                    Console.WriteLine(e.Message);
#endif
                }
            }
            return checks.ToArray();
        }
#endif
        /// <summary>
        /// Build an engine.cs from a base engine and a list of checks
        /// </summary>
        /// <param name="EngineBase">The base of the engine</param>
        /// <param name="checks">A list of checks</param>
        /// <returns></returns>
        private static string BuildEngine(string EngineBase, CheckPreWrapper[] checks)
        {
            List<string> Includes = new List<string>();
            int count = 0;
            string engine_classes = "";
            string engine_includes = "";
            string engine_init = "";
            string engine_fields = "";
            string engine_tick = "";
            foreach (var check in checks)
            {
                foreach(string include in check.Template.Includes)
                {
                    if(!engine_includes.Contains(include))
                        engine_includes += include + "\n";
                    Includes.Add(include);
                }
                string code = "";
                foreach(string s in check.Template.CodeLines)
                {
                    code += s + "\r\n";
                }
                if (check.Definition == null) //should only work for the checktemplate class
                {
                    engine_classes += code + "\r\n";
                    continue;
                }
                string old = code;
                string name = ((CheckTypes)check.Definition.CheckKey).ToString() + "__" + count;
                code = code.Replace(((CheckTypes)check.Definition.CheckKey).ToString(), name);
                string args = "";
                if(code != old)
                {
                    engine_classes += code + "\r\n";
                    check.ClassName = name;
                    string[] arguments = check.Definition.Arguments;
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        args += "@\"" + arguments[i] + "\"" + (i == arguments.Length - 1 ? "" : ", ");
                    }
                    check.Declarator = "c_" + count + " = new " + check.ClassName + "(" + args + "){ Flags = (byte)" + check.Definition.Flags + " }; Expect((uint)" + check.Definition.CheckKey + "," + "(uint)" + check.Definition.OfflineAnswer + ");";
                    check.InstanceName = "c_" + count;
                    check.StateName = check.InstanceName + "_s";
                    check.Header = "private " + check.ClassName + " c_" + count + ";\n\rprivate uint " + check.StateName + ";";
                    
                    engine_fields += check.Header + "\r\n";
                    engine_init += check.Declarator + "\r\n";
                    engine_tick += "if(" + check.InstanceName + "?.Enabled ?? false){ " + check.StateName + " = await " + check.InstanceName + ".GetCheckValue(); ";
                    engine_tick += "RegisterCheck((ushort)" + check.Definition.CheckID + "|((uint)" + check.InstanceName + ".Flags << 16)," + check.StateName + ");}\n\r";
                }
                count++;
            }
            EngineBase = EngineBase.Replace("//?installer.includes", engine_includes);
            EngineBase = EngineBase.Replace("//?installer.fields", engine_fields);
            EngineBase = EngineBase.Replace("//?installer.init", engine_init);
            EngineBase = EngineBase.Replace("//?installer.tick", engine_tick);
            EngineBase = EngineBase.Replace("//?installer.classes", engine_classes);
            return EngineBase;
        }
    }
}
