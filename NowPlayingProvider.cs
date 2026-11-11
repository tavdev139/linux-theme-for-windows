using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;

public sealed class NowPlayingInfo {
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Album { get; set; }
    public string SourceApp { get; set; }
    public string PlaybackStatus { get; set; }
    public byte[] ThumbnailBytes { get; set; }

    public bool IsPlaying {
        get { return String.Equals(PlaybackStatus, "Playing", StringComparison.OrdinalIgnoreCase); }
    }
}

public static class MediaVisibilityPolicy {
    public static bool ShouldShow(string playbackStatus, string title) {
        if (String.IsNullOrWhiteSpace(title)) return false;
        return String.Equals(playbackStatus, "Playing", StringComparison.OrdinalIgnoreCase) ||
               String.Equals(playbackStatus, "Paused", StringComparison.OrdinalIgnoreCase);
    }
}

public static class NowPlayingProvider {
    private const string ManagerTypeName = "Windows.Media.Control.GlobalSystemMediaTransportControlsSessionManager, Windows.Media.Control, ContentType=WindowsRuntime";
    private static readonly object ThumbnailCacheLock = new object();
    private static string CachedThumbnailKey;
    private static byte[] CachedThumbnailBytes;

    private static Type GetManagerType() {
        return Type.GetType(ManagerTypeName, false);
    }

    private static MethodInfo GetAsTaskMethod() {
        Type extensions = Type.GetType("System.WindowsRuntimeSystemExtensions, System.Runtime.WindowsRuntime", false);
        if (extensions == null) {
            string framework = Environment.Is64BitProcess ? "Framework64" : "Framework";
            string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string path = Path.Combine(windows, "Microsoft.NET", framework, "v4.0.30319", "System.Runtime.WindowsRuntime.dll");
            try {
                Assembly assembly = Assembly.LoadFrom(path);
                extensions = assembly.GetType("System.WindowsRuntimeSystemExtensions", false);
            } catch { }
        }
        if (extensions == null) return null;
        return extensions.GetMethods().FirstOrDefault(method =>
            method.Name == "AsTask" && method.IsGenericMethodDefinition &&
            method.GetParameters().Length == 1 && method.GetGenericArguments().Length == 1);
    }

    private static object AwaitWinRt(object operation, Type resultType) {
        if (operation == null || resultType == null) return null;
        MethodInfo asTask = GetAsTaskMethod();
        if (asTask == null) return null;
        object taskObject = asTask.MakeGenericMethod(resultType).Invoke(null, new object[] { operation });
        Task task = taskObject as Task;
        if (task == null) return null;
        task.GetAwaiter().GetResult();
        PropertyInfo result = task.GetType().GetProperty("Result");
        return result == null ? null : result.GetValue(task, null);
    }

    private static object GetCurrentSession() {
        Type managerType = GetManagerType();
        if (managerType == null) return null;
        MethodInfo request = managerType.GetMethod("RequestAsync", BindingFlags.Public | BindingFlags.Static);
        if (request == null) return null;
        object manager = AwaitWinRt(request.Invoke(null, null), request.ReturnType.GetGenericArguments()[0]);
        if (manager == null) return null;
        MethodInfo getCurrent = managerType.GetMethod("GetCurrentSession", BindingFlags.Public | BindingFlags.Instance);
        return getCurrent == null ? null : getCurrent.Invoke(manager, null);
    }

    private static string ReadString(object instance, string propertyName) {
        if (instance == null) return String.Empty;
        PropertyInfo property = instance.GetType().GetProperty(propertyName);
        object value = property == null ? null : property.GetValue(instance, null);
        return value == null ? String.Empty : value.ToString();
    }

