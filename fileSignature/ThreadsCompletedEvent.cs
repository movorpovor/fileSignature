using System;

namespace fileSignature
{
    delegate void ThreadsCompletedHandler(object sender, ThreadsCompletedEventArgs e);
    
    class ThreadsCompletedEventArgs:EventArgs
    {
        public bool isBreaked;
    }
}
