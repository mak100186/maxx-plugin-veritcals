using System.Reflection;
using System.Runtime.Loader;

namespace Maxx.PluginVerticals.Core.Extensions;

public class AssemblyLoader : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public Assembly Assembly { get; }

    public AssemblyLoader(string fullPathToPlugin)
    {
        _resolver = new(fullPathToPlugin);

        var assemblyName = AssemblyName.GetAssemblyName(fullPathToPlugin);
        Assembly = LoadFromAssemblyName(assemblyName);
        if (Assembly == null)
        {
            var message = $"Assembly \"{assemblyName}\" not found";
            throw new(message);
        }
    }

    #region Overrides

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }

    #endregion
}
