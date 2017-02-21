using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DeenGames.DeepGeo.Core.IO
{
    public class FileWatcher
    {
        private readonly Action<string> onUpdateCallback;
        private readonly Timer timer;
        private readonly FileInfo fileInfo;
        private DateTime lastUpdated;
        private DateTime lastCreated;

        private static List<FileWatcher> watchers = new List<FileWatcher>();

        public FileWatcher(string fileName, Action<string> onUpdateCallback)
        {
            this.onUpdateCallback = onUpdateCallback;

            this.timer = new Timer(TimeSpan.FromSeconds(0.1).TotalSeconds);
            this.timer.Elapsed += (s, e) => this.OnTick();

            this.fileInfo = new FileInfo(fileName);

            watchers.Add(this);
        }

        /// <summary>
        /// Start watching the file for changes. This causes the callback to be
        /// triggered immediately, since we never knew about the file. (This is
        /// usually the desired behaviour.)
        /// </summary>
        public void Watch()
        {
            this.OnTick();
            this.timer.Start();
        }

        public void Stop()
        {
            this.timer.Stop();
        }

        private void OnTick()
        {
            this.fileInfo.Refresh();

            // On Xamarin, LastWriteTime may not update. But CreationTime does update.
            if (this.fileInfo.LastWriteTime != this.lastUpdated || this.fileInfo.CreationTime != this.lastCreated)
            {
                this.lastUpdated = this.fileInfo.LastWriteTime;
                this.lastCreated = this.fileInfo.CreationTime;
                var contents = File.ReadAllText(fileInfo.FullName);
                onUpdateCallback(contents);
            }
        }
    }
}
