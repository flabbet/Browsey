using Browsey.Micron;
using Browsey.Rnsd;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Host;
using Drawie.Numerics;
using Drawie.Rendering;
using DrawiEngine;

namespace Browsey;

public class BrowseyApp : DrawieApp
{
    private MicronDocument doc;
    private MicronRenderer renderer;
    public override IHost CreateMainWindow()
    {
        var wind = Engine.WindowingPlatform.CreateWindow("Browsey", new VecI(1920, 1080));
        wind.SubscribeToRenderContent("MicronPage", "", RenderPage);

        Paint paint = new Paint();
        paint.Color = Colors.White;
        Font robotoMono = Font.FromStream(File.OpenRead("Assets/Fonts/RobotoMonoNerdFont-Regular.ttf"));
        renderer = new MicronRenderer(robotoMono, paint);
        return wind;
    }

    protected override void OnInitialize()
    {
        doc = MicronParser.Parse(File.ReadAllText(@"C:\Users\flubb\Git\Browsey\Browsey\micron-reference.mu"));
        /*RnsdService rnsdService = new RnsdService();
        rnsdService.Start();
        rnsdService.OnPageFetched += s =>
        {
            doc = MicronParser.Parse(s);
        };*/
    }

    private void RenderPage(TextureFramebuffer fb, double dt)
    {
        if (doc != null)
        {
            renderer.Render(fb, doc);
        }
    }
}