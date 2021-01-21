using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    class ServerData
    {
       public string host;
       public int port;
       public string name;
       public List<Client> clients;
        public ServerData(string host,int port) {
            this.host = host;
            this.port = port;
            
        }
        public ServerData(string host, int port,string name)
        {
            this.host = host;
            this.port = port;
            this.name = name;

        }
        public void SetClients(string[] id,string[] name,string[] status,string[] room) {
            clients = new List<Client>();
            
            for (int i=0; i < id.Length;i++) {
               clients.Add(new Client(name[i],int.Parse(id[i]),status[i],room[i]));
             }

        }
        public void AddClient(string name,int id,string status,string room) {
            if (clients == null) return;
            clients.Add(new Client(name,id,status,room));
        }
        public void RemoveClient(int id) {
            if (clients == null) return;
            for (int i=0;i<clients.Count;i++) {
                if (clients[i].user.id == id) {clients[i].Stop(); clients.RemoveAt(i); }
            }
        }
        public Client GetClient(int id) {
            if (clients == null) return null;
            
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].user.id == id)return clients[i];
            }
            return null;
        }
    }
}
