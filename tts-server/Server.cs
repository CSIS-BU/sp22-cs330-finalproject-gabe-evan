using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace tts_server
{
    internal class Server
    {
        //private readonly static Thread _main = Thread.CurrentThread;

        public static void Main()
        {
            string ip = "127.0.0.1";
            int port = 80;
            var server = new TcpListener(IPAddress.Parse(ip), port);

            server.Start();
            Console.WriteLine("Server has started on {0}:{1}, Waiting for a connection...", ip, port);

            TcpClient client;

            while (true)
            {
                client = server.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(ThreadProc, client);
            }
        }

        private static void ThreadProc(object obj)
        {
            TcpClient client = (TcpClient)obj;

            Console.WriteLine("A client connected.");

            NetworkStream stream = client.GetStream();

            while (true)
            {
                while (!stream.DataAvailable) ;
                while (client.Available < 3) ; // match against "get"

                byte[] bytes = new byte[client.Available];
                stream.Read(bytes, 0, client.Available);
                string s = Encoding.UTF8.GetString(bytes);

                if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
                {
                    Console.WriteLine("=====Handshaking from client=====\n{0}", s);

                    // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
                    // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
                    // 3. Compute SHA-1 and Base64 hash of the new value
                    // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
                    string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                    string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                    byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                    string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                    // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                    byte[] response = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 101 Switching Protocols\r\n" +
                        "Connection: Upgrade\r\n" +
                        "Upgrade: websocket\r\n" +
                        "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    bool fin = (bytes[0] & 0b10000000) != 0, // Indicates that this is the final fragment in a message.
                        mask = (bytes[1] & 0b10000000) != 0; // All messages from the client must have mask bit set

                    int opcode = bytes[0] & 0b00001111,
                        msglen = bytes[1] & 0b01111111, // or -128
                        offset = 2;

                    // Opcodes: https://datatracker.ietf.org/doc/html/rfc6455#section-5.2
                    bool opcodeText = opcode == 1, // %x1 denotes a text frame
                         opcodeClose = opcode == 8; // %x8 denotes a connection close

                    if (msglen == 0)
                        Console.WriteLine("msglen == 0");
                    else if(mask && opcodeClose)
                    {
                        // Close connection to the client
                        client.Close();
                        Console.WriteLine("Client closed the connection");
                        return;
                    }
                    else if (mask && opcodeText)
                    {
                        byte[] decoded = new byte[msglen];
                        byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                        offset += 4;

                        for (int i = 0; i < msglen; ++i)
                            decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);

                        string text = Encoding.UTF8.GetString(decoded);
                        Console.WriteLine("{0}", text);
                    }
                    else
                    {
                        Console.WriteLine("Mask bit not set");
                    }

                    Console.WriteLine();
                }
            }

        }

    }
}