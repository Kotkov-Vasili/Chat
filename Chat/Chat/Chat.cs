using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat

{
    enum ResizeType {Left,LeftBottom,Right,RightBottom,Bottom }

    public partial class Chat : Form
    {
        int X, Y;
        bool recording = true;
        bool speaking = true;
        bool connected = false;
        bool canSelectTab = false;
        bool maximized = false;
        bool resize = false;

        ResizeType resizeType;

        UserData user;
        ServerData server;

        static TcpClient client;
        static NetworkStream stream;

        Thread receiveThread;

        WaveIn input;
        WaveOut output;
        BufferedWaveProvider bufferStream;
        
        List<ServerData> servers;
        Crypt crp;
        public Chat()
        {
            InitializeComponent();

            input = new WaveIn();  // WaveIn - поток для записи
            input.WaveFormat = new WaveFormat(16000, 16, 1);//Создаётся обьект запсии в установленным битрейтом,разрядностью и количеством каналов
            input.DataAvailable += Voice_Input;//При записи выполняется метод
            input.DeviceNumber = 0;//Выбрано первое в списке устройство записи
            input.StartRecording();//Начало записи
            
            servers = new List<ServerData>();

          


        }
        public void SetFolders() {

        }
        //При записи данных они приходят в этот метод
        private void Voice_Input(object sender, WaveInEventArgs e)
        {
            try
            {
                
                if (connected &&user.accepted && recording) {
                    Message mess = new Message(MessageType.voice,(ushort)user.id,e.Buffer);
                   
                    Send(mess);
                    //Формируется сообщение и отсылается на сервер
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        private void Turn(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            notifyIcon.ShowBalloonTip(5);
        }

        private void Exit(object sender, EventArgs e)
        {
            connected = false;
            Disconnect();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void TurnLeave(object sender, EventArgs e)
        {
            pictureBox2.Image = Properties.Resources.Turn;
        }
        private void TurnHover(object sender, EventArgs e)
        {
            pictureBox2.Image = Properties.Resources.Turn_2;
        }
        private void ExitLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.Exit;
        }

        private void ExitHover(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.Exit_2;
        }

        private void FormMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) this.SetDesktopLocation(MousePosition.X - X, MousePosition.Y - Y);

        }

        

        private void FormDown(object sender, MouseEventArgs e)
        {
            X = e.X;
            Y = e.Y;
        }
        void Disconnect()
        {
                if (connected == false) return;
                Clear();
                recording = false;
                Log("Вы отключились от сервера");
                server = null;
                connected = false;
                if (receiveThread != null) { receiveThread.Abort(); receiveThread = null; }
                
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
         
        }
        void Disconnect(string comm)
        {
            if (connected == false) return;
            Clear();
            recording = false;
            Log(comm);
            server = null;
            connected = false;
            if (receiveThread != null) { receiveThread.Abort(); receiveThread = null; }
          
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();

        }
        void Connect()
        {
            Task process = Task.Run(() =>
            {
                Clear();
                client = new TcpClient();
                
                connected = true;
                try
                {
                    user.accepted = false;
                    Log("Попытка подключения");
                    client.Connect(server.host, server.port);

                    Config conn_config = new Config();
                    conn_config.SetParam("name",user.name);
                    conn_config.SetParam("pass", user.pass);
                    stream = client.GetStream();
                    Message mess = new Message(MessageType.conn,0,conn_config.ToString());
                    Send(mess);
                    receiveThread = new Thread(ReceiveMessage);
                    receiveThread.Start(); //старт потока

                }
                catch (Exception ex)
                {
                    Log("Подключение не удалось: "+ex.GetBaseException());
                }
            });
           

        }

        void Send(Message mess)
        {
            try
            {
                stream.Write(mess.GetFullData(), 0, mess.lenght+6);
               

            }
            catch(Exception ex) {
                Disconnect("Соединение было разорванно: "+ex.GetBaseException());
            }
        }
        void Log(string str,string sender)
        {
            DateTime time = DateTime.Now;
            sender = "User";
            string log = "[" + time.Hour + ":" + time.Minute + ":" + time.Second + "] "+" ["+sender+"] "+ str; 
            if (this.InvokeRequired)
                this.Invoke((MethodInvoker)delegate { ShowMessage(MessageType.mess, log); });
            else
            {
                ShowMessage(MessageType.mess,log);
        
            }


        }
        void Log(string str)
        {
            DateTime time = DateTime.Now;
            string log = "[" + time.Hour + ":" + time.Minute + ":" + time.Second + "] " + str;
            if (this.InvokeRequired)
                this.Invoke((MethodInvoker)delegate { ShowMessage(MessageType.mess, log); });
            else
            {
                ShowMessage(MessageType.mess, log);

            }


        }
        public void ShowMessage(MessageType type,object obj) {
            if (type == MessageType.mess) {
                Label label = new Label();
                
                
                label.Text = obj.ToString();
                label.AutoSize = true;
                label.Font = new Font("Trebuchet MS",8,FontStyle.Regular);
                label.MaximumSize = new Size(panelMessages.Width, panelMessages.Height);
                if (panelMessages.Controls.Count>0) {
                
                    label.Location = new Point(0, panelMessages.Controls[panelMessages.Controls.Count-1].Location.Y+ panelMessages.Controls[panelMessages.Controls.Count - 1].Height+ 10);
                }
                panelMessages.Controls.Add(label);
                
            }
        }
        public void LogOnline() {
            string str = "Online Users: ";
            for (int i=0;i<server.clients.Count;i++) {
                str += server.clients[i].user.name+" "; 
            }
            Log(str);
        }
        void Clear() {
            if (this.InvokeRequired)
                this.Invoke((MethodInvoker)delegate {panelMessages.Controls.Clear();});
            else
            {
                panelMessages.Controls.Clear();

            }
           

        }
        void Process(Message mess)
        {
           if (client == null) return;
            Config conf;
           int id;
           switch ((byte)mess.type)
            {
                            case 1:/*Заход с ПК <байты(имя)>*/;  conf = mess.GetConfig(); id = int.Parse(conf.GetValue("id")); if ((!user.accepted) && (conf.GetValue("name") == user.name)) { Log("Вы подключены к серверу"); user.id = id; user.status = conf.GetValue("status"); user.room = conf.GetValue("room"); user.accepted = true;recording = true;speaking = true; } else { Log(conf.GetValue("name") + " подключился к серверу"); server.AddClient(conf.GetValue("name"), id, conf.GetValue("status"), conf.GetValue("room"));user.accepted = true; }; break;
                            case 2:/*Выход <байт(id)>*/; conf = mess.GetConfig(); id = int.Parse(conf.GetValue("id")); if (!user.accepted) { Disconnect("Подключение не удалось: " + conf.GetValue("desc"));} else if (user.id == id) { Disconnect("Вы отключены по причине: " + conf.GetValue("desc"));} else { Log(conf.GetValue("name") + " отключился от сервера"); server.RemoveClient(id); }; break;
                            case 3:/*Сообщение <байты(сообщение)>*/ ; if (!user.accepted) return;  if (server.GetClient((int)mess.sender) != null) Log(mess.GetString(), server.GetClient((int)mess.sender).user.status); else Log(mess.GetString()); break;
                            case 4:/*Голос <байты(голос)>*/; if (!user.accepted) return; Hear(mess); break;
                            case 5:/*Онлайн*/; SetServerClients(mess); break;
            }
                    
         
                
              
            
        }
       
       
       void SetServerClients(Message mess) {
            Config conf = mess.GetConfig();
            string[] ids = conf.GetValues("ids");
            string[] names = conf.GetValues("names");
            string[] statuses = conf.GetValues("statuses");
            string[] rooms = conf.GetValues("rooms");

            server.SetClients(ids,names,statuses,rooms);
            
       }
        void Hear(Message mess) {
            Client client = server.GetClient(mess.sender);//Получает экземпляр класса Client с id отправителя
           if (client == null) return;
            BufferedWaveProvider bufferStream = client.bufferStream;
            if (!speaking) return;
            byte[] data = mess.GetFullData();
            bufferStream.AddSamples(data, 6, 3200);//Воспроизводит звук 

        }
        void Console(string str)
        {
            if (str == "") return;
            if (str.StartsWith("/"))
            {
                string[] words = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                switch (words[0])
                {
                   case "/clear": Clear(); Log("Консоль очищена"); break;
                   case "/online":LogOnline();break;
                   default: Send(new Message(MessageType.command, (ushort)user.id, str)); break;
                }

            }
            else
            {
             string st = user.name + ": " + str;
             Message mess = new Message(MessageType.mess,(ushort)user.id,st);
             Send(mess);
             Log(st,user.status);
            }
            
        }


        private void Enter(object sender, KeyPressEventArgs e)
        {
            if ((!user.accepted) || (!connected)) { e.Handled = true; return; }
            else
            if (e.KeyChar != '\r') return;
            e.Handled = true;
            string str = textBoxEnter.Text;
            textBoxEnter.Text = "";
            textBoxEnter.SelectionStart = 1;
            Console(str.Trim());

        }
        public void SetTab(int index) {
            canSelectTab = true;
           tabChat.SelectTab(index);
            canSelectTab = false;
        }
        
        private void Microfone(object sender, EventArgs e)
        {
            recording = !recording;
        }

        private void Speaker(object sender, EventArgs e)
        {
            speaking = !speaking;
        }

        private void ConnectButton(object sender, EventArgs e)
        {
            string str = SetServer();
            if (str == "Ок")
            {
                AddServerLabel.Text = "";
                Disconnect();
                Connect();
                SetTab(2);
            }
            else {
                AddServerLabel.Text = str;
            }
        }
        public string SetServer() {
            string[] host = HostBox.Text.Split(':');
            if (host.Length!=2) { return "Формат адреса - <ip>:<порт>";}
            string ip = host[0].Trim();
            string name = NameBox.Text.Trim();
            int port;

            IPAddress adress;
            if (!IPAddress.TryParse(ip, out adress)) { return "Некоректный адресс"; } 
            if (!int.TryParse(host[1], out port)) { return "Некоректный порт"; } else if (port> 65535||port<0) { return "Порт число от 0 до 65535"; }
            if (name=="") {name = "Сервер_"+dataGridViewServerList.Rows.Count.ToString(); }
            server = new ServerData(ip, port,name);
            

            return "Ок";
        }
        private void DisconnectButton(object sender, EventArgs e)
        {
            SetTab(1);
            Disconnect();
           
        }

        private void Login(object sender, EventArgs e)

        {
            if (InvokeRequired)Invoke(new Action(Login));else Login();
            
        }
        public void Login() {
            
                loginLabel.Text = "Подключение...";
                bool remember = rememberCheckBox.Checked;

                if (remember) { }
                string login = LoginBox.Text.Trim(),password = PasswordBox.Text, mess = TryLogin(login,password);
                if (mess == "Ок")
                {
                    user = new UserData(LoginBox.Text.Trim());
                    user.pass = PasswordBox.Text.Trim();
                loginLabel.Text = "";
                    SetTab(1);

                }
                else
                {
                    loginLabel.Text = mess;
                }
            
        }
        public string TryLogin(string login,string password) {
            if (login.Trim() == "") return "Введите имя пользователя";
            for (int i=0;i<login.Length;i++) {
                if (login[i] == ':') { return "Уберите символы из имени пользователя: ':' "; };
            }
            return "Ок";
        }
        public string TryLogin(string login)
        {
            if (login.Trim() == "") return "Введите имя пользователя";
            for (int i = 0; i < login.Length; i++)
            {
                if (login[i] == ':') { return "Уберите символы из имени пользователя: ':' "; };
            }
            return "Ок";
        }

        private void Unlogin(object sender, EventArgs e)
        {
            loginLabel.Text = "";
            user = null;
            SetTab(0);

        }

        private void AddServer(object sender, EventArgs e)
        {
            string str = SetServer();
            if (str == "Ок")
            {
                AddServerLabel.Text = "";
                AddServer(server);
            }
            else {
                AddServerLabel.Text = str;
            }
        }
         
        void AddServer(ServerData data) {
            for (int i = 0; i < dataGridViewServerList.Rows.Count; i++) {
                    if (servers[i].host == data.host&& servers[i].port==data.port) return;
                }
            if (dataGridViewServerList.SelectedRows.Count == 0) {
                servers.Add(data);
                dataGridViewServerList.Rows.Add( data.name,data.host+":"+data.port.ToString());
            } else {
                int i = dataGridViewServerList.SelectedRows[0].Index;
                dataGridViewServerList.Rows[i].Cells[1].Value = data.host+":"+data.port;
                dataGridViewServerList.Rows[i].Cells[0].Value = data.name;
                servers[i].host = data.host;
                servers[i].name = data.name;
            }
        }

        bool CheckServer(ServerData data)
        { 
            return true;
        }
        private void RemoveServer(object sender, EventArgs e)
        {
            if (dataGridViewServerList.SelectedRows.Count == 0)return;
           int i = dataGridViewServerList.SelectedRows[0].Index;
            dataGridViewServerList.Rows.RemoveAt(i);
            servers.RemoveAt(i);
        }

        private void SelectServer(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridViewServerList.SelectedRows.Count == 0) return;
            int i = dataGridViewServerList.SelectedRows[0].Index;
            HostBox.Text = (string)dataGridViewServerList.Rows[i].Cells[1].Value;
            NameBox.Text = (string)dataGridViewServerList.Rows[i].Cells[0].Value;
        }

        private void SelectTabChat(object sender, TabControlCancelEventArgs e)
        {
            if (!canSelectTab) {
                e.Cancel = true;
            }
            canSelectTab = false;
        }

        private void SelectTabLogin(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == 0)
            {
                labelLogin.Text = "Вход";
            }
            else {
                labelLogin.Text = "Регистрация";
            }
        }

        
        private void IconClick(object sender, EventArgs e)
        {
           
           WindowState = FormWindowState.Normal;
           TopMost = true;
        }

  
        private void Full(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    maximized = false;
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                    maximized = true;
                }
            }
        }

        private void Resized(object sender, EventArgs e)
        {
            panelMain.Width = Width;
            tabChat.Width = Width;
            tabChat.Height = Height-41;
            panelBrowser.Width = Width - 286;
            panelBrowser.Height = Height - 86;
            int h = Height - 480;
            int w = Width - 840;
            panelServerList.Width = 633 + w;
            panelServerList.Height = 394 + h;
            panelDialog.Width = 633 + w;
            panelDialog.Height = 394 + h;
            dataGridViewServerList.Width = 625 + w;
            dataGridViewServerList.Height = 386 + h;
            panelMessages.Width = 625 + w;
            panelMessages.Height = 296 + h;
            textBoxEnter.Width = 625 + w;
            textBoxEnter.Location = new Point(textBoxEnter.Location.X,305+h);
            foreach(Control c in panelMessages.Controls) {
                c.MaximumSize = new Size(panelMessages.Width, panelMessages.Height);
            }

        }

        private void FormLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Arrow;
            
        }

        private void FormResizeEnter(object sender, MouseEventArgs e)
        {
            if (resize) return;
            int x = MousePosition.X, y = MousePosition.Y;

            if (x > ((Location.X + Width) - 20)) { if (y > ((Location.Y + Height) - 20)) { Cursor = Cursors.SizeNWSE; resizeType = ResizeType.RightBottom; } else { Cursor = Cursors.SizeWE; resizeType = ResizeType.Right; } } else if (x < (Location.X + 20)) { if (y > ((Location.Y + Height) - 20)) { Cursor = Cursors.SizeNESW; resizeType = ResizeType.LeftBottom; } else { Cursor = Cursors.SizeWE; resizeType = ResizeType.Left; } } else if (y > ((Location.Y + Height) - 20)) { Cursor = Cursors.SizeNS; resizeType = ResizeType.Bottom; }
            

        }

        private void ResizeStart(object sender, MouseEventArgs e)
        {
            resize = true;
        }

        private void ResizeStop(object sender, MouseEventArgs e)
        {
            resize = false;
        }

        private void ResizeTimer(object sender, EventArgs e)
        {
            if (resize)
            {
                switch (resizeType)
                {
                    case ResizeType.Right: Width = MousePosition.X - Location.X; break;
                    case ResizeType.RightBottom: Width = MousePosition.X - Location.X; Height = MousePosition.Y - Location.Y; break;
                    case ResizeType.Bottom:; Height = MousePosition.Y - Location.Y; break;
                    case ResizeType.Left: int x = MousePosition.X - Location.X; if (Width - x <= MinimumSize.Width) { x = Width - MinimumSize.Width; Location = new Point(Location.X + x, Location.Y); Width = MinimumSize.Width; } else { Location = new Point(Location.X + x, Location.Y); Width -= x; }; break;
                    case ResizeType.LeftBottom: x = MousePosition.X - Location.X;Height = MousePosition.Y - Location.Y; if (Width - x <= MinimumSize.Width) { x = Width - MinimumSize.Width; Location = new Point(Location.X + x, Location.Y); Width = MinimumSize.Width; } else { Location = new Point(Location.X + x, Location.Y); Width -= x; }; break;
                    
                }

            }
        }

        private void ReloadStatus(object sender, EventArgs e)
        {
            LoadServersData();
        }
         void LoadServersData() {


        }

        private void SendFile(object sender, EventArgs e)
        {
            if (connected)
            {
                OpenFileDialog FileDialog = new OpenFileDialog();
                if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileInfo file = new FileInfo(FileDialog.FileName);
                }
            }
        }

        private void ChangeAdress(object sender, EventArgs e)
        {
          
                string str = SetServer();
                if (str == "Ок") {
                if (dataGridViewServerList.SelectedRows.Count == 1)
                {
                    dataGridViewServerList.SelectedRows[0].Cells[0].Value = NameBox.Text;
                    dataGridViewServerList.SelectedRows[0].Cells[1].Value = HostBox.Text;

                }
                    AddServerLabel.Text = "";
                }
                else
                {
                    AddServerLabel.Text = str;
                }
            
        }

        private void LoginCheck(object sender, EventArgs e)
        {
            string str = LoginBox.Text;
            if (str.Trim()=="") { loginLabel.Text = ""; return; }
            string st = TryLogin(str);
            if (st != "Ок")
            {
                loginLabel.Text = st;
            }
            else {
                loginLabel.Text = "";
            }
        }

      






        // получение сообщений
        void ReceiveMessage()
        {
            while (connected)
            {
                if (!stream.DataAvailable) {
                    Thread.Sleep(50);
                    continue;
                }
                    try {
                
                byte[] prop = new byte[6];
                stream.Read(prop,0,6);
                 
                ushort lenght = BitConverter.ToUInt16(prop,0);
                ushort sender = BitConverter.ToUInt16(prop, 2);
                ushort type = BitConverter.ToUInt16(prop, 4);
                byte[] data = new byte[lenght];
                stream.Read(data, 0, lenght);
               
                Message mess = new Message(data,lenght,sender,type);
                Process(mess);
                    }
                    catch (Exception ex)
                    {
                    Disconnect("Соединение было разорванно: "+ex.GetBaseException());
                    return;
                    }
                    
               
                  
                
                
            }
        }

    }
   
   
}
