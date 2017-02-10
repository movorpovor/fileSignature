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
        BlockPool _blockPool;
        public event ThreadsCompletedHandler ThreadsCompleted;

        public void Start(BlockPool blockPool)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;  
            this._blockPool = blockPool;
            _threads[0] = new Thread(blockPool.Start);
            _threads[0].Start();

            for (int i = 1; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(HashIt);
                _threads[i].Start();
            }
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            lock(_blockPool)
            {
                _blockPool.manualStop = true;
            }

            _blockPool.Close();

            /*ThreadsCompleted?.Invoke(this, new ThreadsCompletedEventArgs
            {
                isBreaked = true
            });*/
        }

        private void HashIt()
        {
            bool earlyEnd = false;

            try
            {
                using (var hasher = SHA256.Create())
                {
                    BlockInfo block;
                    while ((block = _blockPool.GetNext()) != null)
                    {
                        if (_blockPool.manualStop)
                        {
                            earlyEnd = true;
                            break;
                        }

                        var hash = hasher.ComputeHash(block.bytes);
                        var hashString = string.Join("", hash.Select(x => x.ToString("x2")).ToArray());

                        Console.WriteLine("{0}:{1}", block.index, hashString);
                        _blockPool.ReturnBlock(block);
                    }
                }

                Interlocked.Increment(ref _countOfEndedThreads);

                if (_countOfEndedThreads == _threads.Length - 1)
                {
                    ThreadsCompleted?.Invoke(this, new ThreadsCompletedEventArgs
                    {
                        isBreaked = earlyEnd
                    });
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("{0}\n{1}", exc.Message, exc.StackTrace);
                Console.ReadKey();
            }
        }
    }
}
