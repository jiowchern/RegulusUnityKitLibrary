using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Remoting.Unity.ProrocolBuilder
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 1)
            {
                Console.WriteLine("Param 1 Need to build file path.");

                var iniSample = @"
[Build]
common_path = path/common.dll
unityengine_path = path/UnityEngine.dll
common_namespace = Namespace
output_path = path/protocl.dll
";
                Console.WriteLine("The format is as follows");                
                Console.WriteLine(iniSample);
                System.IO.File.WriteAllText("BuildProtocol.ini" , iniSample);
                return;
            }


            var path = args[0];

            var iniStream = System.IO.File.ReadAllText(path);
            var ini = new Regulus.Utility.Ini(iniStream);

            var commonPath = ini.Read("Build", "common_path");
            var unityenginePath = ini.Read("Build", "unityengine_path");
            var commonNamespace = ini.Read("Build", "common_namespace");
            var outputPath = ini.Read("Build", "output_path");


            var commonAsm = Assembly.LoadFile(commonPath);
            var unityengineAsm = Assembly.LoadFile(unityenginePath);

            var assemblyOutputer = new AssemblyOutputer(commonAsm, unityengineAsm, commonNamespace);
            assemblyOutputer.ErrorMessageEvent += Console.WriteLine;
            assemblyOutputer.OutputDll(outputPath , commonAsm.Location);            
        }
    }

    
}


