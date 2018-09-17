using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace BigBoi
{
    [Serializable]
    public class BigBoiFile
    {

        private String name;
        private FileAttributes attributes;
        private String[] contents;

        public BigBoiFile(FileInfo file)
        {
            name = file.Name;
            attributes = file.Attributes;
            contents = Read(file);
            Console.Out.WriteLine("    File " + name + " has been stored");
        }

        public void Create(String dir)
        {
            string pathString = Path.Combine(dir, name);
            File.WriteAllLines(pathString, contents);
            File.SetAttributes(pathString, attributes);
        }

        private String[] Read(FileInfo file)
        {
            String[] contents = File.ReadAllLines(file.FullName);
            return contents;
        }

        public String Name()
        {
            return name;
        }

    }
}
