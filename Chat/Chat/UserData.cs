using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    class UserData
    {
       public string name;
       public int id;
       public bool accepted;
        public string pass;
        public string status;
        public string room;
        public UserData(string name) {
            this.name = name;
        }
        public UserData(string name,int id,string status,string room)
        {
            this.name = name;
            this.id = id;
        }

    }
}
