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
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync("127.0.0.1", 8888);
                _stream = _tcpClient.GetStream();
                ReceivedMessageBox.Text += "Connected to server!\n";

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
                if (bytesRead == 0) break; // 服务器断开连接

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Dispatcher.Invoke(() => ReceivedMessageBox.Text += $"Server: {receivedMessage}\n");
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
            byte[] data = Encoding.UTF8.GetBytes(message);
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
    }
}