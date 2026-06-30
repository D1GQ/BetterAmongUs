using BetterAmongUs.Modules;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;

namespace BetterAmongUs.Attributes;

/// <summary>
/// Attribute to register a class in the Il2Cpp runtime, optionally specifying interfaces to implement.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal class RegisterInIl2Cpp(params Type[] interfaces) : Attribute
{
    /// <summary>
    /// Gets the array of interface types that the registered class should implement in Il2Cpp.
    /// </summary>
    /// <value>An array of interface types, or an empty array if none were specified.</value>
    public Type[] Interfaces { get; } = interfaces ?? [];

    /// <summary>
    /// Registers all classes marked with <see cref="RegisterInIl2Cpp"/> from the executing assembly.
    /// </summary>
    internal static void Initialize()
    {
        var types = ModInfo.Assembly.GetTypes();
        foreach (var type in types)
        {
            var attr = type.GetCustomAttribute<RegisterInIl2Cpp>();
            if (attr == null)
                continue;

            try
            {
                if (attr.Interfaces.Length > 0)
                    ClassInjector.RegisterTypeInIl2Cpp(type, new RegisterTypeOptions { Interfaces = attr.Interfaces });
                else
                    ClassInjector.RegisterTypeInIl2Cpp(type);
            }
            catch (Exception ex)
            {
                Logger_.Error($"Failed to register {type.Name}: {ex.Message}");
            }
        }
    }
}