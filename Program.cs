using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.IO.Compression;

namespace BigBoi
{
    class Program
    {
        public static String version = "0.1.0";
        public static BigBoiFolder[] container;
        public static DirectoryInfo configs = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+"\\BigBoi");
        public static DirectoryInfo wow = LoadWoW();

        static void Main(string[] args)
        {
            if (!configs.Exists)
            {
                configs.Create();
            }
            Console.Out.WriteLine("BigBoi " + version);
            Console.Out.WriteLine("Options");
            Console.Out.WriteLine("0 - Create new Backup");
            Console.Out.WriteLine("1 - Load Backup");
            ConsoleKeyInfo keyInput = Console.ReadKey();
    
            if (keyInput.Key.Equals(ConsoleKey.D0))
            {
                Console.Clear();
                container = new BigBoiFolder[] { LoadFile(new DirectoryInfo(wow.FullName + "\\WTF")), LoadFile(new DirectoryInfo(wow.FullName + "\\Interface")) };
                Console.Out.WriteLine(container[0].Name() + " has been loaded");
                Console.Out.WriteLine(container[1].Name() + " has been loaded");
                SerializeFolder(container, configs);
                Console.Out.WriteLine("Press any key to quit");
                Console.ReadKey();
            }
            if (keyInput.Key.Equals(ConsoleKey.D1))
            {
                Console.Clear();
                FileInfo[] files = configs.GetFiles();
                List<FileInfo> raw = new List<FileInfo>();
                int counter = 0;

                foreach (FileInfo file in files)
                {
                    if (file.Name.Contains(".bbs"))
                    {
                        raw.Add(file);
                        counter++;
                    }
                }
                Console.Out.WriteLine("Which Backup would you like to restore?");

                FileInfo[] backups = new FileInfo[counter];
                counter = 0;

                foreach (FileInfo backup in raw)
                {
                    Console.Out.WriteLine(counter + " - " + backup.Name);
                    backups[counter] = backup;
                    counter++;
                }

                bool alive = true;

                while (alive)
                {
                    String input0 = Console.In.ReadLine();
                    int index;
                    if (int.TryParse(input0, out index))
                    {
                        if (!(index > counter))
                        {
                            container = DeSerializeFolder(backups[index]);
                            alive = false;
                        }
                        else
                        {
                            Console.Out.WriteLine("Incorrect Selection. Please try again");
                        }

                    }
                    else
                    {
                        Console.Out.WriteLine("Please use a number from above");
                    }
                }

                foreach(BigBoiFolder folder in container)
                {
                    folder.Create(wow.FullName);
                }

                Console.Out.WriteLine("You have successfully restored your config");
                Console.Out.WriteLine("Press any key to Quit");
                Console.ReadKey();
            }
            if (keyInput.Key.Equals(ConsoleKey.J))
            {
                Console.Out.WriteLine("Enter the Dir Below you want to Serialize");
                String dir = Console.In.ReadLine();
                Console.Out.WriteLine("The Dir you want to output the File");
                String file = Console.In.ReadLine();
                BigBoiFolder[] folder = new BigBoiFolder[] { new BigBoiFolder(new DirectoryInfo(dir)) };
                SerializeFolder(folder, new DirectoryInfo(file));

            }
        }

        public static BigBoiFolder LoadFile(DirectoryInfo dir)
        {
            Console.Out.WriteLine("Beginning to load " + dir.FullName);
            BigBoiFolder folder = new BigBoiFolder(dir);
            Console.Out.WriteLine(dir.FullName + " has been loaded successfully");
            return folder;
        }

        public static DirectoryInfo LoadWoW()
        {
            String[] drives = Environment.GetLogicalDrives();
            foreach (String drive in drives)
            {
                DriveInfo di = new DriveInfo(drive);
                DirectoryInfo temp = SniffWow(di.RootDirectory);
                if (temp != null)
                {
                    return temp;
                }
            }
            return null;
        }

        public static void Purge(DirectoryInfo dir)
        {
            FileInfo[] subFiles = dir.GetFiles();
            DirectoryInfo[] subFolders = dir.GetDirectories();

            foreach(FileInfo file in subFiles)
            {
                file.Delete();
            }

            foreach(DirectoryInfo folder in subFolders)
            {
                Purge(folder);
            }
            dir.Delete();
        }

        public static BigBoiFolder[] DeSerializeFolder(FileInfo file)
        {
            Decompress(file);
            IFormatter formatter = new BinaryFormatter();
            FileInfo file0 = new FileInfo(file.FullName.Remove(file.FullName.Length-3));
            Console.Out.WriteLine("Beginning to Deserialize " + file0.Name);
            Stream stream = new FileStream(file0.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
            BigBoiFolder[] folder = (BigBoiFolder[]) formatter.Deserialize(stream);
            Console.Out.WriteLine(file.Name + "has been Deserialized");
            stream.Dispose();
            file0.Delete();
            return folder;
        }

        public static FileInfo SerializeFolder(BigBoiFolder[] folder, DirectoryInfo loc)
        {
            IFormatter formatter = new BinaryFormatter();
            Console.Out.WriteLine("What would you like to name your Backup?");
            String input = Console.In.ReadLine();
            FileInfo file = new FileInfo(loc.FullName + "\\" + input + ".bbs");
            Stream stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, folder);
            Console.Out.WriteLine("Your Backup: " + input + " has been saved");
            stream.Dispose();
            Compress(file);
            file.Delete();
            return file;
        }

        public static DirectoryInfo SniffWow(DirectoryInfo startDir)
        {
            System.IO.DirectoryInfo[] subFolders = startDir.GetDirectories();
            foreach (System.IO.DirectoryInfo dir in subFolders)
            {
                if (dir.Name.Contains("World of Warcraft"))
                {
                    return dir;
                }

                if (dir.Name.Contains("Program Files"))
                {
                    SniffWow(dir);
                }
            }
            return null;
        }

        public static void Compress(FileInfo fi)
        {
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Prevent compressing hidden and 
                // already compressed files.
                if ((File.GetAttributes(fi.FullName)
                    & FileAttributes.Hidden)
                    != FileAttributes.Hidden & fi.Extension != ".gz")
                {
                    // Create the compressed file.
                    using (FileStream outFile =
                                File.Create(fi.FullName + ".gz"))
                    {
                        using (GZipStream Compress =
                            new GZipStream(outFile,
                            CompressionMode.Compress))
                        {
                            // Copy the source file into 
                            // the compression stream.
                            inFile.CopyTo(Compress);

                            Console.WriteLine("Compressed {0} from {1} to {2} MB.",
                                fi.Name, (fi.Length/1024)/1024, (outFile.Length/1024)/1024);
                        }
                    }
                }
            }
        }

        public static void Decompress(FileInfo fi)
        {
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Get original file extension, for example
                // "doc" from report.doc.gz.
                string curFile = fi.FullName;
                string origName = curFile.Remove(curFile.Length -
                        fi.Extension.Length);

                //Create the decompressed file.
                using (FileStream outFile = File.Create(origName))
                {
                    using (GZipStream Decompress = new GZipStream(inFile,
                            CompressionMode.Decompress))
                    {
                        // Copy the decompression stream 
                        // into the output file.
                        Decompress.CopyTo(outFile);

                        Console.WriteLine("Decompressed: {0}", fi.Name);

                    }
                }
            }
        }

    }
}
