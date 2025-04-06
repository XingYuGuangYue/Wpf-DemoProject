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
                if (bytesRead == 0) break; // 客户端断开连接

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Dispatcher.Invoke(() => ReceivedMessageBox.Text += $"Client: {receivedMessage}\n");
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
            byte[] data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);
            ReceivedMessageBox.Text += $"Server: {message}\n";
            SendMessageBox.Clear();
        }
    }
}