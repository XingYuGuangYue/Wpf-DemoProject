using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Net.Http;

namespace TCPServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener _tcpListener;
        private TcpClient _client;
        private NetworkStream _stream;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
                _tcpListener.Start();
                ReceivedMessageBox.Text += "Server started. Waiting for client...\n";

                _client = await _tcpListener.AcceptTcpClientAsync();
                _stream = _client.GetStream();
                ReceivedMessageBox.Text += "Client connected!\n";
                //StartServerBtn.Content = "Stop Server";
                // 持续接收消息
                await ReceiveMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async Task ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) 
                {
                    //await Task.Delay(500);
                    Dispatcher.Invoke(() => ReceivedMessageBox.Text += "Client disconnect!\n");
                    _client = await _tcpListener.AcceptTcpClientAsync();
                    _stream = _client.GetStream();
                    //_client = await _tcpListener.AcceptTcpClientAsync();
                    //_tcpListener.Stop();
                    //break;
                }// 客户端断开连接
                else
                {
                    //string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);//接收文字信息，解码为UTF8
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead); 
                    string receivedMessage = BitConverter.ToString(data).Replace("-", " ");//接收字节信息，转换成16进制字符串

                    Dispatcher.Invoke(() => ReceivedMessageBox.Text += $"Client: {receivedMessage}\n");
                }
                
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (_stream == null)
            {
                MessageBox.Show("No client connected!");
                return;
            }

            string message = SendMessageBox.Text;
            //byte[] data = Encoding.UTF8.GetBytes(message);//发送文字信息，编码为UTF8
            //发送字节信息，每个字节用两个16进制字符串表示的，中间用空格分隔
            byte[] data = message.Split(' ') // 按空格分割字符串
                            .Select(hex => Convert.ToByte(hex, 16)) // 将每个十六进制字符串转换为字节
                            .ToArray(); // 转换为字节数组;
            await _stream.WriteAsync(data, 0, data.Length);
            ReceivedMessageBox.Text += $"Server: {message}\n";
            SendMessageBox.Clear();
        }

    }
}