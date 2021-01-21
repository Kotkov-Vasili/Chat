using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    class FileInfo
    {
        Queue<Message> packets;
        string path;
        string name;
        long lenght;
        byte[] data;
        public FileInfo(string filepath) {
            data = File.ReadAllBytes(filepath);
        }

    }
}
