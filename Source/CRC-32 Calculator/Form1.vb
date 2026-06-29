
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading.Tasks

Imports DevCase.Core.Security.DataIntegrity.Checksum
Imports DevCase.Win32
Imports DevCase.Win32.Enums

#End Region

Public NotInheritable Class Form1 : Inherits Form

#Region " Event Handlers "

    <DebuggerStepThrough>
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Text = $"{My.Application.Info.ProductName} {My.Application.Info.Version.ToString(fieldCount:=3)} — ElektroStudios"
    End Sub

    <DebuggerStepThrough>
    Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs) Handles MyBase.DragEnter
        Dim isFile As Boolean = e.Data.GetDataPresent(DataFormats.FileDrop)

        If isFile Then
            Dim filePaths As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())

            If filePaths IsNot Nothing AndAlso filePaths.Length = 1 Then
                Dim rawPath As String = filePaths(0)
                Dim extendedPath As String = Me.GetExtendedPath(rawPath)

                If File.Exists(extendedPath) Then
                    e.Effect = DragDropEffects.Copy
                    Exit Sub
                End If
            End If
        End If

        e.Effect = DragDropEffects.None
    End Sub

    <DebuggerStepThrough>
    Private Sub Form1_DragDrop(sender As Object, e As DragEventArgs) Handles MyBase.DragDrop

        Dim filePaths As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())
        Dim selectedPath As String = filePaths.Single()

        Me.SetTextBoxFilePath(selectedPath)
    End Sub

    <DebuggerStepThrough>
    Private Sub ButtonBrowse_Click(sender As Object, e As EventArgs) Handles ButtonBrowse.Click

        If Me.OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            Me.SetTextBoxFilePath(Me.OpenFileDialog1.FileName)
        End If
    End Sub

    <DebuggerStepperBoundary>
    Private Async Sub TextBoxFile_TextChanged(sender As Object, e As EventArgs) Handles TextBoxFile.TextChanged

        Dim tb As TextBox = DirectCast(sender, TextBox)

        Me.ButtonBrowse.Enabled = False
        Me.ButtonCopy.Enabled = False
        Me.ButtonClear.Enabled = False
        Me.TextBoxFile.Enabled = False
        Me.TextBoxCrc.Enabled = False
        Me.TextBoxCrc.Clear()
        Me.ToolStripStatusLabel1.Text = "Waiting..."

        If Not String.IsNullOrWhiteSpace(tb.Text) Then
            Me.UseWaitCursor = True
            Dim pathForProcessing As String = Me.GetExtendedPath(tb.Text)

            Try
                Me.ShowFileInfo(pathForProcessing)

                Dim crc32 As String =
                    Await Task.Run(Function()
                                       Return UtilChecksum.ComputeCRC32OfFile(pathForProcessing)
                                   End Function)

                Me.TextBoxCrc.Text = crc32
                Me.ButtonBrowse.Enabled = True

                Me.ToolTip1.Show("File loaded.", Me.ButtonBrowse, 0, +Me.ButtonBrowse.Height, 1500)
            Catch ex As Exception
                MessageBox.Show(Me, $"ERROR: {ex.Message}", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Me.TextBoxCrc.Text = "ERROR"

            Finally
                Me.UseWaitCursor = False
                Me.TextBoxFile.Enabled = True
                Me.TextBoxCrc.Enabled = True
                Me.ButtonCopy.Enabled = True
                Me.ButtonClear.Enabled = True

            End Try
        End If

        Me.ButtonBrowse.Enabled = True
    End Sub

    <DebuggerStepThrough>
    Private Sub ButtonCopy_Click(sender As Object, e As EventArgs) Handles ButtonCopy.Click

        Clipboard.SetText(Me.TextBoxCrc.Text)

        Me.ToolTip1.Show("CRC-32 copied to the clipboard.", Me.ButtonCopy, 0, +Me.ButtonCopy.Height, 1500)
    End Sub

    <DebuggerStepThrough>
    Private Sub ButtonClear_Click(sender As Object, e As EventArgs) Handles ButtonClear.Click

        Me.TextBoxFile.Clear()
        Me.ButtonBrowse.Focus()

        Me.ToolTip1.Show("Values cleared.", Me.TextBoxFile, 0, +Me.TextBoxFile.Height, 1500)
    End Sub

    <DebuggerStepThrough>
    Private Sub LinkLabelWebsite_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)

        Process.Start("https://github.com/ElektroStudios/CRC32-Calculator-for-Windows")
    End Sub

    <DebuggerStepThrough>
    Private Sub LinkLabelGithub_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabelGithub.LinkClicked

        Dim msgResult As DialogResult =
            MessageBox.Show(Me, $"Do you want to open the project's GitHub page in your default web browser?",
                            My.Application.Info.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2)

        If msgResult = DialogResult.Yes Then

            Try
                Process.Start("https://github.com/ElektroStudios/CRC32-Calculator-for-Windows")

            Catch ex As Exception
                MessageBox.Show(Me, $"ERROR: {ex.Message}", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error)

            End Try
        End If
    End Sub

