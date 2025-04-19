public class FileExplorer
{
    private static int selectedIndex = 0;
    private static DirectoryInfo currentDirectory = new DirectoryInfo(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName);
    private static string lastVisitedFolderName = null;


    public static string Browse()
    {
        ConsoleKey key;
        do
        {
            Console.Clear();
            var entries = GetEntries();
            SyncSelectionWithLastVisited(entries);
            DisplayEntries(entries);

            key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.W:
                    selectedIndex = Math.Max(0, selectedIndex - 1);
                    break;

                case ConsoleKey.S:
                    selectedIndex = Math.Min(entries.Length - 1, selectedIndex + 1);
                    break;

                case ConsoleKey.A:
                    if (currentDirectory.Parent != null)
                    {
                        lastVisitedFolderName = currentDirectory.Name;
                        currentDirectory = currentDirectory.Parent;
                    }

                    break;

                case ConsoleKey.D:
                    try
                    {
                        if (entries[selectedIndex] is DirectoryInfo dir)
                        {
                            currentDirectory = dir;
                            lastVisitedFolderName = null;
                            selectedIndex = 0;
                        } 
                    }
                    catch 
                    {
                    }
    

                    break;

                case ConsoleKey.Spacebar:
                    if (entries.Length > 0)
                        return entries[selectedIndex].FullName;
                    break;
            }
        } while (true);
    }

    private static FileSystemInfo[] GetEntries()
    {
        var dirs = currentDirectory.GetDirectories();
        var files = currentDirectory.GetFiles("*.java");
        return dirs.Cast<FileSystemInfo>().Concat(files).OrderBy(f => f.Name).ToArray();
    }

    private static void DisplayEntries(FileSystemInfo[] entries)
    {
        Console.WriteLine($"📁 Current: {currentDirectory.FullName}");
        Console.WriteLine("Navigate with W/S (up/down), A (back), D (space), Enter to select a .java file\n");

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            var isSelected = i == selectedIndex;

            if (isSelected)
                Console.BackgroundColor = ConsoleColor.DarkGray;

            if (entry is DirectoryInfo)
                Console.WriteLine($"📂 {entry.Name}");
            else
                Console.WriteLine($"📄 {entry.Name}");

            if (isSelected)
                Console.ResetColor();
        }
    }

    private static void SyncSelectionWithLastVisited(FileSystemInfo[] entries)
    {
        if (lastVisitedFolderName == null) return;

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Name == lastVisitedFolderName)
            {
                selectedIndex = i;
                break;
            }
        }

        lastVisitedFolderName = null;
    }
}