    private static byte[] ReadThumbnail(object properties, string cacheKey) {
        lock (ThumbnailCacheLock) {
            if (String.Equals(CachedThumbnailKey, cacheKey, StringComparison.Ordinal)) return CachedThumbnailBytes;
        }

        byte[] bytes = null;
        try {
            object thumbnail = properties.GetType().GetProperty("Thumbnail").GetValue(properties, null);
            if (thumbnail != null) {
                MethodInfo openRead = thumbnail.GetType().GetMethod("OpenReadAsync");
                if (openRead != null) {
                    Type streamType = openRead.ReturnType.GetGenericArguments()[0];
                    object asyncStream = AwaitWinRt(openRead.Invoke(thumbnail, null), streamType);
                    string framework = Environment.Is64BitProcess ? "Framework64" : "Framework";
                    string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    string path = Path.Combine(windows, "Microsoft.NET", framework, "v4.0.30319", "System.Runtime.WindowsRuntime.dll");
                    Assembly runtimeAssembly = Assembly.LoadFrom(path);
                    Type streamExtensions = runtimeAssembly.GetTypes().FirstOrDefault(type => type.Name == "WindowsRuntimeStreamExtensions");
                    MethodInfo asStream = streamExtensions == null ? null : streamExtensions.GetMethods().FirstOrDefault(method => method.Name == "AsStreamForRead" && method.GetParameters().Length == 1);
                    if (asyncStream != null && asStream != null) {
                        using (Stream stream = (Stream)asStream.Invoke(null, new object[] { asyncStream }))
                        using (MemoryStream memory = new MemoryStream()) {
                            stream.CopyTo(memory);
                            if (memory.Length > 0 && memory.Length <= 5 * 1024 * 1024) bytes = memory.ToArray();
                        }
                    }
                }
            }
        } catch { }

        lock (ThumbnailCacheLock) {
            CachedThumbnailKey = cacheKey;
            CachedThumbnailBytes = bytes;
        }
        return bytes;
    }

    public static NowPlayingInfo GetCurrent() {
        try {
            object session = GetCurrentSession();
            if (session == null) return null;
            Type sessionType = session.GetType();
            MethodInfo propertiesMethod = sessionType.GetMethod("TryGetMediaPropertiesAsync");
            if (propertiesMethod == null) return null;
            Type propertiesType = propertiesMethod.ReturnType.GetGenericArguments()[0];
            object properties = AwaitWinRt(propertiesMethod.Invoke(session, null), propertiesType);
            if (properties == null) return null;

            MethodInfo playbackMethod = sessionType.GetMethod("GetPlaybackInfo");
            object playback = playbackMethod == null ? null : playbackMethod.Invoke(session, null);
            string status = ReadString(playback, "PlaybackStatus");
            string title = ReadString(properties, "Title");
            if (!MediaVisibilityPolicy.ShouldShow(status, title)) return null;

            string artist = ReadString(properties, "Artist");
            string album = ReadString(properties, "AlbumTitle");
            string sourceApp = ReadString(session, "SourceAppUserModelId");
            string thumbnailKey = title + "|" + artist + "|" + album + "|" + sourceApp;
            return new NowPlayingInfo {
                Title = title,
                Artist = artist,
                Album = album,
                SourceApp = sourceApp,
                PlaybackStatus = status,
                ThumbnailBytes = ReadThumbnail(properties, thumbnailKey)
            };
        } catch {
            return null;
        }
    }

    public static bool TogglePlayPause() { return SendCommand("TryTogglePlayPauseAsync"); }
    public static bool SkipNext() { return SendCommand("TrySkipNextAsync"); }
    public static bool SkipPrevious() { return SendCommand("TrySkipPreviousAsync"); }

    private static bool SendCommand(string methodName) {
        try {
            object session = GetCurrentSession();
            if (session == null) return false;
            MethodInfo command = session.GetType().GetMethod(methodName);
            if (command == null) return false;
            Type resultType = command.ReturnType.GetGenericArguments()[0];
            object result = AwaitWinRt(command.Invoke(session, null), resultType);
            return result != null && Convert.ToBoolean(result);
        } catch {
            return false;
        }
    }
}
