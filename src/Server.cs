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
Socket socket = server.AcceptSocket(); // wait for client

socket.Send(Encoding.ASCII.GetBytes(PING_RESPONSE));
