using System;
using System.IO;
using System.Linq;
using System.Text;
using Nancy;

namespace Dashing
{
    public class EventStreamWriterResponse : Response
    {
        private Stream _responseStream;
        private ISerializer _serializer;
        private byte[] _dataheader = Encoding.UTF8.GetBytes("data: ");
        private byte[] _newline = Encoding.UTF8.GetBytes("\n");

        public EventStreamWriterResponse()
        {

        }

        public EventStreamWriterResponse(IResponseFormatter formatter, string id, dynamic body)
        {
            _serializer = formatter.Serializers.FirstOrDefault(s => s.CanSerialize("application/json"));
            ContentType = "text/event-stream";
            Contents = s =>
            {
                _responseStream = s;
                Write(body);
            };
        }

        public virtual void Write(dynamic body)
        {
            if (_responseStream == null)
                throw new InvalidOperationException("Cannot write to the response before we returned it from the connect route.");
            var ms = new MemoryStream();

            WriteDataJson(body, ms);

            ms.Position = 0;
            ms.CopyTo(_responseStream);
            _responseStream.Flush();
        }

        private void WriteDataJson(dynamic body, MemoryStream ms)
        {
            ms.Write(_dataheader, 0, _dataheader.Length);
            _serializer.Serialize("application/json", body, ms);

            ms.Write(_newline, 0, _newline.Length);
            ms.Write(_newline, 0, _newline.Length);
        }

        public void CloseStream()
        {
            _responseStream.Close();
        }
    }
}