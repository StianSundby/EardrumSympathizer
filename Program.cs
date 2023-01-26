using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
#pragma warning disable CA1416
#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604

namespace EardrumSympathizer
{
    public class Program
    {
        private static bool _initialized = false;
        private static string? _logData = string.Empty;
        private static readonly List<SteamGame?> _subKeyValues = new();

        public static void Main()
        {
            if (!_initialized) Init();

            ConsoleSpiner spin = new ConsoleSpiner();
            Console.WriteLine("Press any key to exit");
            Console.Write("Running....");
            bool keyPressed = false;
            while (!keyPressed)
            {
                if (Console.KeyAvailable)
                    keyPressed = true;
                GetKeys(@"Software\Valve\Steam\Apps\");
                foreach (SteamGame game in _subKeyValues)
                {
                    if (game.Running == 0) break;
                    if (_logData.ToLower().Contains(game.Name.ToLower())) break;
                    var allProcesses = Process.GetProcesses();
                    foreach (var process in allProcesses)
                    {
                        if (process.ProcessName.ToLower() != game.Name.ToLower()) break;
                        //find process
                        //lower volume - add to log
                    }
                }
                //update logfile and _logData
                spin.Turn();
                Thread.Sleep(1000);
            }
        }

        private static void Init()
        {
            PrintLogo();
            Console.ReadKey(false);
            _logData = ReadLog();
            _initialized = true;
        }

        public static string ReadLog()
        {
            WriteLine("\n\nLocating prlog.json", null);
            var path = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location)!, "prlog.json");

            if (!File.Exists(path))
            {
                WriteLine("Couldn't find prlog.json.", MessageColor.Red);
                WriteLine("Creating a new prlog.json at " + path.ToString(), null);
                File.Create(path).Close();
            }

            WriteLine("Reading prlog.json", null);
            var logTxt = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(logTxt))
                return JsonSerializer.Deserialize<string>(logTxt);
            return string.Empty;
        }

        public static void GetKeys(string path)
        {
            var keys = Registry.CurrentUser.OpenSubKey(path);
            string[] valueNames = keys.GetSubKeyNames();
            keys.Close();
            GetSubKeyValues(path, valueNames);
        }

        public static bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;
            if (file1 == file2) return true;

            fs1 = new FileStream(file1, FileMode.Open);
            fs2 = new FileStream(file2, FileMode.Open);

            if (fs1.Length != fs2.Length)
            {
                fs1.Close();
                fs2.Close();
                return false;
            }
            do
            {
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            } while ((file1byte == file2byte) && (file1byte != -1));

            fs1.Close();
            fs2.Close();
            return ((file1byte - file2byte) == 0);
        }

        public static void GetSubKeyValues(string path, string[] valueNames)
        {
            foreach (var key in valueNames)
            {
                var tempPath = Path.Combine(path, key);
                var subKey = Registry.CurrentUser.OpenSubKey(tempPath);
                if (subKey != null)
                {
                    if (!string.IsNullOrEmpty((string)subKey.GetValue("Name")))
                    {
                        var installed = subKey.GetValue("Installed");
                        var name = subKey.GetValue("Name");
                        var running = subKey.GetValue("Running");

                        var game = new SteamGame(Convert.ToInt32(installed), name.ToString(), Convert.ToInt32(running));
                        _subKeyValues.Add(game);
                    }
                }
                subKey.Close();
            }
        }

        private static void PrintLogo()
        {
            var header = "\r\n  _____              _                         ____                              _   _     _              \r\n " +
                "| ____|__ _ _ __ __| |_ __ _   _ _ __ ___    / ___| _   _ _ __ ___  _ __   __ _| |_| |__ (_)_______ _ __ \r\n |  _| / _` | '__/ _` " +
                "| '__| | | | '_ ` _ \\   \\___ \\| | | | '_ ` _ \\| '_ \\ / _` | __| '_ \\| |_  / _ \\ '__|\r\n | |__| (_| | | | (_| | |  | |_| | | | " +
                "| | |   ___) | |_| | | | | | | |_) | (_| | |_| | | | |/ /  __/ |   \r\n |_____\\__,_|_|  \\__,_|_|   \\__,_|_| |_| |_|  |____/ \\__, |_| " +
                "|_| |_| .__/ \\__,_|\\__|_| |_|_/___\\___|_|   \r\n                                                     |___/          |_|                 " +
                "                  \r\n";
            Console.Write(header);
            Console.WriteLine("Press any key to continue...");
        }



        private static void WriteLine(string msg, MessageColor? color)
        {
            if (color == MessageColor.Red) 
                Console.ForegroundColor = ConsoleColor.Red;
            else if (color == MessageColor.Green) 
                Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);

            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public class SteamGame
    {
        public int Installed { get; set; }
        public string Name { get; set; }
        public int Running { get; set; }
        public SteamGame(int installed, string name, int running)
        {
            Installed = installed;
            Name = name;
            Running = running;
        }
    }

    public class ConsoleSpiner
    {
        int counter;
        public ConsoleSpiner()
        {
            counter = 0;
        }
        public void Turn()
        {
            counter++;
            switch (counter % 4)
            {
                case 0: Console.Write("/"); break;
                case 1: Console.Write("-"); break;
                case 2: Console.Write("\\"); break;
                case 3: Console.Write("|"); break;
            }
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
    }

    public enum MessageColor
    {
        Red,
        Green
    }
}