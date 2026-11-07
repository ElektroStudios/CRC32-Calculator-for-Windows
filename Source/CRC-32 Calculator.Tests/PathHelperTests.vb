#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Text

Imports Crc32Calculator.Tests.Helpers

#End Region

#Region " PathHelperTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for <see cref="PathHelper.GetExtendedPath(String)"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class PathHelperTests

#Region " Unchanged Inputs "

        ''' <summary>
        ''' Verifies that a null path is returned unchanged.
        ''' </summary>
        <TestMethod>
        Public Sub GetExtendedPath_NullPath_ReturnsNull()

            Assert.IsNull(PathHelper.GetExtendedPath(Nothing))
        End Sub

        ''' <summary>
        ''' Verifies that empty and whitespace-only paths are returned unchanged.
        ''' </summary>
        '''
        ''' <param name="input">
        ''' The input path.
        ''' </param>
        <TestMethod>
        <DataRow("", DisplayName:="Empty")>
        <DataRow("   ", DisplayName:="Spaces")>
        <DataRow(vbTab, DisplayName:="Tab")>
        Public Sub GetExtendedPath_EmptyOrWhitespacePath_ReturnsInputUnchanged(input As String)

            Assert.AreEqual(input, PathHelper.GetExtendedPath(input))
        End Sub

        ''' <summary>
        ''' Verifies that paths that already are Extended-Length or device namespace paths are returned unchanged.
        ''' </summary>
        '''
        ''' <param name="input">
        ''' The input path.
        ''' </param>
        <TestMethod>
        <DataRow("\\?\C:\Folder\file.txt", DisplayName:="Extended local path")>
        <DataRow("\\?\UNC\Server\Share\file.txt", DisplayName:="Extended UNC path")>
        <DataRow("\\?\Volume{00000000-0000-0000-0000-000000000000}\file.txt", DisplayName:="Extended volume GUID path")>
        <DataRow("\\.\PhysicalDrive0", DisplayName:="Device namespace path")>
        <DataRow("\\.\C:", DisplayName:="Device namespace volume")>
        Public Sub GetExtendedPath_AlreadyExtendedOrDevicePath_ReturnsInputUnchanged(input As String)

            Assert.AreEqual(input, PathHelper.GetExtendedPath(input))
        End Sub

        ''' <summary>
        ''' Verifies that relative paths, including drive-relative and root-relative paths, are returned unchanged,
        ''' because they cannot be converted to Extended-Length paths.
        ''' </summary>
        '''
        ''' <param name="input">
        ''' The input path.
        ''' </param>
        <TestMethod>
        <DataRow("file.txt", DisplayName:="File name")>
        <DataRow("Folder\file.txt", DisplayName:="Relative path")>
        <DataRow(".\file.txt", DisplayName:="Current directory relative")>
        <DataRow("..\file.txt", DisplayName:="Parent directory relative")>
        <DataRow("C:file.txt", DisplayName:="Drive-relative path")>
        <DataRow("\Folder\file.txt", DisplayName:="Root-relative path")>
        <DataRow("/Folder/file.txt", DisplayName:="Root-relative path with forward slashes")>
        Public Sub GetExtendedPath_RelativePath_ReturnsInputUnchanged(input As String)

            Assert.AreEqual(input, PathHelper.GetExtendedPath(input))
        End Sub

#End Region

#Region " Conversions "

        ''' <summary>
        ''' Verifies the conversion of fully qualified local and UNC paths, including their normalization
        ''' (Extended-Length paths are not normalized by Win32, so forward slashes and dot segments would break them).
        ''' </summary>
        '''
        ''' <param name="input">
        ''' The input path.
        ''' </param>
        '''
        ''' <param name="expected">
        ''' The expected Extended-Length path.
        ''' </param>
        <TestMethod>
        <DataRow("C:\Folder\file.txt", "\\?\C:\Folder\file.txt", DisplayName:="Local path")>
        <DataRow("C:\", "\\?\C:\", DisplayName:="Drive root")>
        <DataRow("C:\Folder With Spaces\file name.txt", "\\?\C:\Folder With Spaces\file name.txt", DisplayName:="Path with spaces")>
        <DataRow("C:/Folder/file.txt", "\\?\C:\Folder\file.txt", DisplayName:="Forward slashes")>
        <DataRow("C:\Folder\\Sub\file.txt", "\\?\C:\Folder\Sub\file.txt", DisplayName:="Duplicated separators")>
        <DataRow("C:\Folder\.\file.txt", "\\?\C:\Folder\file.txt", DisplayName:="Current directory segment")>
        <DataRow("C:\Folder\Sub\..\file.txt", "\\?\C:\Folder\file.txt", DisplayName:="Parent directory segment")>
        <DataRow("\\Server\Share\file.txt", "\\?\UNC\Server\Share\file.txt", DisplayName:="UNC path")>
        <DataRow("//Server/Share/file.txt", "\\?\UNC\Server\Share\file.txt", DisplayName:="UNC path with forward slashes")>
        <DataRow("\\Server\Share\Folder\..\file.txt", "\\?\UNC\Server\Share\file.txt", DisplayName:="UNC path with parent segment")>
        Public Sub GetExtendedPath_FullyQualifiedPath_ReturnsNormalizedExtendedPath(input As String, expected As String)

            Assert.AreEqual(expected, PathHelper.GetExtendedPath(input))
        End Sub

        ''' <summary>
        ''' Verifies that a path longer than MAX_PATH (260 characters) is converted correctly.
        ''' </summary>
        <TestMethod>
        Public Sub GetExtendedPath_PathLongerThanMaxPath_ReturnsExtendedPath()

            Dim segment As String = New String("x"c, 100)
            Dim longPath As String = $"C:\{segment}\{segment}\{segment}\file.txt"
            Assert.IsTrue(longPath.Length > 260, "Precondition: the test path must be longer than MAX_PATH.")

            Dim actual As String = PathHelper.GetExtendedPath(longPath)

            Assert.AreEqual($"\\?\{longPath}", actual)
        End Sub

        ''' <summary>
        ''' Verifies that the conversion is idempotent.
        ''' </summary>
        '''
        ''' <param name="input">
        ''' The input path.
        ''' </param>
        <TestMethod>
        <DataRow("C:\Folder\file.txt")>
        <DataRow("\\Server\Share\file.txt")>
        <DataRow("Folder\file.txt")>
        Public Sub GetExtendedPath_AppliedTwice_ReturnsSameResultAsAppliedOnce(input As String)

            Dim once As String = PathHelper.GetExtendedPath(input)
            Dim twice As String = PathHelper.GetExtendedPath(once)

            Assert.AreEqual(once, twice)
        End Sub

        ''' <summary>
        ''' Verifies that a malformed fully qualified path does not throw and is returned unchanged,
        ''' so the subsequent I/O operation is the one that reports the failure (e.g. "file not found").
        ''' </summary>
        '''
        ''' <param name="input">
        ''' The input path.
        ''' </param>
        <TestMethod>
        <DataRow("C:\Folder\file|name.txt", DisplayName:="Invalid character")>
        <DataRow("\\Server", DisplayName:="UNC path without share")>
        Public Sub GetExtendedPath_MalformedPath_DoesNotThrow(input As String)

            Dim actual As String = PathHelper.GetExtendedPath(input)

            Assert.IsNotNull(actual)
            Assert.IsFalse(File.Exists(actual))
        End Sub

#End Region

#Region " File System Integration "

        ''' <summary>
        ''' Verifies that the converted path of real files (written in several equivalent forms) resolves to the file,
        ''' as the application does when receiving a path from the command line or drag and drop.
        ''' </summary>
        <TestMethod>
        Public Sub GetExtendedPath_EquivalentFormsOfExistingFile_ResolveToExistingFile()

            Using tempDir As New TempDirectory()
                Dim filePath As String = tempDir.CreateFile(Path.Combine("Sub", "file.txt"), Encoding.ASCII.GetBytes("data"))

                Dim forms As String() = {
                    filePath,
                    filePath.Replace("\"c, "/"c),
                    Path.Combine(tempDir.FullPath, "Sub", ".", "file.txt"),
                    Path.Combine(tempDir.FullPath, "Sub", "..", "Sub", "file.txt")
                }

                For Each form As String In forms
                    Dim extendedPath As String = PathHelper.GetExtendedPath(form)

                    StringAssert.StartsWith(extendedPath, "\\?\")
                    Assert.IsTrue(File.Exists(extendedPath), $"The file was not found through '{extendedPath}' (input: '{form}').")
                Next
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that the converted path of a real file longer than MAX_PATH resolves to the file.
        ''' </summary>
        <TestMethod>
        Public Sub GetExtendedPath_ExistingFileLongerThanMaxPath_ResolvesToExistingFile()

            Using tempDir As New TempDirectory()
                Dim longFilePath As String = tempDir.CreateLongPathFile(Encoding.ASCII.GetBytes("data"))

                Dim extendedPath As String = PathHelper.GetExtendedPath(longFilePath)

                Assert.IsTrue(File.Exists(extendedPath))
            End Using
        End Sub

#End Region

    End Class

End Namespace

#End Region
