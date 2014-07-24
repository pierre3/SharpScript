using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpScript
{
    public partial class CodeGenerator
    {
        public IList<string> UsingNamespaces { get; private set; }
        public IList<string> TypeDefinitions { get; private set; }
        public IList<string> ExecuteCodes { get; private set; }
        public IList<MetadataFileReference> References { get; private set; }
        public string GeneratedText
        {
            get
            {
                return this.GenerationEnvironment == null ? "" : this.GenerationEnvironment.ToString();
            }
        }
        public bool IsEmpty
        {
            get
            {
                return TypeDefinitions.Count == 0
                    && ExecuteCodes.Count == 0;
            }
        }

        public CodeGenerator()
            : base()
        {
            UsingNamespaces = new List<string>();
            TypeDefinitions = new List<string>();
            ExecuteCodes = new List<string>();
            References = new List<MetadataFileReference>() 
            { 
                //microlib.dll
                new MetadataFileReference(typeof(object).Assembly.Location),
                //System.dll
                new MetadataFileReference(typeof(System.Collections.ObjectModel.ObservableCollection<>).Assembly.Location),
                //System.Core.dll
                new MetadataFileReference(typeof(System.Linq.Enumerable).Assembly.Location),
                //System.Xml.dll
                new MetadataFileReference(typeof(System.Xml.XmlDocument).Assembly.Location),
                //Sytem.Xml.Linq
                new MetadataFileReference(typeof(System.Xml.Linq.XDocument).Assembly.Location),
                //this assembly
                new MetadataFileReference(this.GetType().Assembly.Location),
            };
        }

        public bool AddReference(string name)
        {
            if (string.IsNullOrEmpty(name))
            { return false; }

            try
            {
#pragma warning disable 618
                var assembly = System.Reflection.Assembly.LoadWithPartialName(name);
#pragma warning restore 618
                if (References.All(r => r.FullPath != assembly.Location))
                {
                    References.Add(new MetadataFileReference(assembly.Location));

                }
                return true;
            }
            catch (FileNotFoundException) { return false; }
            catch (FileLoadException) { return false; }
            catch (BadImageFormatException) { return false; }
        }

        public bool AddReferenceFrom(string path)
        {
            if (path == null)
            { throw new ArgumentNullException(path); }

            if (System.IO.File.Exists(path))
            {
                if (References.All(r => r.FullPath != path))
                {
                    References.Add(new MetadataFileReference(path));
                }
                return true;
            }
            return false;
        }

        public static CodeGenerator CreateFrom(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return CreateFrom(stream);
            }
        }

        public static CodeGenerator CreateFrom(Stream stream)
        {
            var instance = new CodeGenerator();
            using (var reader = new StreamReader(stream))
            {
                instance.AppendCode(reader);
            }
            return instance;
        }

        public void AppendCode(TextReader reader)
        {
            bool isTypeDefBlock = false;

            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                { break; }

                var trimLine = line.TrimStart();

                //Comments
                if (trimLine == string.Empty || trimLine.StartsWith("//") || trimLine.StartsWith(";"))
                { continue; }

                //import assembly
                if (trimLine.StartsWith("@assembly-name"))
                {
                    var assemblyName = trimLine.Replace("@assembly-name", "").Trim();
                    this.AddReference(assemblyName);
                    continue;
                }
                if (trimLine.StartsWith("@assembly-path"))
                {
                    var path = trimLine.Replace("@assembly-path", "").Trim();
                    this.AddReferenceFrom(path);
                    continue;
                }

                //using namespace
                if (trimLine.StartsWith("@using"))
                {
                    var tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length >= 2)
                    {
                        var ns = tokens[1].TrimEnd(';');
                        if (!UsingNamespaces.Contains(ns))
                        {
                            this.UsingNamespaces.Add(ns);
                        }
                    }
                    continue;
                }

                switch (trimLine)
                {
                    case "@type":
                        isTypeDefBlock = true;
                        break;
                    case "@endtype":
                        isTypeDefBlock = false;
                        break;
                    case "@run":
                        return;
                    default:
                        if (isTypeDefBlock)
                        {
                            this.TypeDefinitions.Add(line);
                        }
                        else
                        {
                            this.ExecuteCodes.Add(line);
                        }
                        break;
                }

            }
        }

        public void Clear()
        {
            this.GenerationEnvironment.Clear();
        }

    }
}
