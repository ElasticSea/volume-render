namespace Render
{
    public class RenderPreset
    {
        public string Name { get; set; }
        public RenderSettings Settings { get; set; }

        public override string ToString() => Name;
    }
}