#End Region

#Region " Private Methods "

    <DebuggerStepThrough>
    Private Sub SetTextBoxFilePath(filePath As String)

        Me.TextBoxFile.Text = filePath

        Me.TextBoxFile.SelectionStart = Me.TextBoxFile.Text.Length
        Me.TextBoxFile.SelectionLength = 0
        Me.TextBoxFile.Focus()
    End Sub

    <DebuggerStepThrough>
    Public Sub ShowFileInfo(filePath As String)

        Dim fileinfo As New FileInfo(filePath)

        Dim sizeTextBuffer As New StringBuilder(8, 32)
        Dim result As Integer =
            NativeMethods.StrFormatByteSizeEx(CULng(fileinfo.Length), StrFormatByteSizeFlags.Truncate,
                                              sizeTextBuffer, CUInt(sizeTextBuffer.MaxCapacity))
        If result <> 0 Then
            Marshal.ThrowExceptionForHR(result)
        End If

        Dim attributes As FileAttributes = fileinfo.Attributes
        Dim attributeList As New List(Of String)

        ' Note: Same order as ATTRIB.exe command.
        If (attributes And FileAttributes.Archive) <> 0 Then attributeList.Add("A")
        If (attributes And FileAttributes.System) <> 0 Then attributeList.Add("S")
        If (attributes And FileAttributes.Hidden) <> 0 Then attributeList.Add("H")
        If (attributes And FileAttributes.ReadOnly) <> 0 Then attributeList.Add("R")
        If (attributes And FileAttributes.Offline) <> 0 Then attributeList.Add("O")
        If (attributes And FileAttributes.NotContentIndexed) <> 0 Then attributeList.Add("I")
        If (attributes And FileAttributes.NoScrubData) <> 0 Then attributeList.Add("X")

        Me.ToolStripStatusLabel1.Text = $"Size: {sizeTextBuffer} | Attributes: {String.Join(", ", attributeList)}"
    End Sub

    ''' <summary>
    ''' Converts a standard path into an Extended-Length Path (prefixed with \\?\) to bypass the 260 character MAX_PATH limitation.
    ''' </summary>
    ''' 
    ''' <param name="targetPath">
    ''' The absolute path to convert.
    ''' </param>
    ''' 
    ''' <returns>
    ''' The extended-length path string.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function GetExtendedPath(targetPath As String) As String

        If String.IsNullOrWhiteSpace(targetPath) Then
            Return targetPath
        End If

        ' Already an extended path.
        If targetPath.StartsWith("\\?\", StringComparison.Ordinal) Then
            Return targetPath
        End If

        ' Relative paths cannot be converted to Extended-Length paths.
        If Not Path.IsPathRooted(targetPath) Then
            Return targetPath
        End If

        ' Handle UNC paths: \\Server\Share -> \\?\UNC\Server\Share
        If targetPath.StartsWith("\\", StringComparison.Ordinal) Then
            Return $"\\?\UNC\{targetPath.Substring(2)}"
        End If

        ' Handle Local paths: C:\Folder -> \\?\C:\Folder
        Return $"\\?\{targetPath}"
    End Function

#End Region

End Class