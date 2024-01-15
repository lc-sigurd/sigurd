# SigurdLib

## For Players

## For Developers

## Contributing

You'll need to:
1. Fork and clone. (We use GitHub Flow)
2. Duplicate the `SigurdLib.template.props.user` file and rename it to
`SigurdLib.props.user`.
3. Edit the properties defined in the new file to match your environment.
   Most importantly, set the `<LethalCompanyDir>` property to the
   filepath of your Lethal Company installation folder (including trailing
   path separator `/` or `\`).
4. `dotnet tool restore`
5. Attempt build
