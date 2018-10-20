using System.Drawing.Printing;

public class Finder
{
    public static string FindDrawingAssembly()
    {
        var pd = new PrintDocument();
        return typeof(PrintDocument).Assembly.CodeBase;
    }
}

