using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    class Client
    {
        //Данные пользователя: id ,ник и другое
        public UserData user;

        //Обьекты для воспроизведения звука
        WaveOut output;
        public BufferedWaveProvider bufferStream;

        public Client(string name, int id, string status, string room) {
            user = new UserData(name,id,status,room);
            output = new WaveOut();//Данные пользователя: id ,ник и другое
            bufferStream = new BufferedWaveProvider(new WaveFormat(16000, 16, 1));//Создаётся аудиопоток
            output.Init(bufferStream);//Подключение аудиопотока
            output.Play();//Начало вопроизведения
        }
        public void Stop() {
            if (output != null)
            {
                output.Stop();
                output.Dispose();
                output = null;
            }
            bufferStream = null; 
        }
        
    }
}
