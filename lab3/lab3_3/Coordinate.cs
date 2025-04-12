namespace Lab3_3
{
    public class Coordinate<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }

        public Coordinate(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Coordinate(Coordinate<T> coordinate)
        {
            X = coordinate.X;
            Y = coordinate.Y;
            Z = coordinate.Z;
        }

        // Create string based indexer
        public T this[string index]
        {
            get
            {
                if (index.Contains("-"))
                    index = index.Remove(0, 1);

                switch (index)
                {
                    case "X":
                        return X;
                    case "Y":
                        return Y;
                    case "Z":
                        return Z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                if (index.Contains("-"))
                    index = index.Remove(0, 1);

                switch (index)
                {
                    case "X":
                        X = value;
                        break;
                    case "Y":
                        Y = value;
                        break;
                    case "Z":
                        Z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}