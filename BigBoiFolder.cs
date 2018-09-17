using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace BigBoi
{
    [Serializable]
    class BigBoiFolder
    {
        private String name;
        private BigBoiFile[] files;
        private BigBoiFolder[] folders;
        private FileAttributes attributes;

        public BigBoiFolder(DirectoryInfo dir)
        {
            Console.Out.WriteLine("Beginning to Map " + dir.FullName);
            name = dir.Name;
            attributes = dir.Attributes;

            FileInfo[] subFiles = dir.GetFiles();
            DirectoryInfo[] subDirs = dir.GetDirectories();

            files = new BigBoiFile[subFiles.Length];
            folders = new BigBoiFolder[subDirs.Length];

            int count = 0;
            foreach(FileInfo file in subFiles)
            {
                files[count] = new BigBoiFile(file);
                count++;
            }
            count = 0;
            foreach(DirectoryInfo dirs in subDirs)
            {
                folders[count] = new BigBoiFolder(dirs);
                count++;
            }
            Console.Out.WriteLine(dir.FullName + " has been mapped");
        }

        public String Name()
        {
            return name;
        }

        public void Create(String dir)
        {
            Console.Out.WriteLine("Beginning to create folder " + name + " in " + dir);
            //Place the folder inside the cwd
            String pathName = Path.Combine(dir, name);
            
            //If it exists we recursively delete everything inside.
            if (Directory.Exists(pathName))
                Program.Purge(new DirectoryInfo(pathName));

            //Create our Directory
            Directory.CreateDirectory(pathName);

            File.SetAttributes(pathName, attributes);

            //Generate our Files
            foreach(BigBoiFile file in files)
            {
                Console.Out.WriteLine("Beginning to create " + file.Name());
                file.Create(pathName);
            }

            //Then repeat the process on the rest of the folders
            foreach(BigBoiFolder folder in folders)
            {
                folder.Create(pathName);
            }
        }
    }
}
