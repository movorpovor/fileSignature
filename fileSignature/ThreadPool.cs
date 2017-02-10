using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace fileSignature
{
    class ThreadPool
    {
        Thread[] _threads = new Thread[Environment.ProcessorCount];
        int _countOfEndedThreads = 0;
        BlockPool blockPool;
        public event ThreadsCompletedHandler ThreadsCompleted;

        public void Start(BlockPool blockPool)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;  
            this.blockPool = blockPool;
            _threads[0] = new Thread(blockPool.Start);
            _threads[0].Start();

            for (int i = 1; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(hashIt);
                _threads[i].Start();
            }
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            for (int i = 0; i < _threads.Length; i++)
                _threads[i].Abort();

            ThreadsCompleted?.Invoke(this, new ThreadsCompletedEventArgs
            {
                isBreaked = true
            });
        }

        private void hashIt()
        {
            using (var hasher = SHA256.Create())
            {
                BlockInfo block;
                while ((block = blockPool.GetNext()) != null)
                {
                    var hash = hasher.ComputeHash(block.bytes);
                    var hashString = string.Join("", hash.Select(x => x.ToString("x2")).ToArray());

                    Console.WriteLine("{0}:{1}", block.index, hashString);
                    blockPool.returnBlock(block);
                }
            }

            Interlocked.Increment(ref _countOfEndedThreads);

            if (_countOfEndedThreads == _threads.Length - 1)
            {
                ThreadsCompleted?.Invoke(this, new ThreadsCompletedEventArgs
                {
                    isBreaked = false
                });
            }
        }
    }
}
