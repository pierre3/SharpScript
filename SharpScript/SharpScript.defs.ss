//[Default Import Assemblies]
// microlib.dll
// System.dll
// System.Core.dll
// System.Xml.dll
// System.Xml.Linq.dll
// SharpScript.dll

//[Additional References]
// @assembly-name System.Xml
// @assembly-name System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// @assembly-path C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Xml\v4.0_4.0.0.0__b77a5c561934e089\System.Xml.dll

//[Using Namespaces]
@using System;
@using System.Collections.Generic;
@using System.Linq;
@using System.IO;
@using System.Text;

@type
partial class CodeExecutor
{
	private void _wl(string str)
	{
		Console.WriteLine(str);
	}

	private void _wl(string format, params object[] args)
	{
		Console.WriteLine(format,args);	
	}
	
	private void _wl<T>(IEnumerable<T> collection)
	{
		Console.WriteLine(string.Join(Environment.NewLine, collection));	
	}
}
@endtype