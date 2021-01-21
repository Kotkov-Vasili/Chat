using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    //Типы сообщений
    public enum MessageType
    {
        conn = 1, disconn = 2, mess = 3, voice = 4 , online = 5,file_start = 6,file_data=7,file_end=8,command = 9,
    }
    //Фрагмент класса собщения
    class Message
    {
        public MessageType type;//тип сообщений
        byte[] data;//данные
        public ushort sender;//размер
        public ushort lenght;//отправитель

        public Message(byte[] mess,ushort lenght,ushort sender,ushort type) {
            data = new byte[lenght + 6];
            this.lenght = lenght;
            this.sender = sender;
            this.type = (MessageType)type;
            byte[] blenght = BitConverter.GetBytes(lenght);
            byte[] bsender = BitConverter.GetBytes(sender);
            byte[] btype = BitConverter.GetBytes((ushort)this.type);
            data[0] = blenght[0]; data[1] = blenght[1]; data[2] = bsender[0]; data[3] = bsender[1]; data[4] = btype[0]; data[5] = btype[1];
            for (int i = 0; i < lenght; i++)
            {
                data[i + 6] = mess[i];
            }
        }
        public Message(MessageType type,ushort sender,byte[] mess) {
            this.type = type;
            lenght = (ushort)mess.Length;
            this.sender = sender;
            data = new byte[lenght+6];
            byte[] blenght = BitConverter.GetBytes(lenght);
            byte[] bsender = BitConverter.GetBytes(this.sender);
            byte[] btype = BitConverter.GetBytes((ushort)this.type);
            data[0] = blenght[0]; data[1] = blenght[1]; data[2] = bsender[0]; data[3] = bsender[1]; data[4] = btype[0]; data[5] = btype[1];
            for (int i=0;i<lenght;i++) {
                data[i + 6] = mess[i];
            }
        }
        public Message(MessageType type, ushort sender, string msg)
        {
            byte[] mess = Encoding.Unicode.GetBytes(msg);
            this.type = type;
            lenght = (ushort)mess.Length;
            this.sender = sender;
            data = new byte[lenght + 6];
            byte[] blenght = BitConverter.GetBytes(lenght);
            byte[] bsender = BitConverter.GetBytes(this.sender);
            byte[] btype = BitConverter.GetBytes((ushort)this.type);
            data[0] = blenght[0]; data[1] = blenght[1]; data[2] = bsender[0]; data[3] = bsender[1]; data[4] = btype[0]; data[5] = btype[1];
            for (int i = 0; i < lenght; i++)
            {
                data[i + 6] = mess[i];
            }
        }
        public string GetString()
        {
          
            string str =  Encoding.Unicode.GetString(data, 6, data.Length-6);
            return str;
        }
        public Config GetConfig() {
            Config conf = new Config(GetString());
            return conf;
        }
        public byte[] GetData()
        {
            byte[] bytes = new byte[lenght];
            Array.Copy(data, 6, bytes, 0, lenght);
            return bytes;
        }
        public byte[] GetFullData()
        {

            return data;
        }
         
    }
}
