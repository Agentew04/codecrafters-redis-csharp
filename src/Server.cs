using System.Net;
using System.Net.Sockets;
using System.Text;

const int PORT = 6379;
const string PING_RESPONSE = "+PONG\r\n";

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, PORT);
server.Start();
Socket socket = await server.AcceptSocketAsync();

while (true) {
    byte[] buffer = new byte[1024];
    var received = await socket.ReceiveAsync(buffer, SocketFlags.None);

    string msg = Encoding.ASCII.GetString(buffer, 0, received);
    if(msg.Length == 0) {
        Console.WriteLine("Client ended conn");
        break;
    }

    await socket.SendAsync(Encoding.ASCII.GetBytes(PING_RESPONSE), SocketFlags.None);

}