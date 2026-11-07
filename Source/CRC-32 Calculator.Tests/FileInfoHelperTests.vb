#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Imports Crc32Calculator.Tests.Helpers

Imports DevCase.Win32.Enums
Imports DevCase.Win32.NativeMethods

#End Region

#Region " FileInfoHelperTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for <see cref="FileInfoHelper.BuildFileInfoString(String)"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class FileInfoHelperTests

#Region " Fields "

        ''' <summary>
        ''' The expected format of the file info string:
        ''' <c>Size: {size} | Attributes: {A} {S} {H} {R} {O} {I} {X}</c>.
        ''' </summary>
        Private Shared ReadOnly FileInfoStringRegex As New Regex(
            "^Size: (?<size>.+) \| Attributes: (?<attributes>[A-] [S-] [H-] [R-] [O-] [I-] [X-])$",
            RegexOptions.CultureInvariant)

        ''' <summary>
        ''' The temporary directory where the test files are created.
        ''' </summary>
        Private tempDir As TempDirectory

#End Region

#Region " Test Initialization / Cleanup "

        ''' <summary>
        ''' Creates a new temporary directory before each test.
        ''' </summary>
        <TestInitialize>
        Public Sub TestInitialize()
            Me.tempDir = New TempDirectory()
        End Sub

        ''' <summary>
        ''' Deletes the temporary directory after each test.
        ''' </summary>
        <TestCleanup>
        Public Sub TestCleanup()
            Me.tempDir?.Dispose()
        End Sub

#End Region

#Region " Format "

        ''' <summary>
        ''' Verifies that the result matches the expected format.
        ''' </summary>
        <TestMethod>
        Public Sub BuildFileInfoString_ExistingFile_MatchesExpectedFormat()

            Dim filePath As String = Me.tempDir.CreateFile("format.txt", Encoding.ASCII.GetBytes("123456789"))

            Dim actual As String = FileInfoHelper.BuildFileInfoString(filePath)

            Assert.IsTrue(FileInfoHelperTests.FileInfoStringRegex.IsMatch(actual), $"Unexpected format: '{actual}'.")
        End Sub

#End Region

#Region " Size "

        ''' <summary>
        ''' Verifies that the size text equals the output of the Windows API <c>StrFormatByteSizeEx</c>
        ''' with the <see cref="StrFormatByteSizeFlags.Truncate"/> flag (the same text shown by Windows Explorer),
        ''' independently of the current locale.
        ''' </summary>
        '''
        ''' <param name="length">
        ''' The file length, in bytes.
        ''' </param>
        <TestMethod>
        <DataRow(0)>
        <DataRow(1)>
        <DataRow(1023)>
        <DataRow(1024)>
        <DataRow(1535, DisplayName:="1535 bytes (truncated, not rounded)")>
        <DataRow(1048576)>
        <DataRow(5 * 1048576 + 123)>
        Public Sub BuildFileInfoString_FileOfGivenLength_SizeTextMatchesWindowsApi(length As Integer)

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile($"size_{length}.bin", length, content)

            Dim actual As String = FileInfoHelper.BuildFileInfoString(filePath)

            Dim sizeText As String = FileInfoHelperTests.FileInfoStringRegex.Match(actual).Groups("size").Value
            Assert.AreEqual(FileInfoHelperTests.FormatByteSize(CULng(length), StrFormatByteSizeFlags.Truncate), sizeText)
        End Sub

        ''' <summary>
        ''' Verifies that the size unit of a file of several kilobytes is expressed in kilobytes.
        ''' </summary>
        <TestMethod>
        Public Sub BuildFileInfoString_KilobytesFile_SizeTextIsExpressedInKilobytes()

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile("kb.bin", 10 * 1024, content)

            Dim actual As String = FileInfoHelper.BuildFileInfoString(filePath)

            Dim sizeText As String = FileInfoHelperTests.FileInfoStringRegex.Match(actual).Groups("size").Value
            StringAssert.StartsWith(sizeText, "10")
            StringAssert.Contains(sizeText, "KB")
        End Sub

#End Region

