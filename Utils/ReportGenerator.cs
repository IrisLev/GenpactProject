using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenpactProject.Utils;

public class TestResult
{
    public string TestName { get; set; }
    public bool Passed { get; set; }
    public string Message { get; set; }

    public TestResult(string testName, bool passed, string message = "")
    {
        TestName = testName;
        Passed = passed;
        Message = message;
    }
}

public static class ReportGenerator
{
    private static readonly List<TestResult> Results = new();

    public static void AddResult(string testName, bool passed, string message = "")
    {
        Results.Add(new TestResult(testName, passed, message));
    }

    public static void GenerateHtmlReport(string outputPath = "TestReport.html")
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><title>Test Report</title>");
        sb.AppendLine("<style>body{font-family:Arial;}table{border-collapse:collapse;width:100%;}th,td{border:1px solid #ddd;padding:8px;}th{background-color:#f2f2f2;}tr:nth-child(even){background-color:#f9f9f9;}.pass{color:green;}.fail{color:red;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<h1>Test Report</h1>");
        sb.AppendLine("<table><tr><th>Test Name</th><th>Status</th><th>Message</th></tr>");

        foreach (var result in Results)
        {
            var status = result.Passed ? "Pass" : "Fail";
            var cssClass = result.Passed ? "pass" : "fail";
            sb.AppendLine($"<tr><td>{result.TestName}</td><td class=\"{cssClass}\">{status}</td><td>{result.Message}</td></tr>");
        }

        sb.AppendLine("</table></body></html>");
        File.WriteAllText(outputPath, sb.ToString());
    }
}
