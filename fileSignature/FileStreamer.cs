using System.IO;
using System.Threading;

namespace fileSignature
{
    public static class FileStreamer
    {
        static FileStream _file;
        static int _blockIndex = 0;
        public static int blockSize;

        public static void Init(int size, string path)
        {
            _file = new FileStream(path, FileMode.Open);
            blockSize = size;
        }

        public static void Close()
        {
            lock (_file)
            {
                _file.Close();
            }
        }

        public static BlockInfo GetNextBlock(BlockInfo block = null)
        {
            if (block == null)
                block = new BlockInfo(blockSize);

            lock (_file)
            {
                if (!_file.CanRead) return null;

                if (_file.Read(block.bytes, 0, blockSize) == 0)
                {
                    _file.Close();
                    return null;
                }

                block.index = _blockIndex;

                _blockIndex++;
            }

            return block;
        }
    }
}
