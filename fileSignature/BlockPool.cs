﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace fileSignature
{
    class BlockPool
    {
        bool _isOver = true;
        Queue<BlockInfo> _poolClear;
        Queue<BlockInfo> _poolWithBlocks;
        int _capacity = Environment.ProcessorCount * 4;
        public string fileName;
        public bool manualStop = false;

        public BlockPool(int size, string path)
        {
            fileName = path;
            FileStreamer.Init(size, path);
        }

        public void Close()
        {
            FileStreamer.Close();
        }

        public void Start()
        {
            try
            {
                _poolClear = new Queue<BlockInfo>(_capacity);
                _poolWithBlocks = new Queue<BlockInfo>(_capacity);

                for (int i = 0; i < _capacity; i++)
                    _poolClear.Enqueue(new BlockInfo(FileStreamer.blockSize));

                ReadBlocks();

                lock(this)
                    _isOver = true;
            }
            catch (Exception exc)
            {
                Console.WriteLine("{0}\n{1}", exc.Message, exc.StackTrace);
                Console.ReadKey();
            }
        }

        private void ReadBlocks()
        {
            BlockInfo block;

            while ((block = FileStreamer.GetNextBlock(GetNextInClearQueue())) != null)
            {
                if (manualStop)
                    return;

                lock (_poolWithBlocks)
                {
                    _poolWithBlocks.Enqueue(block);
                    Monitor.Pulse(_poolWithBlocks);
                }
            }
        }

        private BlockInfo GetNextInClearQueue()
        {          
            lock(_poolClear)
            {
                if (_poolClear.Count == 0)
                {
                    if (_isOver)
                        return null;
                    else
                        Monitor.Wait(_poolClear);
                }

                return _poolClear.Dequeue();
            }            
        }

        public BlockInfo GetNext()
        {
            lock (_poolWithBlocks)
            {
                if (_poolWithBlocks.Count == 0)
                {
                    if (_isOver)
                        return null;
                    else
                        Monitor.Wait(_poolWithBlocks);
                }
                return _poolWithBlocks.Dequeue();
            }
        }

        public void ReturnBlock(BlockInfo block)
        {
            block.bytes = new byte[FileStreamer.blockSize];
            block.index = 0;
            lock (_poolClear)
            {
                _poolClear.Enqueue(block);
                Monitor.Pulse(_poolClear);
            } 
        }
    }
}