#Region " Attributes "

        ''' <summary>
        ''' Verifies that each file attribute flag is mapped to its character, at its fixed position
        ''' (ATTRIB.exe order), and that missing flags are rendered as hyphens.
        ''' </summary>
        '''
        ''' <param name="attributes">
        ''' The attributes to apply to the test file.
        ''' </param>
        '''
        ''' <param name="expected">
        ''' The expected attributes text.
        ''' </param>
        <TestMethod>
        <DataRow(FileAttributes.Normal, "- - - - - - -", DisplayName:="Normal")>
        <DataRow(FileAttributes.Archive, "A - - - - - -", DisplayName:="Archive")>
        <DataRow(FileAttributes.System, "- S - - - - -", DisplayName:="System")>
        <DataRow(FileAttributes.Hidden, "- - H - - - -", DisplayName:="Hidden")>
        <DataRow(FileAttributes.ReadOnly, "- - - R - - -", DisplayName:="ReadOnly")>
        <DataRow(FileAttributes.Offline, "- - - - O - -", DisplayName:="Offline")>
        <DataRow(FileAttributes.NotContentIndexed, "- - - - - I -", DisplayName:="NotContentIndexed")>
        <DataRow(FileAttributes.Archive Or FileAttributes.Hidden Or FileAttributes.ReadOnly, "A - H R - - -", DisplayName:="Archive + Hidden + ReadOnly")>
        <DataRow(FileAttributes.Archive Or FileAttributes.System Or FileAttributes.Hidden Or FileAttributes.ReadOnly Or FileAttributes.Offline Or FileAttributes.NotContentIndexed,
                 "A S H R O I -", DisplayName:="All settable flags")>
        Public Sub BuildFileInfoString_FileWithAttributes_RendersExpectedAttributes(attributes As FileAttributes, expected As String)

            Dim filePath As String = Me.tempDir.CreateFile("attributes.txt", Encoding.ASCII.GetBytes("data"))
            File.SetAttributes(filePath, attributes)

            Dim actual As String = FileInfoHelper.BuildFileInfoString(filePath)

            Dim attributesText As String = FileInfoHelperTests.FileInfoStringRegex.Match(actual).Groups("attributes").Value
            Assert.AreEqual(expected, attributesText, $"Full string: '{actual}'.")
        End Sub

        ''' <summary>
        ''' Verifies that unrelated attributes (not shown by ATTRIB.exe) are ignored.
        ''' </summary>
        <TestMethod>
        Public Sub BuildFileInfoString_FileWithUnmappedAttribute_IgnoresUnmappedAttribute()

            Dim filePath As String = Me.tempDir.CreateFile("temporary.txt", Encoding.ASCII.GetBytes("data"))
            File.SetAttributes(filePath, FileAttributes.Temporary Or FileAttributes.Archive)

            Dim actual As String = FileInfoHelper.BuildFileInfoString(filePath)

            Dim attributesText As String = FileInfoHelperTests.FileInfoStringRegex.Match(actual).Groups("attributes").Value
            Assert.AreEqual("A - - - - - -", attributesText)
        End Sub

#End Region

#Region " Paths and Errors "

        ''' <summary>
        ''' Verifies that the Extended-Length path of a file produces the same result as its standard path.
        ''' </summary>
        <TestMethod>
        Public Sub BuildFileInfoString_ExtendedLengthPath_ReturnsSameResultAsStandardPath()

            Dim filePath As String = Me.tempDir.CreateFile("extended.txt", Encoding.ASCII.GetBytes("123456789"))

            Dim standardResult As String = FileInfoHelper.BuildFileInfoString(filePath)
            Dim extendedResult As String = FileInfoHelper.BuildFileInfoString(PathHelper.GetExtendedPath(filePath))

            Assert.AreEqual(standardResult, extendedResult)
        End Sub

        ''' <summary>
        ''' Verifies that a file whose path is longer than MAX_PATH can be evaluated through its Extended-Length path.
        ''' </summary>
        <TestMethod>
        Public Sub BuildFileInfoString_PathLongerThanMaxPath_ReturnsWellFormedString()

            Dim longFilePath As String = Me.tempDir.CreateLongPathFile(Encoding.ASCII.GetBytes("123456789"))

            Dim actual As String = FileInfoHelper.BuildFileInfoString(PathHelper.GetExtendedPath(longFilePath))

            Assert.IsTrue(FileInfoHelperTests.FileInfoStringRegex.IsMatch(actual), $"Unexpected format: '{actual}'.")
        End Sub

        ''' <summary>
        ''' Verifies that a non-existent file throws <see cref="FileNotFoundException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub BuildFileInfoString_NonExistentFile_ThrowsFileNotFoundException()

            Dim filePath As String = Path.Combine(Me.tempDir.FullPath, "does_not_exist.txt")

            Assert.ThrowsExactly(Of FileNotFoundException)(Sub() FileInfoHelper.BuildFileInfoString(filePath))
        End Sub

        ''' <summary>
        ''' Verifies that a null path throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub BuildFileInfoString_NullPath_ThrowsArgumentNullException()

            Assert.ThrowsExactly(Of ArgumentNullException)(Sub() FileInfoHelper.BuildFileInfoString(Nothing))
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Formats the specified number of bytes using the Windows API <c>StrFormatByteSizeEx</c> function.
        ''' </summary>
        '''
        ''' <param name="number">
        ''' The number of bytes.
        ''' </param>
        '''
        ''' <param name="flags">
        ''' The rounding flags.
        ''' </param>
        '''
        ''' <returns>
        ''' The formatted size string.
        ''' </returns>
        Private Shared Function FormatByteSize(number As ULong, flags As StrFormatByteSizeFlags) As String

            Dim buffer As New StringBuilder(32)
            Dim hResult As Integer = ShlwApi.StrFormatByteSizeEx(number, flags, buffer, CUInt(buffer.Capacity))
            If hResult <> 0 Then
                Throw New InvalidOperationException($"StrFormatByteSizeEx failed with HRESULT 0x{hResult:X8}.")
            End If

            Return buffer.ToString()
        End Function

#End Region

    End Class

End Namespace

#End Region
