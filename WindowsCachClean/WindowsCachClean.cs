string userProfilePath =
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string recentDirectory = userProfilePath + "\\Recent";
string tempDirectory = userProfilePath + "\\AppData\\Local\\Temp";
string prefetchDirectory = @"C:\Windows\Prefetch\";


try
{
    //delete recent folder
    Console.WriteLine("----------Starting Delete Recent Folder----------");
    DeleteFile(recentDirectory, "*");
    DeleteFolder(recentDirectory, "*");

    //delete temp folder
    Console.WriteLine("----------Starting Delete Temp Folder----------");
    DeleteFile(tempDirectory, "*.tmp");
    DeleteFolder(tempDirectory, "*");

    //delete prefetch folder
    Console.WriteLine("----------Starting Delete Prefetch Folder----------");
    DeleteFile(prefetchDirectory, "*.pf");
    DeleteFolder(prefetchDirectory, "*");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

void DeleteFile(string path, string extention)
{
    try
    {
        foreach (string file in Directory.GetFiles(path, extention))
        {
            if (IsFileBusy(file))
            {
                File.Delete(file);
                Console.WriteLine($"Delete File:{file}", file);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

void DeleteFolder(string path, string extention)
{
    try
    {
        if (Directory.Exists(path))
        {
            string[] directories = Directory.GetDirectories(
                tempDirectory, extention, SearchOption.AllDirectories
                );

            foreach (string directory in directories)
            {
                if (IsFolderBusy(directory))
                {
                    Console.WriteLine($"Delete Folde:{directory}", directory);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

bool IsFileBusy(string file)
{
    bool isBusy = true;

    try
    {
        FileStream fs =
            File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        fs.Close();
    }
    catch (IOException ex)
    {
        if (ex != null)
        {
            isBusy = false;
        }
    }

    return isBusy;
}

bool IsFolderBusy(string file)
{
    bool isBusy = true;

    try
    {
        DirectoryInfo directory = new DirectoryInfo(file);
        Directory.Delete(file, true);
    }
    catch (UnauthorizedAccessException uex)
    {
        if (uex != null)
        {
            isBusy = false;
        }
    }
    catch (DirectoryNotFoundException dex)
    {
        if (dex != null)
        {
            isBusy = false;
        }
    }
    catch (IOException iex)
    {
        if (iex != null)
        {
            isBusy = false;
        }
    }

    return isBusy;
}


