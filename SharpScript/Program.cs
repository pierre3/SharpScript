using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SharpScript
{
    public interface ICodeExecutor
    {
        void Execute(string[] parameters);
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            var generator = RunScript(args);
            while (true)
            {
                var line = Console.ReadLine();
                switch (line)
                {
                    case "@list":
                        ListCode(generator.GeneratedText);
                        break;
                    case "@clear":
                        generator = RunScript(new string[0]);
                        break;
                    case "@exit":
                        return;
                    default:
                        generator.Clear();
                        using (var reader = new StringReader(line))
                        {
                            generator.AppendCode(reader);
                        }
                        generator.AppendCode(Console.In);
                        CodeExecute(generator, new string[0]);
                        break;
                }
            }
        }

        private static CodeGenerator RunScript(string[] args)
        {
            var defsFile = @".\SharpScript.defs.ss";
            if (!File.Exists(defsFile))
            {
                Console.Error.WriteLine("File not found. ({0})", Path.GetFullPath(defsFile));
                return new CodeGenerator();
            }

            var generator = CodeGenerator.CreateFrom(defsFile);
            var parameters = new string[0];
            if (args.Length > 0 && File.Exists(args[0]))
            {
                using (var stream = new FileStream(args[0], FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream))
                {
                    generator.AppendCode(reader);
                }
                parameters = args.Skip(1).ToArray();
            }

            CodeExecute(generator, parameters);
            return generator;
        }

        private static void CodeExecute(CodeGenerator generator, string[] parameters)
        {
            if (generator == null)
            { throw new ArgumentNullException("generator"); }
            if (parameters == null)
            { throw new ArgumentNullException("parameters"); }

            var code = generator.TransformText();
            var executor = CreateExecutor(code, generator.References);
            if (executor != null)
            {
                try
                {
                    executor.Execute(parameters);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            }
            else
            {
                ListCode(code);
            }
        }

        static void ListCode(string code)
        {
            using (var reader = new StringReader(code))
            {
                int i = 0;
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null) { break; }
                    Console.WriteLine((i++).ToString("D3") + ": " + line);
                }
            }
        }
                
        static ICodeExecutor CreateExecutor(string code, IEnumerable<MetadataFileReference> references)
        {
            //ソースコードをParseしてSyntax Treeを生成
            var sourceTree = CSharpSyntaxTree.ParseText(code);

            //コンパイルオブジェクト
            var assemblyName = string.Format("GeneratedAssembly_{0}", Guid.NewGuid());
            var compilation = CSharpCompilation.Create(assemblyName,
                new[] { sourceTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            Assembly assembly = null;
            using (var stream = new System.IO.MemoryStream())
            {
                //コンパイル実行。結果をストリームに書き込む
                var result = compilation.Emit(stream);
                if (result.Success)
                {
                    //成功時 アセンブリとしてロードする。
                    assembly = System.Reflection.Assembly.Load(stream.GetBuffer());
                }
                else
                {
                    //失敗時 コンパイルエラーを出力
                    foreach (var mes in result.Diagnostics.Select(d =>
                        string.Format("[{0}]:{1}({2})", d.Id, d.GetMessage(), d.Location.GetLineSpan().StartLinePosition)))
                    {
                        Console.Error.WriteLine(mes);
                    }
                    return null;
                }
            }

            //アセンブリから目的のクラスを取得してインスタンスを生成
            var type = assembly.GetType("SharpScript.CodeExecutor");
            return (ICodeExecutor)Activator.CreateInstance(type);

        }
    }
}
