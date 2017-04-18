using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CSharp;

namespace Regulus.Remoting.Unity
{
    public class AssemblyOutputer
    {
        //private readonly Assembly _CommonAsm;

        private readonly Assembly _UnityEngine;

        private readonly string _CommonNamespace;

        private readonly Type[] _Types;

        public event Action<string> ErrorMessageEvent;

        public AssemblyOutputer(Assembly common_asm, Assembly unity_engine, string common_namespace)
        {
            _UnityEngine = unity_engine;
            _CommonNamespace = common_namespace;

            _Types = _GetTypes(common_asm).ToArray();
        }

        public AssemblyOutputer(Type[] types,Assembly unity_engine, string common_namespace)
        {            
            _UnityEngine = unity_engine;
            _CommonNamespace = common_namespace;

            _Types = _GetTypes(types);
        }

        private Type[] _GetTypes(Type[] types)
        {
            return
                (from type in types
                 where type.IsInterface && type.Namespace != null && type.Namespace == _CommonNamespace
                 select type).ToArray();
        }

        private IEnumerable<Type> _GetTypes(Assembly asm)
        {
            var types = asm.GetTypes();
            foreach (var type in types)
            {
                if (type.IsInterface && type.Namespace == _CommonNamespace)
                {
                    yield return type;
                }
            }
        }

        public void OutputDll(string output_path , string asm_path)
        {
            var codes = new List<string>();

            var protocolBuilder = new Regulus.Protocol.CodeBuilder();
            protocolBuilder.ProviderEvent += (name,code) => codes.Add(code);
            protocolBuilder.GpiEvent += (name, code) => codes.Add(code);
            protocolBuilder.EventEvent += (type_name, event_name, code) => codes.Add(code);
            protocolBuilder.Build(_CommonNamespace + ".ProtocolProvider" ,new [] { _CommonNamespace  } , _Types);

            var unityProtocolBuilder = new Regulus.Remoting.Unity.CodeBuilder(_Types, _CommonNamespace , "ProtocolProvider");
            
            unityProtocolBuilder.AgentEvent += (name , code) =>  codes.Add(code);
            unityProtocolBuilder.TypeEvent += (name, ads, b) =>
            {
                codes.Add(ads);
                codes.Add(b);
            };            
            unityProtocolBuilder.Build();


            var optionsDic = new Dictionary<string, string>
            {
                {"CompilerVersion", "v3.5"}
            };
            var provider = new CSharpCodeProvider(optionsDic);

            var options = new CompilerParameters
            {

                OutputAssembly = output_path,
                GenerateExecutable = false,
                ReferencedAssemblies =
                {
                    "System.Core.dll",
                    "RegulusLibrary.dll",
                    "RegulusRemoting.dll",
                    "protobuf-net.dll",
                    _UnityEngine.Location,
                    asm_path
                }
            };
            var result = provider.CompileAssemblyFromSource(options, codes.ToArray());


            if(ErrorMessageEvent != null)
                for (int i = 0; i < result.Errors.Count; i++)
                {
                    var error = result.Errors[i];
                    if(error.IsWarning == false)
                        ErrorMessageEvent(result.Errors[i].ErrorText);
                }
                
        }

        public void OutputDir(string output_path)
        {

            System.IO.Directory.Delete(output_path , true);
            var proxy = output_path + "/Proxy";
            var adsorber = output_path + "/Adsorber";
            var broadcaster = output_path + "/Broadcaster";
            System.IO.Directory.CreateDirectory(proxy);
            System.IO.Directory.CreateDirectory(adsorber);
            System.IO.Directory.CreateDirectory(broadcaster);
            var protocolBuilder = new Regulus.Protocol.CodeBuilder();
            protocolBuilder.ProviderEvent += (name , code) => _WriteFile(_GetFile(proxy, name) , code);
            protocolBuilder.GpiEvent += (name, code) => _WriteFile(_GetFile(proxy, name), code);
            protocolBuilder.EventEvent += (type_name, event_name, code) => _WriteFile(_GetFile(proxy, type_name + event_name), code);
            protocolBuilder.Build(_CommonNamespace+".ProtocolProvider", new[] { _CommonNamespace }, _Types);

            var unityProtocolBuilder = new Regulus.Remoting.Unity.CodeBuilder(_Types, _CommonNamespace, "ProtocolProvider");

            unityProtocolBuilder.AgentEvent += (name,code) => _WriteFile(_GetFile(output_path, name), code);
            unityProtocolBuilder.TypeEvent += (name, ads, b) =>
            {
                _WriteFile(_GetFile(adsorber, name + "Adsorber"), ads);
                _WriteFile(_GetFile(broadcaster, name + "Broadcaster"), b);
                
            };
            unityProtocolBuilder.Build();
        }

        private void _WriteFile(string path, string code)
        {
            System.IO.File.WriteAllText(path , code);
        }

        private string _GetFile(string output_path, string name)
        {
            return  string.Format("{0}\\{1}.cs" , output_path , name);
        }
    }
}