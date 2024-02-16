/*
 * Copyright (c) 2024 The Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text.Json.Nodes;

namespace MSBuildTasks.StageThunderstorePackage;

public class ThunderstorePackageArchive
{
    static readonly PackageInstallRuleSet Rules = PackageInstallRuleSet.DefaultBepInExInstallRules;

    public FileInfo ArchivePath { get; }

    public string? PackageNamespace { get; private set; }

    public string? PackageName { get; private set; }

    public ThunderstorePackageArchive(FileInfo archivePath)
    {
        ArchivePath = archivePath ?? throw new ArgumentException("Package archive must be a file.");
        ReadMetadata();
    }

    public string PackageIdentifier => $"{PackageNamespace}-{PackageName}";

    [MemberNotNullWhen(true, nameof(ArchivePath))]
    public bool ArchiveIsValid => ArchivePath is { Exists: true };

    [MemberNotNull(nameof(ArchivePath))]
    private void EnsureValidArchive()
    {
        if (ArchiveIsValid) return;
        throw new ArgumentException("Package archive filepath does not exist.");
    }

    private void ReadMetadata()
    {
        EnsureValidArchive();
        using var archiveHandle = ZipFile.OpenRead(ArchivePath.FullName);

        var manifestEntry = archiveHandle.GetEntry("manifest.json");
        if (manifestEntry is null)
            throw new ArgumentException("Package archive manifest.json could not be found.");

        using var manifestHandle = new StreamReader(manifestEntry.Open());
        JsonNode? manifestRoot = JsonNode.Parse(manifestHandle.ReadToEnd());
        if (manifestRoot is not JsonObject manifestObject)
            throw new InvalidOperationException("Manifest does not contain a valid JSON object.");

        if (!(manifestObject["namespace"] is JsonValue namespaceValue && namespaceValue.TryGetValue<string>(out var @namespace)))
            throw new InvalidOperationException("Manifest 'namespace' string is missing or of invalid type.");
        PackageNamespace = @namespace;

        if (!(manifestObject["name"] is JsonValue nameValue && nameValue.TryGetValue<string>(out var name)))
            throw new InvalidOperationException("Manifest 'name' string is missing or of invalid type.");
        PackageName = name;
    }

    public void StageToProfile(DirectoryInfo profilePath)
    {
        EnsureValidArchive();

        DirectoryInfo defaultLocation = profilePath.CreateSubdirectory(Rules.DefaultLocationRule.GetEffectiveRoute(this));
        using var archiveHandle = ZipFile.OpenRead(ArchivePath.FullName);

        foreach (var archiveEntry in archiveHandle.Entries) {
            ExtractWithInstallRules(archiveEntry);
        }

        void ExtractWithInstallRules(ZipArchiveEntry entry)
        {
            Serilog.Log.Debug("Now considering {Entry}", entry);
            if (TryMatchAndApplyRouteRule(entry)) return;
            if (TryMatchAndApplyImplicitRule(entry)) return;
            FlattenIntoDefaultLocation(entry);
        }

        bool TryMatchAndApplyRouteRule(ZipArchiveEntry entry)
        {
            var ruleMatch = Rules.MatchRouteRule(entry);
            if (ruleMatch is null) return false;

            Serilog.Log.Debug("Matched rule by path: {Rule}", ruleMatch);

            var relativeExtractFilePath = Path.Join(ruleMatch.GetEffectiveRoute(this), entry.FullName.Substring(ruleMatch.Route.Length));
            var extractDirectoryPath = profilePath.CreateSubdirectory(Path.GetDirectoryName(relativeExtractFilePath)!);
            var extractFilePath = Path.Join(extractDirectoryPath.FullName, entry.Name);
            entry.ExtractToFile(extractFilePath, true);
            Serilog.Log.Debug("Wrote to {extractFilePath}", extractFilePath);
            return true;
        }

        bool TryMatchAndApplyImplicitRule(ZipArchiveEntry entry)
        {
            var ruleMatch = Rules.MatchImplicitRule(entry);
            if (ruleMatch is null) return false;

            Serilog.Log.Debug("Matched rule by file extension: {Rule}", ruleMatch);

            var extractDirectoryPath = profilePath.CreateSubdirectory(ruleMatch.GetEffectiveRoute(this));
            var extractFilePath = Path.Join(extractDirectoryPath.FullName, entry.Name);
            entry.ExtractToFile(extractFilePath, true);
            Serilog.Log.Debug("Wrote to {extractFilePath}", extractFilePath);
            return true;
        }

        void FlattenIntoDefaultLocation(ZipArchiveEntry entry)
        {
            Serilog.Log.Debug("No rule matched. Flattening into {DefaultLocation}", defaultLocation);
            entry.ExtractToFile(Path.Join(defaultLocation.FullName, entry.Name), true);
        }
    }
}
