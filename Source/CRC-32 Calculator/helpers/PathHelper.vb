
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.IO
Imports System.Security

#End Region

#Region " PathHelper "

''' <summary>
''' Provides helper methods for manipulating Windows file system paths.
''' </summary>
Friend Module PathHelper

#Region " Constants "

    ''' <summary>
    ''' The prefix that identifies a Win32 Extended-Length path (e.g. <c>\\?\C:\Folder</c>).
    ''' </summary>
    Private Const ExtendedPathPrefix As String = "\\?\"

    ''' <summary>
    ''' The prefix that identifies a Win32 device namespace path (e.g. <c>\\.\PhysicalDrive0</c>).
    ''' </summary>
    Private Const DevicePathPrefix As String = "\\.\"

#End Region

#Region " Static Methods "

    ''' <summary>
    ''' Converts a standard path into an Extended-Length Path (prefixed with \\?\) to bypass the 260 character MAX_PATH limitation.
    ''' </summary>
    '''
    ''' <remarks>
    ''' Extended-Length paths are passed verbatim to the file system without any normalization,
    ''' so the path is fully normalized (forward slashes, <c>.</c> and <c>..</c> segments, etc.) before adding the prefix.
    ''' <para></para>
    ''' Relative paths, including drive-relative (<c>C:file.txt</c>) and root-relative (<c>\Folder\file.txt</c>) paths,
    ''' are returned unchanged because they cannot be converted to Extended-Length paths.
    ''' </remarks>
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

        ' Already an extended path, or a device namespace path.
        If targetPath.StartsWith(PathHelper.ExtendedPathPrefix, StringComparison.Ordinal) OrElse
           targetPath.StartsWith(PathHelper.DevicePathPrefix, StringComparison.Ordinal) Then
            Return targetPath
        End If

        ' Relative paths (including drive-relative and root-relative paths) cannot be converted to Extended-Length paths.
        If Not PathHelper.IsPathFullyQualified(targetPath) Then
            Return targetPath
        End If

        ' Normalize the path, because the Extended-Length prefix disables any normalization done by Win32 APIs.
        Dim fullPath As String
        Try
            fullPath = Path.GetFullPath(targetPath)

        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse
                                   TypeOf ex Is NotSupportedException OrElse
                                   TypeOf ex Is PathTooLongException OrElse
                                   TypeOf ex Is SecurityException
            ' Malformed path: return it unchanged so the subsequent I/O operation reports the failure to the caller.
            Return targetPath

        End Try

        ' Handle UNC paths: \\Server\Share -> \\?\UNC\Server\Share
        If fullPath.StartsWith("\\", StringComparison.Ordinal) Then
            Return $"{PathHelper.ExtendedPathPrefix}UNC\{fullPath.Substring(2)}"
        End If

        ' Handle Local paths: C:\Folder -> \\?\C:\Folder
        Return $"{PathHelper.ExtendedPathPrefix}{fullPath}"
    End Function

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Determines whether the specified path is fully qualified,
    ''' that is, a drive-rooted path (<c>C:\Folder</c>) or a UNC path (<c>\\Server\Share</c>).
    ''' </summary>
    '''
    ''' <param name="targetPath">
    ''' The path to evaluate.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the path is fully qualified; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function IsPathFullyQualified(targetPath As String) As Boolean

        ' UNC path: \\Server\Share (or //Server/Share).
        If targetPath.Length >= 2 AndAlso
           PathHelper.IsDirectorySeparator(targetPath(0)) AndAlso
           PathHelper.IsDirectorySeparator(targetPath(1)) Then
            Return True
        End If

        ' Drive-rooted path: C:\Folder (or C:/Folder).
        Return targetPath.Length >= 3 AndAlso
               PathHelper.IsAsciiLetter(targetPath(0)) AndAlso
               targetPath(1) = ":"c AndAlso
               PathHelper.IsDirectorySeparator(targetPath(2))
    End Function

    ''' <summary>
    ''' Determines whether the specified character is a directory separator (<c>\</c> or <c>/</c>).
    ''' </summary>
    '''
    ''' <param name="c">
    ''' The character to evaluate.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the character is a directory separator; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function IsDirectorySeparator(c As Char) As Boolean

        Return c = "\"c OrElse c = "/"c
    End Function

    ''' <summary>
    ''' Determines whether the specified character is an ASCII letter (a valid drive letter).
    ''' </summary>
    '''
    ''' <param name="c">
    ''' The character to evaluate.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the character is an ASCII letter; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function IsAsciiLetter(c As Char) As Boolean

        Return (c >= "A"c AndAlso c <= "Z"c) OrElse (c >= "a"c AndAlso c <= "z"c)
    End Function

#End Region

End Module

#End Region
