using System;
using System.Linq;

internal static class ArchAppSearchCatalogTests {
    private static int failures;

    private static void Assert(bool condition, string name) {
        if (!condition) {
            Console.Error.WriteLine("FAIL: " + name);
            failures++;
        } else {
            Console.WriteLine("PASS: " + name);
        }
    }

    public static int Main() {
        AppSearchEntry[] entries = {
            new AppSearchEntry("Visual Studio Code", "Code.exe", "", "", true),
            new AppSearchEntry("Code Runner", "runner.exe", "", "", false),
            new AppSearchEntry("Arduino IDE", "arduino.exe", "", "", true),
            new AppSearchEntry("Network Settings", "ms-settings:network", "", "", true)
        };

        AppSearchEntry[] empty = ArchAppSearchCatalog.Filter(entries, "");
        Assert(empty.Length == 3 && empty.All(item => item.IsPinned), "empty search shows pinned apps only");

        AppSearchEntry[] matches = ArchAppSearchCatalog.Filter(entries, "code");
        Assert(matches.Length == 2, "search matches app names case-insensitively");
        Assert(matches[0].Name == "Code Runner", "prefix matches rank before substring matches");

        AppSearchEntry[] mixedCase = ArchAppSearchCatalog.Filter(entries, "nEtWoRk");
        Assert(mixedCase.Length == 1 && mixedCase[0].Name == "Network Settings", "mixed-case search works");
        Assert(ArchAppSearchCatalog.Filter(entries, "missing").Length == 0, "no-match search returns no results");
        return failures == 0 ? 0 : 1;
    }
}
