﻿using System.IO;
using System.Threading;
using System.Windows;
using Wabbajack.Common;
using Wabbajack.Lib;
using Wabbajack.Lib.Downloaders;

namespace Wabbajack.UI
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        public enum WindowResult
        {
            Undefined,
            Completed,
            Canceled
        }

        public WindowResult Result { get; internal set; } = WindowResult.Undefined;

        public DownloadWindow(string url, string name, long size, string destination)
        {
            InitializeComponent();
            DataContext = new DownloadWindowViewModel(this, url, name, size, destination);
        }
    }

    public class DownloadWindowViewModel : ViewModel
    {

        private readonly string _destination;
        private readonly DownloadWindow _parent;
        private long _size;

        public DownloadWindowViewModel(DownloadWindow parent, string url, string name, long size, string destination)
        {
            _parent = parent;
            _url = url;
            _downloadName = name;
            _destination = destination;
            _size = size;

            Start();
        }

        private void Start()
        {
            _downloadThread = new Thread(() =>
            {
                var state = DownloadDispatcher.ResolveArchive(_url);
                state.Download(new Archive {Name = _downloadName, Size = _size}, _destination);
                _destination.FileHash();


                _parent.Result = DownloadWindow.WindowResult.Completed;
                _parent.Dispatcher.Invoke(() => _parent.Close());
            });
            _downloadThread.Start();
        }

        public void Cancel()
        {
            if (_downloadThread != null && _downloadThread.IsAlive)
            {
                _downloadThread.Abort();
            }

            File.Delete(_destination);
            _parent.Result = DownloadWindow.WindowResult.Canceled;
        }


        private int _downloadProgress;


        public int DownloadProgress
        {
            get => _downloadProgress;
            set => RaiseAndSetIfChanged(ref _downloadProgress, value);
        }

        private string _url;
        public string Url
        {
            get => _url;
            set => RaiseAndSetIfChanged(ref _url, value);
        }


        private string _downloadName;
        private Thread _downloadThread;

        public string DownloadName
        {
            get => _downloadName;
            set => RaiseAndSetIfChanged(ref _downloadName, value);
        }
    }
}
