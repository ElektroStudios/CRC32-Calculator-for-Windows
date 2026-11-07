#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Text.RegularExpressions

#End Region

#Region " AppGlobalsTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="AppGlobals"/> module.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class AppGlobalsTests

        ''' <summary>
        ''' Verifies that the GitHub repository URL is a well-formed, absolute HTTPS URL
        ''' pointing to the official repository (it is opened with <c>Process.Start</c>).
        ''' </summary>
        <TestMethod>
        Public Sub GitHubRepositoryUrl_Value_IsAbsoluteHttpsUrlToOfficialRepository()

            Dim uri As Uri = Nothing
            Dim isValid As Boolean = Uri.TryCreate(AppGlobals.GitHubRepositoryUrl, UriKind.Absolute, uri)

            Assert.IsTrue(isValid, "The URL is not a well-formed absolute URI.")
            Assert.AreEqual(Uri.UriSchemeHttps, uri.Scheme)
            Assert.AreEqual("github.com", uri.Host)
            Assert.AreEqual("/ElektroStudios/CRC32-Calculator-for-Windows", uri.AbsolutePath)
        End Sub

        ''' <summary>
        ''' Verifies that the application title has the format <c>{ProductName} {Major.Minor.Build} — ElektroStudios</c>.
        ''' </summary>
        '''
        ''' <remarks>
        ''' The product name and version come from <c>My.Application.Info</c>, which resolves to the entry assembly
        ''' (the test host when running tests), so only the format is verified.
        ''' </remarks>
        <TestMethod>
        Public Sub AppTitle_Value_HasProductNameVersionAndAuthor()

            Dim titleRegex As New Regex("^.* \d+\.\d+\.\d+ — ElektroStudios$")

            Assert.IsTrue(titleRegex.IsMatch(AppGlobals.AppTitle), $"Unexpected application title: '{AppGlobals.AppTitle}'.")
        End Sub

    End Class

End Namespace

#End Region
