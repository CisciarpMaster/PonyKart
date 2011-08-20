using LuaNetInterface;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class LuaMainWrapper {

		public LuaMainWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("doFile", "Runs a lua script.", "string file - The filename. If it doesn't start with media/scripts/, it's added automagically.")]
		public void DoFile(string file) {
			LKernel.Get<LuaMain>().DoFile(file);
		}

		
		[LuaFunction("quit", "Quits the lua VM.")]
		public void Quit() {
			LKernel.Get<LuaMain>().Quit();
		}

		
		[LuaFunction("restart", "Shuts down the Lua VM and starts it again.")]
		public void Restart() {
			LKernel.Get<LuaMain>().Restart();
		}

		
		[LuaFunction("print", "Prints something.", "string s - the string to print")]
		public void Print(string s) {
			LKernel.Get<LuaMain>().Print(s);
		}

		[LuaFunction("helpcmd", "Show help for a given command or package", "string command - Package to get help of.")]
		public void GetCommandHelp(string command) {
			LKernel.Get<LuaMain>().GetCommandHelp(command);
		}

		[LuaFunction("help", "List available commands.")]
		public void GetHelp() {
			LKernel.Get<LuaMain>().GetHelp();
		}
	}
}
