using System;
using System.IO;
using System.Net;

namespace RedditSharp
{
    public class MultipartFormBuilder
    {
        public HttpWebRequest Request { get; set; }

        private string Boundary { get; set; }
        private MemoryStream Buffer { get; set; }
        private TextWriter TextBuffer { get; set; }

        public MultipartFormBuilder(HttpWebRequest request)
        {
            // TODO: See about regenerating the boundary when needed
            Request = request;
            var random = new Random();
            Boundary = "----------" + CreateRandomBoundary();
            request.ContentType = "multipart/form-data; boundary=" + Boundary;
            Buffer = new MemoryStream();
            TextBuffer = new StreamWriter(Buffer);
        }

        private string CreateRandomBoundary()
        {
            // TODO: There's probably a better way to go about this
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string value = "";
            var random = new Random();
            for (int i = 0; i < 10; i++)
                value += characters[random.Next(characters.Length)];
            return value;
        }

        public void AddDynamic(object data)
        {
            var type = data.GetType();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var entry = Convert.ToString(property.GetValue(data, null));
                AddString(property.Name, entry);
            }
        }

        public void AddString(string name, string value)
        {
            TextBuffer.Write("{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
                "--" + Boundary, name, value);
            TextBuffer.Flush();
        }

        public void AddFile(string name, string filename, byte[] value, string contentType)
        {
            TextBuffer.Write("{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                "--" + Boundary, name, filename, contentType);
            TextBuffer.Flush();
            Buffer.Write(value, 0, value.Length);
            Buffer.Flush();
            TextBuffer.Write("\r\n");
            TextBuffer.Flush();
        }

        public void Finish()
        {
            TextBuffer.Write("--" + Boundary + "--");
            TextBuffer.Flush();
            var stream = Request.GetRequestStream();
            Buffer.Seek(0, SeekOrigin.Begin);
            Buffer.WriteTo(stream);
            stream.Close();
        }
    }
}
