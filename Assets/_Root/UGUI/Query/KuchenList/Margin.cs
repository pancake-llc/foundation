namespace Pancake.UIQuery
{
    public interface IReadonlyMargin
    {
        float Top { get; }
        float Bottom { get; }
        float Left { get; }
        float Right { get; }
    }

    public class Margin : IReadonlyMargin
    {
        public float Top { get; set; }
        public float Bottom { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }

        public float TopBottom
        {
            set
            {
                Top = value;
                Bottom = value;
            }
        }

        public float LeftRight
        {
            set
            {
                Left = value;
                Right = value;
            }
        }
    }

    public class Spacer
    {
        public Spacer(float size)
        {
            Size = size;
        }

        public float Size { get; }
    }
}