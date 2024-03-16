using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sigurd.Bus;

internal static class MethodAccessibilityExtensions
{
    public static bool IsAccessible(this MethodInfo method, Assembly context)
    {
        if (method.IsPrivate || method.IsFamily || method.IsFamilyAndAssembly) return false; // 'private', 'protected', 'private protected'
        if (method.IsPublic) { // 'public'
            return method.DeclaringType!.IsAccessible(context);
        }
        if (method.IsAssembly || method.IsFamilyOrAssembly) { // 'internal', 'protected internal'
            if (!method.DeclaringType!.Assembly.MakesInternalsVisibleTo(context)) return false;
            return method.DeclaringType!.IsAccessible(context);
        }

        return false; // something is wrong, but the member is definitely inaccessible
    }

    public static bool IsAccessible(this Type type, Assembly context)
    {
        if (type.IsPublic) return true;
        if (type.IsNested) {
            if (type.IsNestedPrivate || type.IsNestedFamily || type.IsNestedFamANDAssem) return false; // 'private', 'protected', 'private protected'
            if (type.IsNestedPublic) { // 'public'
                return type.DeclaringType!.IsAccessible(context);
            }
            if (type.IsNestedAssembly || type.IsNestedFamORAssem) { // 'internal', 'protected internal'
                if (!type.Assembly.MakesInternalsVisibleTo(context)) return false;
                return type.DeclaringType!.IsAccessible(context);
            }
            return false;
        }

        return false;
    }

    public static bool MakesInternalsVisibleTo(this Assembly assembly, Assembly other)
    {
        var otherName = other.GetName();

        var attributes = assembly.GetCustomAttributes<InternalsVisibleToAttribute>();
        return attributes
            .Any(attribute => AssemblyName.ReferenceMatchesDefinition(new AssemblyName(attribute.AssemblyName), otherName));
    }
}
