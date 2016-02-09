namespace NNX.Core.Utils
{
    public class Pair<T>
    {
        public T First { get; set; }
        public T Second { get; set; }

        public Pair()
        {
        }

        public Pair(T first, T second)
        {
            First = first;
            Second = second;
        } 
    }
}
