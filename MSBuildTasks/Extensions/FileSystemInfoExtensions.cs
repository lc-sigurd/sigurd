/*
 * Copyright (c) 2024 The Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System.IO;

namespace MSBuildTasks.Extensions;

public static class FileSystemInfoExtensions
{
    public static bool HasAttributes(this FileSystemInfo info, FileAttributes attributes)
        => (info.Attributes & attributes) == attributes;
}
