using System;

internal static class MediaVisibilityPolicyTests {
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
        Assert(true, MediaVisibilityPolicy.ShouldShow("Playing", "Song title"), "playing media with title is shown");
        Assert(true, MediaVisibilityPolicy.ShouldShow("Paused", "Video title"), "paused media remains available for controls");
        Assert(false, MediaVisibilityPolicy.ShouldShow("Stopped", "Old title"), "stopped media is hidden");
        Assert(false, MediaVisibilityPolicy.ShouldShow("Playing", ""), "playing session without title is hidden");
        Assert(false, MediaVisibilityPolicy.ShouldShow(null, "Title"), "missing playback status is hidden");
        return failures == 0 ? 0 : 1;
    }
}
