using System;
using System.IO;
using System.Windows;

public class TestProgram {
    [STAThread]
    public static void Main() {
        string log = @"c:\Users\labta\Downloads\theme\test_log.txt";
        try {
            File.WriteAllText(log, "1. Starting\n");
            Window w = new Window {
                Title = "Test Window",
                Width = 300,
                Height = 200
            };
            w.ShowDialog();
        } catch (Exception ex) {
            File.AppendAllText(log, "CATCH: " + ex.ToString() + "\n");
        }
    }
}
