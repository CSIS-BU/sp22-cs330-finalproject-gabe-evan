using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

// https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
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

            // Spin up game
            Game game = new Game();

            while (true)
            {
                while (!stream.DataAvailable) ;
                while (client.Available < 3) ; // match against "get"

                byte[] bytes = new byte[client.Available];
                stream.Read(bytes, 0, client.Available);
                string s = Encoding.UTF8.GetString(bytes);
                string keyB64 = null;

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
                    keyB64 = swkaSha1Base64;

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
                        
                        Console.WriteLine("Client: {0}\r\n", text);

                        string responseText = ProcessStream(ref game, text);

                        Console.WriteLine("Server: {0}", responseText);

                        Queue<string> que = new Queue<string>(responseText.SplitInParts(125));
                        int len = que.Count;

                        // https://stackoverflow.com/questions/27021665/c-sharp-websocket-sending-message-back-to-client
                        while (que.Count > 0)
                        {
                            var header = GetHeader(
                                que.Count > 1 ? false : true,
                                que.Count == len ? false : true
                            );

                            byte[] list = Encoding.UTF8.GetBytes(que.Dequeue());
                            header = (header << 7) + list.Length;
                            stream.Write(IntToByteArray((ushort)header), 0, 2);
                            stream.Write(list, 0, list.Length);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Mask bit not set");
                    }

                    Console.WriteLine();
                }
            }

        }

        private static int GetHeader(bool finalFrame, bool contFrame)
        {
            int header = finalFrame ? 1 : 0;//fin: 0 = more frames, 1 = final frame
            header = (header << 1) + 0;//rsv1
            header = (header << 1) + 0;//rsv2
            header = (header << 1) + 0;//rsv3
            header = (header << 4) + (contFrame ? 0 : 1);//opcode : 0 = continuation frame, 1 = text
            header = (header << 1) + 0;//mask: server -> client = no mask

            return header;
        }

        private static byte[] IntToByteArray(ushort value)
        {
            var ary = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(ary);
            }

            return ary;
        }

        private static string ProcessStream(ref Game game, string text)
        {
            Response res = new Response();

            try
            {
                Game.Move move = JsonConvert.DeserializeObject<Game.Move>(text);

                // Request reset of board by providing a -1
                if (move.row == -1 || move.row == -1)
                {
                    game.resetGame();
                    res.message = "New game started";
                }
                else if (!game.Finished)
                {
                    game.placeTic(Game.player, move.row, move.col);

                    bool playerWon = game.checkWinner(Game.player);
                    bool opponentWon = game.checkWinner(Game.opponent);

                    res.finished = playerWon || opponentWon || !game.isMovesLeft();

                    if (res.finished && (playerWon || opponentWon)) 
                    {
                        game.Finished = true;
                        game.wins++;
                        res.won = playerWon;
                        res.message = "You " + (playerWon ? "won" : "lost");
                    }
                    else if (res.finished)
                    {
                        game.Finished = true;
                        res.won = false;
                        res.message = "There was a tie";
                    }else
                    {
                        // The "AI"'s turn
                        Game.Move opMove = game.findBestMove();

                        game.placeTic(Game.opponent, opMove.row, opMove.col);
                        
                        res.finished = playerWon || opponentWon || !game.isMovesLeft();
                        
                        if (game.checkWinner(Game.opponent))
                        {
                            game.Finished = true;
                            game.losses++;
                            res.finished = true;
                            res.won = playerWon;
                            res.message = "You lost";
                        }
                        else if (!game.isMovesLeft())
                        {
                            game.Finished = true;
                            game.ties++;
                            res.won = false;
                            res.message = "There was a tie";
                        }
                        else
                        {
                            res.message = "Play your next move";
                        }
                    }
                }
                else
                    throw new GameFinishedException();
            }
            catch (GameFinishedException ex)
            {
                Console.WriteLine(ex.Message);
                res.error = true;
                res.won = game.checkWinner(Game.player);
                res.finished = true;
                res.message = "Game is finished";
            }
            catch(InvalidMoveException ex)
            {
                Console.WriteLine(ex.Message);
                res.error = true;
                res.message = "Player made an invalid move";
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex.Message);
                res.error = true;
                res.message = "Data from client could not be processed";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                res.error = true;
                res.message = "An unknown error has occured";
            }

            res.board = game.Board;
            
            return JsonConvert.SerializeObject(res);
        }

        private class Response
        {
            public bool error = false;
            public int wins;
            public int losses;
            public int ties;
            public string message;
            public bool won = false;
            public bool finished = false;
            public int[,] board;
        }
    }
}