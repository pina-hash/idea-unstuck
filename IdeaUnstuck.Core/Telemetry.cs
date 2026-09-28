using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web.Script.Serialization;

namespace IdeaUnstuck.Core
{
    public sealed class Telemetry
    {
        readonly object gate = new object();
        readonly string session = Guid.NewGuid().ToString("N");
        public string Folder { get; }
        public int ExceptionCount { get; private set; }
        public string LastWriteError { get; private set; }
        public Telemetry(string folder = null) { Folder = folder ?? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IDEA", "Unstuck"); }
        public void Write(string eventName, object data = null)
        {
            try
            {
                lock (gate)
                {
                    Directory.CreateDirectory(Folder);
                    var row = new { schema = 1, utc = DateTime.UtcNow.ToString("o"), session, eventName, data };
                    File.AppendAllText(System.IO.Path.Combine(Folder, "log-" + DateTime.Now.ToString("yyyyMMdd") + ".jsonl"), new JavaScriptSerializer().Serialize(row) + "\n", new UTF8Encoding(false));
                }
            }
            catch (Exception ex) { LastWriteError = ex.Message; System.Diagnostics.Debug.WriteLine(ex); }
        }
        public void Exception(string operation, Exception ex) { ExceptionCount++; Write("exception", new { operation, type = ex.GetType().FullName, ex.Message, ex.StackTrace }); }
        public string ExportToday()
        {
            try
            {
                lock (gate)
                {
                    var file = System.IO.Path.Combine(Folder, "log-" + DateTime.Now.ToString("yyyyMMdd") + ".jsonl");
                    var target = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "IDEA-Unstuck-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6) + ".zip");
                    using (var zip = ZipFile.Open(target, ZipArchiveMode.Create)) zip.CreateEntryFromFile(file, System.IO.Path.GetFileName(file));
                    return target;
                }
            }
            catch (Exception ex) { Exception("teacher_export", ex); return null; }
        }
    }
    public sealed class ApiGuard
    {
        readonly Telemetry log;
        public ApiGuard(Telemetry log) { this.log = log; }
        public T Get<T>(string name, Func<T> call, T fallback = default(T), List<string> issues = null)
        {
            try { return call(); }
            catch (Exception ex) { log.Exception(name, ex); issues?.Add(name); return fallback; }
        }
        public bool Run(string name, Action call) => Get(name, () => { call(); return true; });
    }
}
