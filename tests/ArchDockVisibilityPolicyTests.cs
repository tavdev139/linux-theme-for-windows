using System;

internal static class ArchDockVisibilityPolicyTests {
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
        Assert(true, ArchDockVisibilityPolicy.ShouldShow(false, "chrome", "YouTube", false, false), "disabled auto-hide keeps dock visible");
        Assert(true, ArchDockVisibilityPolicy.ShouldShow(true, "explorer", "Program Manager", false, false), "desktop shows primary dock");
        Assert(false, ArchDockVisibilityPolicy.ShouldShow(true, "explorer", "Downloads", false, false), "File Explorer window hides dock");
        Assert(false, ArchDockVisibilityPolicy.ShouldShow(true, "chrome", "YouTube", false, false), "foreground app hides dock");
        Assert(true, ArchDockVisibilityPolicy.ShouldShow(true, "chrome", "YouTube", true, false), "bottom edge reveals dock");
        Assert(true, ArchDockVisibilityPolicy.ShouldShow(true, "chrome", "YouTube", false, true), "hover keeps dock visible");
        Assert(true, ArchDockVisibilityPolicy.ShouldShow(true, "ArchPanel", "Arch Panel", false, false), "top panel interaction keeps dock visible");
        return failures == 0 ? 0 : 1;
    }
}
