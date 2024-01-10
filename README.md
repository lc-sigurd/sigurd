# SigurdLib

## For Players

## For Developers

## Contributing

### `SigurdLib.props.user`

You'll need to create an MSBuild props file to tell the solution where Lethal Company resides
on your PC. Here's a template for that file, which should be called `SigurdLib.props.user` and
placed into the repository root:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
    <PropertyGroup>
        <LethalCompanyDir>/home/joeclack/.steam/debian-installation/steamapps/common/Lethal Company/</LethalCompanyDir>
        <TestProfileDir>/home/joeclack/.config/r2modmanPlus-local/LethalCompany/profiles/Test Sigurd/</TestProfileDir>
    </PropertyGroup>

    <!-- Enable by setting the Condition attribute to "true". *nix users should switch out `copy` for `cp`. -->
    <Target Name="CopyToTestProfile" DependsOnTargets="NetcodePatch" AfterTargets="PostBuildEvent" Condition="false">
        <MakeDir
            Directories="$(TestProfileDir)BepInEx/plugins/Sigurd-Sigurd/Sigurd"
            Condition="!Exists('$(TestProfileDir)BepInEx/plugins/Sigurd-Sigurd/Sigurd')"
        />
        <Exec Command="copy &quot;$(TargetPath)&quot; &quot;$(TestProfileDir)BepInEx/plugins/Sigurd-Sigurd/Sigurd/&quot;" />
    </Target>
</Project>
```

