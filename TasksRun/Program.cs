using System.Diagnostics;
using System.Threading.Tasks;

internal class Program
{
    public class Result
    {
        public string? fileName;
        public int? count;
        public long ms;
        public string? errorMessage;
    }
    public static  void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Укажите путь к текстовым файлам");
            Console.WriteLine("Пример: TasksRun c:\\windows\\inf");
            return;
        }
        DirectoryInfo info = new DirectoryInfo(args[0]);
        var files = info.GetFiles();
        if (files.Length < 3)
        {
            Console.WriteLine($"Слишком мало файлов ({files.Length}). Их должно быть больше 3х");
            return;
        }
        //1. Прочитать 3 файла параллельно и вычислить количество пробелов в них (через Task).
        Console.WriteLine("Три случайных файла:\n");

        Random rng = new ();
        var tasks = new Task<Result>[3];
        var filesToTry = new FileInfo[3];
        var stopWatch = new Stopwatch();
        var fileCount = files.Length;
        stopWatch.Start();
        var i = 0;
        foreach (var t in filesToTry)
        {
            var file = files[rng.Next(fileCount)];
            filesToTry[i] = file;
            tasks[i] = Task.Run(() =>
            {
                return  ReadFileAndCountSpaces(file.FullName);
            });
            i++;
        }
        Task.WaitAll(tasks);
        WriteResult(stopWatch);
        Console.WriteLine($"Сумма всех времен всех задач (ms):{tasks.Sum(_ => _.Result.ms)}");



       Console.WriteLine("Все файлы каталога:\n");
        ReadAllFilesFromFolderAndCalcSpaces(args[0]);
    }


    public static void WriteResult(Stopwatch stopWatch) {
        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        Console.WriteLine($"Параллельное чтение файлов заняло время  (ms) {stopWatch.ElapsedMilliseconds}.");
    }

    /// <summary>
    /// Написать функцию, принимающую в качестве аргумента путь к папке. Из этой папки параллельно прочитать все файлы 
    /// и вычислить количество пробелов в них.
    /// </summary>
    /// <param name="path"></param>
    public static void ReadAllFilesFromFolderAndCalcSpaces(string path) {
        DirectoryInfo info = new DirectoryInfo(path);
        var files = info.GetFiles();
        var fileCount = files.Length;
        var i = 0;
        var tasksAll = new Task<Result>[fileCount];
        Stopwatch stopWatch = new ();
        stopWatch.Start ();
        foreach (var file in files)
        {
            tasksAll[i++] = Task.Run(() =>
            {
                return ReadFileAndCountSpaces(file.FullName);
            });
        }
        Task.WaitAll(tasksAll);
        WriteResult(stopWatch);
        Console.WriteLine($"Сумма всех времен всех задач (ms):{tasksAll.Sum(_ => _.Result.ms)}. ");

    }

    public static   Result ReadFileAndCountSpaces(string filePath) {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        Result result = new () { fileName = filePath};
        try
        {
            using StreamReader reader = new(filePath);
            result.count =   reader.ReadToEnd().Count(x => x == ' ');
            return result;

        }
        catch (IOException e)
        {
            result.errorMessage=(e.Message);
            return result;
        }
        finally{
            stopWatch.Stop();
            result.ms = stopWatch.ElapsedMilliseconds;
            Console.WriteLine($"Файл: {filePath}. Пробелов: {result.count}. Время (ms) {stopWatch.ElapsedMilliseconds}.");
            if (!string.IsNullOrWhiteSpace(result.errorMessage))
            {
                Console.WriteLine($" Ошибка: {result.errorMessage}.");
            }

        }
    }
  
}