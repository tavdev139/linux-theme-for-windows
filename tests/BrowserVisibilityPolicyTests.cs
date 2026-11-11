using System;

internal static class BrowserVisibilityPolicyTests {
    private static int failures;

    private static void Assert(bool expected, bool actual, string name) {
        if (expected != actual) {
            Console.Error.WriteLine("FAIL: " + name + " expected=" + expected + " actual=" + actual);
            failures++;
        } else {
            Console.WriteLine("PASS: " + name);
        }
    }

    public static int Main() {
        Assert(false, BrowserVisibilityPolicy.ShouldHide(false, "chrome"), "disabled setting keeps panel visible");
        Assert(true, BrowserVisibilityPolicy.ShouldHide(true, "chrome"), "Chrome foreground hides panel");
        Assert(true, BrowserVisibilityPolicy.ShouldHide(true, "msedge.exe"), "Edge executable suffix is accepted");
        Assert(true, BrowserVisibilityPolicy.ShouldHide(true, "FIREFOX"), "browser matching is case-insensitive");
        Assert(false, BrowserVisibilityPolicy.ShouldHide(true, "ArchPanel"), "panel process does not hide itself");
        Assert(false, BrowserVisibilityPolicy.ShouldHide(true, "explorer"), "non-browser foreground leaves panel visible");
        Assert(false, BrowserVisibilityPolicy.ShouldHide(true, null), "missing process leaves panel visible");
        return failures == 0 ? 0 : 1;
    }
}
