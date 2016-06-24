using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace ServiceTester
{
    class Program
    {
        public static string image_printer = "";
        public static string document_printer = "";
        public static string temp_file_path = "";
        public static string port = "";
        public static string d_printer = "";

        [DllImport("Winspool.drv")]
        private static extern bool SetDefaultPrinter(string printerName);

        Program()
        {
            
            if (File.Exists("printer.conf"))
            {
                string value1 = File.ReadAllText("printer.conf");
                JObject o = JObject.Parse(value1);
                image_printer=o["img_printer"].ToString();
                document_printer = o["doc_printer"].ToString();
                temp_file_path = o["temp_path"].ToString();
                port = o["port"].ToString();
                d_printer = o["d_printer"].ToString();
                SetDefaultPrinter(d_printer);
            }
            else
            {
                System.Console.WriteLine("Initial Configuration");
                System.Console.WriteLine("+++++++++++++++++++++++++");
                System.Console.WriteLine("");

                System.Console.WriteLine("Enter the image printer name (Case Sensitive):");
                image_printer = System.Console.ReadLine();

                System.Console.WriteLine("Enter the Document printer name (Case Sensitive):");
                document_printer = System.Console.ReadLine();

                System.Console.WriteLine("Enter the Default printer name (Case Sensitive):");
                d_printer = System.Console.ReadLine();

                System.Console.WriteLine("Enter the temproary path for dumpfiles:(Don't Use \"C:\\\" drive Paths )");
                temp_file_path = System.Console.ReadLine();
                temp_file_path=temp_file_path.Replace(@"\",@"\\");
                System.Console.WriteLine("Enter the port value:");
                port = System.Console.ReadLine();

                using (StreamWriter writer = new StreamWriter("printer.conf"))
                {
                    writer.Write("{\"img_printer\":\"" + image_printer + "\",\"doc_printer\":\"" + document_printer + "\",\"temp_path\":\""+temp_file_path+"\",\"port\":\""+port+"\",\"d_printer\":\"" + d_printer +"\"}");
                }
                SetDefaultPrinter(d_printer);
            }
        }
        static void Main(string[] args)
        {
            new Program();

            Console.WriteLine("Starting to receive the file");
            var listener = new TcpListener(IPAddress.Any, int.Parse(port));
            listener.Start();
            string file = "";
            while (true)
            {
                    using (var client = listener.AcceptTcpClient())
                   using (var stream = client.GetStream())
                   using (var output = File.Create("result.dat"))
                   {
                       Console.WriteLine("Client connected. Starting to receive the file");

                       // read the file in chunks of 1KB
                       var buffer = new byte[1024];
                       
                       int bytesRead;
                       while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                       {
                           output.Write(buffer, 0, bytesRead);
                           
                       }
                       Console.WriteLine("finsihed connected. Starting to receive the file "+ file);
                       output.Close();
                       receivedData();
                   }
                /*
                Console.WriteLine("Client connected. Starting to receive the file");


                TcpClient client = listener.AcceptTcpClient();

                NetworkStream stream = client.GetStream();
                byte[] data = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(data, 0, Convert.ToInt32(client.ReceiveBufferSize));
                StringBuilder sb = new StringBuilder();
                sb.Append( Encoding.ASCII.GetString(data, 0, bytesRead));
                //receivedData(sb);
                
                Console.WriteLine("finsihed connected. Starting to receive the file ");
               */
            }
        }

        public static void receivedData()
        {

            string value1 = File.ReadAllText("result.dat");
            JObject o = JObject.Parse(value1);
            Console.WriteLine(o["ftype"]);

            var bytes = Convert.FromBase64String(o["data"].ToString());
            using (var imageFile = new FileStream(temp_file_path + o["fname"] + "." + o["ftype"], FileMode.Create))
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
            }

            if (o["ftype"].ToString() == "png" || o["ftype"].ToString() == "jpg" || o["ftype"].ToString() == "jpeg")
            {
                printImage(temp_file_path + o["fname"] + "." + o["ftype"]);
            }
            else
            {
                printData(temp_file_path+ o["fname"] + "." + o["ftype"]);
            }
            
            
            Console.WriteLine("done");
        }

        public static void printData(string fame)
        {
          /*  ProcessStartInfo info = new ProcessStartInfo(fame);
            info.Verb = "Print";
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(info);*/

        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(fame);
        info.Arguments = "\""+document_printer+"\"";
        info.CreateNoWindow = true;
        info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        info.UseShellExecute = true;
        info.Verb = "PrintTo";
        System.Diagnostics.Process.Start(info);
        info.Arguments = "\"" + d_printer + "\"";
        SetDefaultPrinter(d_printer);
        }
      

        public static void printImage(string fame)
        {
            using (var pd = new System.Drawing.Printing.PrintDocument())
            {
                pd.PrinterSettings.PrinterName = image_printer ;
                pd.PrintPage += (_, e) =>
                {
                    var img = System.Drawing.Image.FromFile(fame);

                    // This uses a 50 pixel margin - adjust as needed
                    e.Graphics.DrawImage(img, new Point(50, 50));
                };
                pd.Print();
                SetDefaultPrinter(d_printer);
                pd.Dispose();
            }
            
        }

       
    
    }
}
