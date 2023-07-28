namespace ScribanGen.Tests;


public partial class CommentRendererTest
{
    [ScribanRenderMultilineComments]
    private partial class Bar
    {
        /*
        {{ for i in 0..10 }}
        public static int I{{ i }} = {{ i }};
        {{ end }}
        */
    }


    public void Foo()
    {
    }
}