@type
partial class CodeExecutor
{
	static void SjisToUtf8(string path)
	{
		var text = File.ReadAllText(path, Encoding.GetEncoding("shift_jis"));
		File.WriteAllText(path, text, Encoding.UTF8);
	}
}
@endtype

var path = parameters.FirstOrDefault(p=>File.Exists(p));
if(path == null)
{
	throw new ArgumentException("A file name must be specified in the parameters.");
}

SjisToUtf8(path);
_wl("Completed. >> {0}",path);