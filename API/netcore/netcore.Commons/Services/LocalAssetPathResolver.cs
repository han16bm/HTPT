namespace netcore.Commons.Services;

public static class LocalAssetPathResolver
{
    public static string Resolve(string? configuredRoot, string contentRootPath, string fallbackRelativePath = "assets")
    {
        var root = string.IsNullOrWhiteSpace(configuredRoot)
            ? fallbackRelativePath
            : configuredRoot.Trim();

        if (Path.IsPathRooted(root))
        {
            return Path.GetFullPath(root);
        }

        var normalized = root.Replace('\\', '/').Trim('/');
        if (normalized.Equals("assets", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith("assets/", StringComparison.OrdinalIgnoreCase))
        {
            var apiRoot = FindApiRoot(contentRootPath) ?? FindApiRoot(Directory.GetCurrentDirectory());
            if (apiRoot is not null)
            {
                return Path.GetFullPath(Path.Combine(apiRoot, root));
            }
        }

        if (normalized.Equals("API/assets", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith("API/assets/", StringComparison.OrdinalIgnoreCase))
        {
            var workspaceRoot = FindWorkspaceRoot(contentRootPath) ?? FindWorkspaceRoot(Directory.GetCurrentDirectory());
            if (workspaceRoot is not null)
            {
                return Path.GetFullPath(Path.Combine(workspaceRoot, root));
            }
        }

        return Path.GetFullPath(Path.Combine(contentRootPath, root));
    }

    private static string? FindWorkspaceRoot(string startPath)
    {
        var apiRoot = FindApiRoot(startPath);
        if (apiRoot is null)
        {
            return null;
        }

        var apiDirectory = new DirectoryInfo(apiRoot);
        return string.Equals(apiDirectory.Name, "API", StringComparison.OrdinalIgnoreCase)
            ? apiDirectory.Parent?.FullName
            : apiRoot;
    }

    private static string? FindApiRoot(string startPath)
    {
        var current = new DirectoryInfo(Path.GetFullPath(startPath));
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "API.sln")))
            {
                return current.FullName;
            }

            var childApiRoot = Path.Combine(current.FullName, "API");
            if (File.Exists(Path.Combine(childApiRoot, "API.sln")))
            {
                return childApiRoot;
            }

            current = current.Parent;
        }

        return null;
    }
}
