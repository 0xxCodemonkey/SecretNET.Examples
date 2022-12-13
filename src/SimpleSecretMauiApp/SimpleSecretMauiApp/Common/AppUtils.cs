namespace SimpleSecretMauiApp.Common;

public static class AppUtils
{

    /// <summary>
    /// Writes the file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="data">The data.</param>
    /// <param name="directory">The directory.</param>
    public static async Task WriteFile(string fileName, string data, string directory = null)
    {
        directory = directory ?? FileSystem.Current.AppDataDirectory;

        // Write the file content to the app data directory
        string targetFile = Path.Combine(directory, fileName);

        using FileStream outputStream = File.OpenWrite(targetFile);
        using StreamWriter streamWriter = new StreamWriter(outputStream);

        await streamWriter.WriteAsync(data);
    }

    /// <summary>
    /// Reads the file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="directory">The directory.</param>
    /// <returns>System.String.</returns>
    public static async Task<string> ReadFile(string fileName, string directory = null)
    {
        directory = directory ?? FileSystem.Current.AppDataDirectory;

        // Write the file content to the app data directory
        string targetFile = Path.Combine(directory, fileName);
        
        if (File.Exists(targetFile))
        {
            return await File.ReadAllTextAsync(targetFile);
        } 

        return null;
    }
}
