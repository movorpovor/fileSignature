using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fileSignature
{
    delegate void ThreadsCompletedHandler(object sender, ThreadsCompletedEventArgs e);
    
    class ThreadsCompletedEventArgs:EventArgs
    {
        public bool isBreaked;
    }
}
