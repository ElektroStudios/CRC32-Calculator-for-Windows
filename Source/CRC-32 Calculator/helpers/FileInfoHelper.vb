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

Imports DevCase.Win32
Imports DevCase.Win32.Enums
Imports DevCase.Win32.NativeMethods

#End Region

#Region " FileInfoHelper "

''' <summary>
''' Provides helper utilities for gathering file system information.
''' </summary>
Friend Module FileInfoHelper

#Region " Fields "

    ''' <summary>
    ''' Holds the strict sequential mapping of <see cref="FileAttributes"/> flags to their respective 
    ''' character indicators, mirroring the standard MS-DOS ATTRIB.exe command output order.
    ''' </summary>
    Private ReadOnly FileAttributesOrderedMapping As (Flag As FileAttributes, Character As String)() = {
        (FileAttributes.Archive, "A"),
        (FileAttributes.System, "S"),
        (FileAttributes.Hidden, "H"),
        (FileAttributes.ReadOnly, "R"),
        (FileAttributes.Offline, "O"),
        (FileAttributes.NotContentIndexed, "I"),
        (FileAttributes.NoScrubData, "X")
    }

#End Region

#Region " Static Methods "

    ''' <summary>
    ''' Retrieves metadata attributes and formatted size metrics for a specified file, 
    ''' returning a unified representation string.
    ''' </summary>
    ''' 
    ''' <param name="filePath">
    ''' The absolute target file system path string to evaluate.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A formatted <see cref="String"/> containing the byte size and DOS file attributes.
    ''' </returns>
    <DebuggerStepperBoundary>
    Friend Function BuildFileInfoString(filePath As String) As String

        Dim fInfo As New FileInfo(filePath)
        Dim sizeText As String = FileInfoHelper.GetFileSizeString(fInfo)
        Dim attributeList As List(Of String) = FileInfoHelper.GetFileAttributeList(fInfo)

        Return $"Size: {sizeText} | Attributes: {String.Join(" ", attributeList)}"
    End Function

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Converts the byte length of a file into a formatted, human-readable string representation 
    ''' utilizing native Windows API <see cref="ShlwApi.StrFormatByteSizeEx"/> function.
    ''' </summary>
    ''' 
    ''' <param name="fInfo">
    ''' The <see cref="FileInfo"/> instance containing the metadata and size metrics of the target file.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A formatted <see cref="String"/> representing the file size with its corresponding unit (e.g., KB, MB, GB).
    ''' </returns>
    <DebuggerStepThrough>
    Private Function GetFileSizeString(fInfo As FileInfo) As String

        Dim sizeTextBuffer As New StringBuilder(16, 32)
        Dim result As Integer =
            NativeMethods.StrFormatByteSizeEx(CULng(fInfo.Length), StrFormatByteSizeFlags.Truncate,
                                              sizeTextBuffer, CUInt(sizeTextBuffer.MaxCapacity))
        If result <> 0 Then
            Marshal.ThrowExceptionForHR(result)
        End If

        Return sizeTextBuffer.ToString()
    End Function

    ''' <summary>
    ''' Evaluates the file attributes of the specified file and compiles a list of string indicators 
    ''' matching the standard MS-DOS ATTRIB.exe command order.
    ''' </summary>
    ''' 
    ''' <param name="fInfo">
    ''' The file to evaluate.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="List(Of String)"/> containing the attribute characters ("A", "S", "H", "R", "O", "I", "X") 
    ''' or hyphens ("-") if the specific flag is missing.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function GetFileAttributeList(fInfo As FileInfo) As List(Of String)

        Dim currentAttributes As FileAttributes = fInfo.Attributes

        Return FileInfoHelper.FileAttributesOrderedMapping.Select(
            Function(mapping) As String
                Return If((currentAttributes And mapping.Flag) <> 0, mapping.Character, "-")
            End Function
        ).ToList()
    End Function

#End Region

End Module

#End Region
