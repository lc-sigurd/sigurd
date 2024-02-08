using System;
using Sigurd.Common.Core;

namespace Sigurd.Common.Resources;

/// <summary>
/// Used to uniquely identify objects, for example in an <see cref="IRegistry{TValue}"/>.
/// </summary>
public class ResourceName: IEquatable<ResourceName>, IComparable<ResourceName>
{
    /// <summary>
    /// The delimiter used to separate the namespace from the path in <see cref="ResourceName"/>
    /// string representations.
    /// </summary>
    public const string NamespaceSeparator = ":";

    /// <summary>
    /// The default namespace, used when none is provided.
    /// </summary>
    public const string DefaultNamespace = "lethal_company";

    private static string Render(string @namespace, string path) => @namespace + NamespaceSeparator + path;

    private static void AssertValidNamespace(string @namespace, string path)
    {
        if (IsValidNamespace(@namespace)) return;
        throw new ResourceNameException($"Non [a-z0-9_.-] character in namespace of name: {Render(@namespace, path)}");
    }

    /// <summary>
    /// Determines whether or not a resource name namespace <see langword="string"/> is valid.
    /// </summary>
    /// <param name="namespace">The namespace <see langword="string"/> to validate.</param>
    /// <returns><see langword="true"/> if the namespace is valid.</returns>
    public static bool IsValidNamespace(string @namespace)
    {
        return true;
    }

    private static void AssertValidPath(string @namespace, string path)
    {
        if (IsValidPath(path)) return;
        throw new ResourceNameException($"Non [a-z0-9/._-] character in path of name: {Render(@namespace, path)}");
    }

    /// <summary>
    /// Determines whether or not a resource name path <see langword="string"/> is valid.
    /// </summary>
    /// <param name="path">The path <see langword="string"/> to validate.</param>
    /// <returns><see langword="true"/> if the path is valid.</returns>
    public static bool IsValidPath(string path)
    {
        return true;
    }

    /// <summary>
    /// Decompose a resource name string into its namespace and path.
    /// <see cref="ResourceName.DefaultNamespace"/> will be used
    /// if the name does not contain a namespace.
    /// </summary>
    /// <param name="name">The resource name <see langword="string"/> to decompose.</param>
    /// <returns><see cref="ValueTuple"/> of namespace, path</returns>
    /// <exception cref="NullReferenceException"><paramref name="name"/> is null.</exception>
    protected static ValueTuple<string, string> Decompose(string? name)
    {
        return Decompose(name, DefaultNamespace);
    }

    /// <summary>
    /// Decompose a resource name string into its namespace and path.
    /// <paramref name="defaultNamespace"/> will be used
    /// if the name does not contain a namespace.
    /// </summary>
    /// <param name="name">The resource name <see langword="string"/> to decompose.</param>
    /// <param name="defaultNamespace">The namespace to default to if <paramref name="name"/> is missing one.</param>
    /// <returns><see cref="ValueTuple"/> of namespace, path</returns>
    /// <exception cref="NullReferenceException"><paramref name="name"/> is null.</exception>
    public static ValueTuple<string, string> Decompose(string? name, string defaultNamespace)
    {
        if (name is null)
            throw new ArgumentException("Can't decompose a `null` resource name.");

        ValueTuple<string, string> parts = ValueTuple.Create(defaultNamespace, name);
        int separatorIndex = name.IndexOf(NamespaceSeparator, StringComparison.InvariantCultureIgnoreCase);
        if (separatorIndex < 0) return parts;
        parts.Item2 = name.Substring(separatorIndex + 1);
        if (separatorIndex == 0) return parts;
        parts.Item1 = name.Substring(0, separatorIndex);
        return parts;
    }

    /// <summary>
    /// The domain (Mod GUID) of this resource name.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// The path of this resource name.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Create a new ResourceName.
    /// </summary>
    /// <param name="namespace">The resource domain identifier to use, usually a mod GUID.</param>
    /// <param name="path">The resource path to use, this should be unique amongst content of a particular type.</param>
    public ResourceName(string @namespace, string path)
    {
        AssertValidNamespace(@namespace, path);
        AssertValidPath(@namespace, path);
        Namespace = @namespace;
        Path = path;
    }

    /// <summary>
    /// Create a new ResourceName.
    /// Will use the <see cref="DefaultNamespace"/> if one is not provided.
    /// </summary>
    /// <param name="name">The full string representation of the resource name, including namespace, separator, and path.</param>
    public ResourceName(string name)
    {
        var parts = Decompose(name);
        (Namespace, Path) = parts;
    }

    /// <summary>
    /// Determines whether the resource name is equal to another resource name.
    /// </summary>
    /// <param name="other">the <see cref="ResourceName"/> to compare against</param>
    /// <returns><see langword="true"/> when equal to <c>other</c>.</returns>
    public bool Equals(ResourceName? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Namespace == other.Namespace && Path == other.Path;
    }

    /// <summary>
    /// Determines whether the resource name is equal to an object.
    /// </summary>
    /// <param name="obj">the <see cref="object"/> to compare against</param>
    /// <returns><see langword="true"/> when equal to <c>obj</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ResourceName)obj);
    }

    public int CompareTo(ResourceName? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var namespaceComparison = string.Compare(Namespace, other.Namespace, StringComparison.Ordinal);
        if (namespaceComparison != 0) return namespaceComparison;
        return string.Compare(Path, other.Path, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets a hash code for this <see cref="ResourceName"/>.
    /// </summary>
    /// <returns>An <see cref="int"/> that contains the hash code for the <see cref="ResourceName"/>.</returns>
    public override int GetHashCode()
    {
#if NETSTANDARD
        return HashCode.Combine(Namespace, Path);
#else
        throw null!;
#endif
    }

    /// <inheritdoc />
    public override string ToString() => Render(Namespace, Path);
}
