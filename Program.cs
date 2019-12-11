using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace compare
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length==0){
                Console.WriteLine($"Specify folder path and options. Select 1 - for duplicate folders, 2 for duplicate files, 3 for both.");
                return;
            }

            DirectoryInfo di = null;
            if (!string.IsNullOrEmpty(args[0])){
                di = new DirectoryInfo(args[0]);
                if (!di.Exists){
                    Console.WriteLine($"Cannot find folder: {di.FullName}");
                    return;
                }
            }

            int option=0;
            if (args.Length>1){
                int.TryParse(args[1], out option);
            }

            if (option==0){
                Console.WriteLine($"Positional parameter option not specified. Select 1 - for duplicate folders, 2 for duplicate files, 3 for both.");
                return;
            }

            var allDirectories = di.GetDirectories("*", SearchOption.AllDirectories);
            var allFiles = di.GetFiles("*", SearchOption.AllDirectories);

            Console.WriteLine();
            Console.ForegroundColor=ConsoleColor.Green;
            Console.WriteLine($"Working.");
            Console.ResetColor();
            Console.WriteLine($"   {allDirectories.Count()} Directories");
            Console.WriteLine($"   {allFiles.Count()} Files");
            Console.WriteLine();

            // files
            List<dynamic> hs = new List<dynamic>();
            foreach (var f in allFiles)
            {
                using (var hash = MD5.Create())
                {
                    try{
                        var h = hash.ComputeHash(f.OpenRead());
                        var ha = BitConverter.ToString(h).Replace("-", "");
                        hs.Add(new { H = ha, F = f });
                    }
                    catch{
                        Console.WriteLine($"Error - cannot read {f.FullName}");
                    }
                }
            }

            // folders
            List<dynamic> ds = new List<dynamic>();
            foreach (var d in allDirectories)
            {
                var dHashes = hs.Where(x=>((FileInfo)x.F).DirectoryName == d.FullName).Select(x=>(string)x.H).OrderBy(x=>x);
                if (dHashes.Any()){
                    var sh = string.Join("", dHashes);
                    ds.Add(new{D=d,H=sh});
                }
            }

            if (option==1 || option==3){
                // print folders
                var o = ds.GroupBy(x=>x.H).Where(x=>x.Count()>1);
                foreach (var d in o)
                {
                    Console.WriteLine();
                    foreach (var f in d)
                    {
                        Console.WriteLine($"{f.D.FullName}");
                    }
                }
            }

            if (option==2 || option==3){
                // print files
                var ordered = hs.GroupBy(x => x.H).Where(g => g.Count() > 1);
                foreach (var g in ordered)
                {
                    Console.WriteLine(g.First().H);
                    foreach (dynamic f in g)
                    {
                        Console.WriteLine($"\t{f.F.FullName}");
                    }
                }
            }
        }
    }
}
