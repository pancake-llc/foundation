namespace Pancake.MobileInput
{
    public enum PerspectiveZoomMode
    {
        FieldOfView,
        Translation,
    }

    public enum SnapAngle
    {
        Straight0Degrees,
        Diagonal45Degrees
    }

    public enum CameraPlaneAxes
    {
        XY2DSideScroll, //Sidescroll
        XZTopDown //Top-Down
    }

    public enum AutoScrollDampMode
    {
        Default,
        SlowFadeOut,
        Custom
    }
}