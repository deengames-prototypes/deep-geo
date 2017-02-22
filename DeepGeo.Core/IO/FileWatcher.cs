using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DeenGames.DeepGeo.Core.IO
{
    public class FileWatcher
    {
        private readonly Action<string> onUpdateCallback;
        private string fileName;
        private readonly FileSystemWatcher watcher;

        public FileWatcher(string fileName, Action<string> onUpdateCallback)
        {
            this.fileName = fileName;
            this.onUpdateCallback = onUpdateCallback;

            var path = fileName.Substring(0, fileName.LastIndexOf("/"));
            var nameOnly = fileName.Substring(fileName.LastIndexOf("/") + 1);
            this.watcher = new FileSystemWatcher(path, nameOnly);
            watcher.EnableRaisingEvents = true;
            
            // Notify if create/write times changed
            this.watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            watcher.Changed += (source, e) =>
            {
                // Notification is when file is first accessed; could be locked by the OS for a few ms
                // try/catch is hideous (and the performance is hideous). This is slightly better.
                Thread.Sleep(25);
                var contents = File.ReadAllText(e.FullPath);
                onUpdateCallback(contents);
            };
        }

        /// <summary>
        /// Start watching the file for changes. This causes the callback to be
        /// triggered immediately, since we never knew about the file. (This is
        /// usually the desired behaviour.)
        /// </summary>
        public void Watch()
        {
            this.onUpdateCallback(File.ReadAllText(this.fileName));
        }
    }
}
