using Fbox.Shared.Services;

var service = new FileCollectionService();

if (args.Length == 0)
{
    PrintUsage();
    return 0;
}

var command = args[0].ToLowerInvariant();

switch (command)
{
    case "list":
        var showNew = args.Length > 1 && args[1] is "--new" or "-n";
        if (showNew)
        {
            var files = service.GetNewFiles();
            if (files.Count == 0)
            {
                Console.Error.WriteLine("暂无新文件");
                return 1;
            }
            foreach (var f in files)
                Console.WriteLine(f.Path);
        }
        else
        {
            var files = service.GetAllFiles();
            if (files.Count == 0)
            {
                Console.Error.WriteLine("收集箱为空");
                return 1;
            }
            foreach (var f in files)
                Console.WriteLine(f.Path);
        }
        return 0;

    case "count":
        var (total, unseen) = service.GetCount();
        Console.Error.WriteLine($"总数: {total}  |  未读: {unseen}");
        return 0;

    case "paths":
        var allPaths = service.GetPaths();
        if (allPaths.Count == 0)
        {
            Console.Error.WriteLine("收集箱为空");
            return 1;
        }
        foreach (var p in allPaths)
            Console.WriteLine(p);
        return 0;

    case "clear":
        service.Clear();
        Console.Error.WriteLine("已清空收集箱");
        return 0;

    default:
        PrintUsage();
        return 1;
}

static void PrintUsage()
{
    Console.Error.WriteLine("""
        fbox - 文件收集箱 CLI
        用法:
          fbox list            显示全部文件路径（标记已读）
          fbox list --new      仅显示未读文件路径（标记已读）
          fbox count           显示总数和未读数
          fbox paths           显示全部路径（不标记已读）
          fbox clear           清空收集箱
        """);
}
