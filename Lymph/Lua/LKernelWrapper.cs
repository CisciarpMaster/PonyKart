using LuaNetInterface;

namespace Lymph.Lua {
	[LuaPackage("Kernel", "A wrapper for Lua for the kernel, since it doesn't support generics")]
	public class LKernelWrapper {

		public LKernelWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

	}
}
