using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Regulus.Remoting.Unity.ProtocolBuilder
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
agent_name = Namespace.Agent
output_path = path/protocol.dll
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
            
            var commonNamespace = ini.Read("Build", "agent_name");
            var outputPath = ini.Read("Build", "output_path");

            var commonAsm = Assembly.LoadFile(commonPath);
            var unityengineAsm = Assembly.LoadFile(unityenginePath);

            var regulusLibrary = _GetRegulusLibrary();
            var regulusRemoting = _GetRegulusRemoting();
            var regulusProrocol = _GetRegulusProtocol();
            var regulusProtocolUnity = _GetRegulusProtocolUnity();
            var regulusRemotingGhost = _GetRegulusRemotingGhost();
            var regulusSerialization = _GetRegulusSerialization();

            var assemblyOutputer = new AssemblyOutputer(commonAsm,  commonNamespace);
            assemblyOutputer.ErrorMessageEvent += Console.WriteLine;
            assemblyOutputer.OutputDll(outputPath , unityengineAsm , regulusLibrary , regulusRemoting , regulusProrocol , regulusProtocolUnity , regulusRemotingGhost , regulusSerialization);            
        }

        private static Assembly _GetRegulusSerialization()
        {
            return _GetAssembly("Regulus.Serialization.dll");
        }

        private static Assembly _GetRegulusRemotingGhost()
        {
            return _GetAssembly("RegulusRemotingGhostNative.dll");
        }

        private static Assembly _GetRegulusProtocolUnity()
        {
            return _GetAssembly("Regulus.Remoting.Unity.dll");
        }

        private static Assembly _GetRegulusProtocol()
        {
            return _GetAssembly("Regulus.Protocol.dll");
        }

        private static Assembly _GetRegulusLibrary()
        {
            return _GetAssembly("reguluslibrary.dll");
        }

        private static Assembly _GetRegulusRemoting()
        {
            return _GetAssembly("regulusremoting.dll");
        }

        private static Assembly _GetAssembly(string filename)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (_Check(assembly.Location, filename))
                {
                    return assembly;
                }
            }
            throw new Exception("Can not find library. " + filename );
        }

        private static bool _Check(string location, string file_name)
        {
            var filename = System.IO.Path.GetFileName(location).ToLower();
            return filename == file_name;
        }       
    }
}


