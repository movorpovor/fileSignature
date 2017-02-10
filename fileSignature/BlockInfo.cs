namespace fileSignature
{
    public class BlockInfo
    {
        public byte[] bytes;
        public int index;

        public BlockInfo(int size)
        {
            bytes = new byte[size];
            index = 0;
        }
    }
}
