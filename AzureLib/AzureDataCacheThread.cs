using System;
using System.Threading;

namespace AzureLib
{
    public class AzureDataCacheThread
    {
        private Thread _thread;

        private bool _isStarted = false;

        private bool _isStopped = true;

        private TimeSpan _pollingFrequency;

        private AutoResetEvent _stopRequested = new AutoResetEvent(false);

        private string _threadName;

        private IAzureDataCache _azureDataCache;

        public void Start()
        {
            if (!this._isStarted)
            {
                this._isStarted = true;
                this._isStopped = false;

                this._thread = new Thread(this.Run);
                this._thread.Name = _threadName;
                this._thread.IsBackground = true;
                this._thread.Start();
            }
        }

        public void Stop()
        {
            if ((this._isStarted) && (!this._isStopped))
            {
                this._stopRequested.Set();
            }
        }

        public AzureDataCacheThread(string threadName, IAzureDataCache azureDataCache, TimeSpan pollingFrequency)
        {
            this._threadName = threadName;
            this._azureDataCache = azureDataCache;
            this._pollingFrequency = pollingFrequency;
        }

        private void Run()
        {
            try
            {
                do
                {
                    this._azureDataCache.RecacheAllGroups();

                    if (this._stopRequested.WaitOne(this._pollingFrequency))
                    {
                        this._isStopped = true;
                        break;
                    }
                }
                while (!this._isStopped);
            }
            finally
            {
                // The thread is terminating. 
                this._isStarted = false;
                this._isStopped = true;
            }
        }
    }
}
