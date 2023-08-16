using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;


string userProfilePath =
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string recentDirectory =
    Environment.GetFolderPath(Environment.SpecialFolder.Recent);
string tempDirectory = userProfilePath + "\\AppData\\Local\\Temp";
string prefetchDirectory = @"C:\Windows\Prefetch\";


try
{
    //check adminstrator
    RequireAdministrator();

    //delete recent folder
    Console.WriteLine("----------Starting Delete Recent Folder----------");
    SetAccess(recentDirectory);
    DeleteFile(recentDirectory, "*.*");
    DeleteFolder(recentDirectory, "*");
    Console.WriteLine(Environment.NewLine, Environment.NewLine);

    //delete temp folder
    Console.WriteLine("----------Starting Delete Temp Folder----------");
    SetAccess(tempDirectory);
    DeleteFile(tempDirectory, "*.*");
    DeleteFolder(tempDirectory, "*");
    Console.WriteLine(Environment.NewLine, Environment.NewLine);


    //delete prefetch folder
    Console.WriteLine("----------Starting Delete Prefetch Folder----------");
    SetAccess(prefetchDirectory);
    DeleteFile(prefetchDirectory, "*.*");
    DeleteFolder(prefetchDirectory, "*");

    Console.WriteLine(Environment.NewLine, Environment.NewLine);
    Console.WriteLine("Press any key to close the console window...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

[DllImport("libc")]
static extern uint getuid();

void RequireAdministrator()
{
    string name = System.AppDomain.CurrentDomain.FriendlyName;
    try
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    throw new InvalidOperationException($"Application must be run as administrator. Right click the {name} file and select 'run as administrator'.");
                }
            }
        }
        else if (getuid() != 0)
        {
            throw new InvalidOperationException($"Application must be run as root/sudo. From terminal, run the executable as 'sudo {name}'");
        }
    }
    catch (Exception ex)
    {
        throw new ApplicationException("Unable to determine administrator or root status", ex);
    }
}

void SetAccess(string folderPath)
{
    try
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

            if (directoryInfo != null)
            {
                DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
                SecurityIdentifier? currentUser = WindowsIdentity.GetCurrent().User;

                if (directorySecurity != null && currentUser != null)
                {
                    directorySecurity.AddAccessRule(new FileSystemAccessRule(currentUser, FileSystemRights.FullControl, AccessControlType.Allow));
                    directoryInfo.SetAccessControl(directorySecurity);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}


void DeleteFile(string path, string extention)
{
    if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(extention))
    {
        return;
    }

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
    if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(extention))
    {
        return;
    }

    try
    {
        if (Directory.Exists(path))
        {
            string[] directories = Directory.GetDirectories(
                path, extention, SearchOption.AllDirectories
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
    if (string.IsNullOrEmpty(file))
    {
        return false;
    }

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
    if (string.IsNullOrEmpty(file))
    {
        return false;
    }

    bool isBusy = true;

    try
    {
        DirectoryInfo directory = new DirectoryInfo(file);

        if (directory.Exists)
        {
            Directory.Delete(file, true);
        }
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


