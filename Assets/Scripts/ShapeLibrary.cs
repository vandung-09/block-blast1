using UnityEngine;

// Danh sách hình khối. 'X' là ô có block, '.' là ô trống, '/' xuống dòng.
public static class ShapeLibrary
{
    static readonly string[] Shapes =
    {
        "X", "XX", "X/X", "XXX", "X/X/X", "XXXX", "X/X/X/X", "XXXXX", "X/X/X/X/X",
        "XX/XX", "XXX/XXX/XXX", "XXX/XXX", "XX/XX/XX",
        "X./XX", ".X/XX", "XX/X.", "XX/.X",
        "X../X../XXX", "..X/..X/XXX", "XXX/X../X..", "XXX/..X/..X",
        "X./X./XX", ".X/.X/XX", "XX/X./X.", "XX/.X/.X",
        "XXX/.X.", ".X./XXX", "X./XX/X.", ".X/XX/.X",
        "XX./.XX", ".XX/XX.", ".X/XX/X.", "X./XX/.X",
    };

    public static string Random() => Shapes[UnityEngine.Random.Range(0, Shapes.Length)];
}
