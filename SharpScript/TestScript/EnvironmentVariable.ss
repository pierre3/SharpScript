@using System.Collections;
@type
class Env
{
	private static Env process;
    private static Env user;
    private static Env machine;
	private EnvironmentVariableTarget target;

    public static Env Process
	{
		get
		{ 
			if( process == null )
			{
				process = new Env(EnvironmentVariableTarget.Process);
			}
			return process;
		}
	}

    public static Env User
	{ 
		get
		{
			if( user == null )
			{
				user = new Env(EnvironmentVariableTarget.User);
			}
			return user;
		}
		
	}

    public static Env Machine
	{
		get
		{
			if( machine == null )
			{
				machine = new Env(EnvironmentVariableTarget.Machine);
			}
			return machine;
		}
	}
    
    public string this[string key]
    {
        get
        {
            var result = GetVariables().FirstOrDefault(v => v.Key == key);
            if (result.Key == null) { return ""; }
            return result.Value;
        }
        set
        {
            Environment.SetEnvironmentVariable(key, value, target);
        }
    }

    public IDictionary<string, string> GetVariables()
    {
        return Environment.GetEnvironmentVariables(target)
            .OfType<DictionaryEntry>()
            .ToDictionary(de => (string)de.Key, de => (string)de.Value);
    }

    public Env(EnvironmentVariableTarget target)
    {
        this.target = target;
    }
}
@endtype

var variable = "CSharpScript";
Env.Process[variable] = "Roslyn!";
_wl("{0} = {1}",variable, Env.Process[variable]);
