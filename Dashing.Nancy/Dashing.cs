using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Interpreter;
using Nancy;
using Nancy.ModelBinding.DefaultBodyDeserializers;
using Nancy.Responses;
using Dashing;
using System.IO;
using Nancy.Json;
using System.Dynamic;
using System.Threading;
using Nancy.ViewEngines;

namespace Dashing
{
    public class ClientBus
    {
        private static readonly Dictionary<string, Event> History = new Dictionary<string, Event>();
        private static readonly List<EventStreamWriterResponse> Clients = new List<EventStreamWriterResponse>();

        public static void Register(EventStreamWriterResponse client)
        {
            Clients.Add(client);
        }

        public static void Disconnect(EventStreamWriterResponse client)
        {
            Clients.Remove(client);
        }

        public static IEnumerable<Event> GetHistory()
        {
            return History.Select(s => s.Value);
        }

        public static void SendEvent(Event eventData)
        {
            History[eventData.Id] = eventData;
            var @event = eventData.Data;
            @event.id = eventData.Id;
            @event.updatedAt = eventData.UpdatedAt.Ticks;
            foreach (var client in Clients)
            {
                client.Write(@event);
            }
        }
    }


    public class DashingModule : NancyModule
    {
        private IFileSystem _fileSystem;

        public DashingModule()
            : base()
        {
            Settings.DefaultDashboard = "sample";
            Settings.Views = "dashboards";

            Get["/"] = x =>
            {
                return Response.AsRedirect("/" + Settings.DefaultDashboard);
            };

            Get["/{dashboard}"] = p =>
            {
                return View["dashboards/" + p.dashboard];
            };

            Get["/views/{widget}.html"] = param =>
            {
                string widget = param.widget;
                return Response.AsFile(string.Format("widgets/{0}/{0}.html", widget));
            };

            Get["/events"] = param =>
            {
                var @event = new { id = "Open", data = "Open" };
                var streamResponse = new EventStreamWriterResponse(Response, null, @event);
                ClientBus.Register(streamResponse);
                return streamResponse;
            };

            Post["/widgets/{id}"] = p =>
                {
                    Request.Body.Position = 0;
                    var s = new JavaScriptSerializer();
                    var body = s.DeserializeObject(new StreamReader(Request.Body).ReadToEnd()).ToExpando();
                    ClientBus.SendEvent(new Event
                    {
                        Id = p.id,
                        Data = body,
                        UpdatedAt = SystemTime.Now()
                    });
                    return 200;
                };
        }





        //def send_event(id, body)
        //  body[:id] = id
        //  body[:updatedAt] ||= Time.now.to_i
        //  event = format_event(body.to_json)
        //  settings.history[id] = event
        //  settings.connections.each { |out| out << event }
        //end


        //def latest_events
        //  settings.history.inject("") do |str, (id, body)|
        //    str << body
        //  end
        //end


    }

    public class SystemTime
    {
        public static Func<DateTime> Now = () => DateTime.Now;
    }

  

    public class Event
    {
        public string Id { get; set; }
        public DateTime UpdatedAt { get; set; }
        public dynamic Data { get; set; }
    }

    //    set :root, Dir.pwd

    //set :sprockets,     Sprockets::Environment.new(settings.root)
    //set :assets_prefix, '/assets'
    //set :digest_assets, false
    //['assets/javascripts', 'assets/stylesheets', 'assets/fonts', 'assets/images', 'widgets', File.expand_path('../../javascripts', __FILE__)]. each do |path|
    //  settings.sprockets.append_path path
    //end

    //set server: 'thin', connections: [], history: {}
    //set :public_folder, File.join(settings.root, 'public')
    //set :views, File.join(settings.root, 'dashboards')
    //set :default_dashboard, nil
    //set :auth_token, nil

    //helpers Sinatra::ContentFor
    //helpers do
    //  def protected!
    //    # override with auth logic
    //  end
    //end

    //post '/widgets/:id' do
    //  request.body.rewind
    //  body =  JSON.parse(request.body.read)
    //  auth_token = body.delete("auth_token")
    //  if !settings.auth_token || settings.auth_token == auth_token
    //    send_event(params['id'], body)
    //    204 # response without entity body
    //  else
    //    status 401
    //    "Invalid API key\n"
    //  end
    //end

    //not_found do
    //  send_file File.join(settings.public_folder, '404.html')
    //end

    //def development?
    //  ENV['RACK_ENV'] == 'development'
    //end

    //def production?
    //  ENV['RACK_ENV'] == 'production'
    //end


    //def format_event(body)
    //  "data: #{body}\n\n"
    //end

    //def latest_events
    //  settings.history.inject("") do |str, (id, body)|
    //    str << body
    //  end
    //end

    //def first_dashboard
    //  files = Dir[File.join(settings.views, '*.erb')].collect { |f| f.match(/(\w*).erb/)[1] }
    //  files -= ['layout']
    //  files.first
    //end

    //Dir[File.join(settings.root, 'lib', '**', '*.rb')].each {|file| require file }
    //{}.to_json # Forces your json codec to initialize (in the event that it is lazily loaded). Does this before job threads start.

    //job_path = ENV["JOB_PATH"] || 'jobs'
    //files = Dir[File.join(settings.root, job_path, '/*.rb')]
    //files.each { |job| require(job) } 

}
