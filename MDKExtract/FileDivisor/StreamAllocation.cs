namespace MDKExtract.FileDivisor
{
    public class StreamAllocation
    {
        public StreamAllocation(int length, string name)
        {
            Length = length;
            Name = name;
        }

        public int Length { get; }
        public string Name { get; }
    }
}