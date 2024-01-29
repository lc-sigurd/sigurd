using System;
using Sigurd.Common.Core;

namespace Sigurd.Common.Resources;

/// <summary>
/// Used to uniquely identify objects, for example in an <see cref="IRegistry{TValue}"/>.
/// </summary>
public class ResourceLocation: IEquatable<ResourceLocation>, IComparable<ResourceLocation>
{
    /// <summary>
    /// The delimiter used to separate the namespace from the path in <see cref="ResourceLocation"/>
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
        throw new ResourceLocationException($"Non [a-z0-9_.-] character in namespace of location: {Render(@namespace, path)}");
    }

    /// <summary>
    /// Determines whether or not a resource location namespace <see langword="string"/> is valid.
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
        throw new ResourceLocationException($"Non [a-z0-9/._-] character in path of location: {Render(@namespace, path)}");
    }

    /// <summary>
    /// Determines whether or not a resource location path <see langword="string"/> is valid.
    /// </summary>
    /// <param name="path">The path <see langword="string"/> to validate.</param>
    /// <returns><see langword="true"/> if the path is valid.</returns>
    public static bool IsValidPath(string path)
    {
        return true;
    }

    /// <summary>
    /// Decompose a resource location string into its namespace and path.
    /// <see cref="ResourceLocation.DefaultNamespace"/> will be used
    /// if the location does not contain a namespace.
    /// </summary>
    /// <param name="location">The resource location <see langword="string"/> to decompose.</param>
    /// <returns><see cref="ValueTuple"/> of namespace, path</returns>
    protected static ValueTuple<string, string> Decompose(string location)
    {
        ValueTuple<string, string> parts = ValueTuple.Create(DefaultNamespace, location);
        int separatorIndex = location.IndexOf(NamespaceSeparator, StringComparison.InvariantCultureIgnoreCase);
        if (separatorIndex < 0) return parts;
        parts.Item2 = location.Substring(separatorIndex + 1);
        if (separatorIndex == 0) return parts;
        parts.Item1 = location.Substring(0, separatorIndex);
        return parts;
    }

    /// <summary>
    /// The domain (Mod GUID) of this resource location.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// The path of this resource location.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Create a new ResourceLocation.
    /// </summary>
    /// <param name="namespace">The resource domain identifier to use, usually a mod GUID.</param>
    /// <param name="path">The resource path to use, this should be unique amongst content of a particular type.</param>
    public ResourceLocation(string @namespace, string path)
    {
        AssertValidNamespace(@namespace, path);
        AssertValidPath(@namespace, path);
        Namespace = @namespace;
        Path = path;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    public ResourceLocation(string path)
    {
        var parts = Decompose(path);
        (Namespace, Path) = parts;
    }

    /// <summary>
    /// Determines whether the resource location is equal to another resource location.
    /// </summary>
    /// <param name="other">the <see cref="ResourceLocation"/> to compare against</param>
    /// <returns><see langword="true"/> when equal to <c>other</c>.</returns>
    public bool Equals(ResourceLocation? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Namespace == other.Namespace && Path == other.Path;
    }

    /// <summary>
    /// Determines whether the resource location is equal to an object.
    /// </summary>
    /// <param name="obj">the <see cref="object"/> to compare against</param>
    /// <returns><see langword="true"/> when equal to <c>obj</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ResourceLocation)obj);
    }

    public int CompareTo(ResourceLocation? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var namespaceComparison = string.Compare(Namespace, other.Namespace, StringComparison.Ordinal);
        if (namespaceComparison != 0) return namespaceComparison;
        return string.Compare(Path, other.Path, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets a hash code for this <see cref="ResourceLocation"/>.
    /// </summary>
    /// <returns>An <see cref="int"/> that contains the hash code for the <see cref="ResourceLocation"/>.</returns>
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
