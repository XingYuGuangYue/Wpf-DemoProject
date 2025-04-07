using System.Net.Http;
using System.Net.Sockets;
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
using System.Windows.Threading;

namespace TCPClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;

        public MainWindow()
        {
            InitializeComponent();
        }
        private async void ConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            if (_tcpClient == null) 
            {
                try
                {
                    _tcpClient = new TcpClient();
                    await _tcpClient.ConnectAsync("127.0.0.1", 8888);
                    ConnectServer();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }  
            else if (!_tcpClient.Connected)
                //，连接断开后，重新连接
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync("127.0.0.1", 8888);
                ConnectServer();
            }
        }

        private async void ConnectServer()
        {
            _stream = _tcpClient.GetStream();
            ConnectBtn.Content = "Connection completed";
            ReceivedMessageBox.Text += "Connected to server!\n";
            // 持续接收消息
            await ReceiveMessages();
        }

        private async Task ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead;
                try//服务器异常关闭
                {
                    
                    bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                    bytesRead = 0;
                }
                
                if (bytesRead == 0) 
                {
                    //_tcpClient.Close();
                    //RestartConnect();
                    ConnectBtn.Content = "Connect to server";
                    _tcpClient.Close();
                    break; // 服务器断开连接
                }
                else
                {
                    //string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);//接收文字信息，解码为UTF8
                    
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
                    string receivedMessage = BitConverter.ToString(data).Replace("-", " ");//接收字节信息，转换成16进制字符串

                    Dispatcher.Invoke(() => ReceivedMessageBox.Text += $"Server: {receivedMessage}\n");
                }             
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (_stream == null)
            {
                MessageBox.Show("Not connected to server!");
                return;
            }

            string message = SendMessageBox.Text;
            //byte[] data = Encoding.UTF8.GetBytes(message);//发送文字信息，编码为UTF8
                                                          
            //发送字节信息，每个字节用两个16进制字符串表示的，中间用空格分隔
            byte[] data = message.Split(' ') // 按空格分割字符串
                            .Select(hex => Convert.ToByte(hex, 16)) // 将每个十六进制字符串转换为字节
                            .ToArray(); // 转换为字节数组;
            await _stream.WriteAsync(data, 0, data.Length);
            ReceivedMessageBox.Text += $"Client: {message}\n";
            SendMessageBox.Clear();
        }

        /*
        关键点 说明
        TcpListener 服务器监听指定端口（如 8888）
        TcpClient 客户端连接服务器（127.0.0.1:8888）
        NetworkStream 用于读写数据
        async/await 异步处理消息，避免 UI 卡死
        Dispatcher.Invoke   跨线程更新 UI
        */

        private async void RestartConnect()
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync("127.0.0.1", 8888);
            _stream = _tcpClient.GetStream();
            ReceivedMessageBox.Text += "Reconnected!\n";
        }
    }
}