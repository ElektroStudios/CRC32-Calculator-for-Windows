#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Threading

#End Region

#Region " TempDirectory "

Namespace Crc32Calculator.Tests.Helpers

    ''' <summary>
    ''' Represents a unique temporary directory that is recursively deleted when disposed.
    ''' <para></para>
    ''' Provides helpers to create test files, including files with special attributes and paths longer than MAX_PATH.
    ''' </summary>
    Friend NotInheritable Class TempDirectory : Implements IDisposable

#Region " Constants "

        ''' <summary>
        ''' The prefix that identifies a Win32 Extended-Length path.
        ''' </summary>
        Friend Const ExtendedPathPrefix As String = "\\?\"

#End Region

#Region " Fields "

        ''' <summary>
        ''' Flag to detect redundant calls to <see cref="TempDirectory.Dispose()"/>.
        ''' </summary>
        Private isDisposed As Boolean

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the full path of the temporary directory (without the Extended-Length prefix).
        ''' </summary>
        Friend ReadOnly Property FullPath As String

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="TempDirectory"/> class, creating a new unique temporary directory.
        ''' </summary>
        Friend Sub New()

            ' Normalize the temp path, because it is later combined with the Extended-Length prefix (which disables normalization).
            Dim tempRoot As String = Path.GetFullPath(Path.GetTempPath())
            Me.FullPath = Path.Combine(tempRoot, $"CRC32CalculatorTests_{Guid.NewGuid():N}")
            Directory.CreateDirectory(Me.FullPath)
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Creates a file with the specified content inside the temporary directory.
        ''' </summary>
        '''
        ''' <param name="fileName">
        ''' The file name, or a relative path to the temporary directory.
        ''' </param>
        '''
        ''' <param name="content">
        ''' The file content.
        ''' </param>
        '''
        ''' <returns>
        ''' The full path of the created file.
        ''' </returns>
        Friend Function CreateFile(fileName As String, content As Byte()) As String

            If content Is Nothing Then
                Throw New ArgumentNullException(NameOf(content))
            End If

            Dim filePath As String = Path.Combine(Me.FullPath, fileName)
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))
            File.WriteAllBytes(filePath, content)
            Return filePath
        End Function

        ''' <summary>
        ''' Creates a file filled with deterministic pseudo-random content inside the temporary directory.
        ''' </summary>
        '''
        ''' <param name="fileName">
        ''' The file name.
        ''' </param>
        '''
        ''' <param name="length">
        ''' The file length, in bytes.
        ''' </param>
        '''
        ''' <param name="content">
        ''' When this method returns, contains the written content.
        ''' </param>
        '''
        ''' <returns>
        ''' The full path of the created file.
        ''' </returns>
        Friend Function CreateRandomFile(fileName As String, length As Integer, ByRef content As Byte()) As String

            content = TempDirectory.CreateRandomBytes(length, seed:=length)
            Return Me.CreateFile(fileName, content)
        End Function

        ''' <summary>
        ''' Creates a file with the specified content whose full path is longer than MAX_PATH (260 characters).
        ''' </summary>
        '''
        ''' <param name="content">
        ''' The file content.
        ''' </param>
        '''
        ''' <returns>
        ''' The full path of the created file (without the Extended-Length prefix).
        ''' </returns>
        Friend Function CreateLongPathFile(content As Byte()) As String

            If content Is Nothing Then
                Throw New ArgumentNullException(NameOf(content))
            End If

            Dim segment As String = New String("d"c, 100)
            Dim directoryPath As String = Path.Combine(Me.FullPath, segment, segment, segment)
            Dim filePath As String = Path.Combine(directoryPath, $"{New String("f"c, 50)}.bin")

            Directory.CreateDirectory($"{TempDirectory.ExtendedPathPrefix}{directoryPath}")
            File.WriteAllBytes($"{TempDirectory.ExtendedPathPrefix}{filePath}", content)
            Return filePath
        End Function

        ''' <summary>
        ''' Creates a deterministic pseudo-random byte array.
        ''' </summary>
        '''
        ''' <param name="length">
        ''' The array length.
        ''' </param>
        '''
        ''' <param name="seed">
        ''' The seed of the pseudo-random generator.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting byte array.
        ''' </returns>
        Friend Shared Function CreateRandomBytes(length As Integer, seed As Integer) As Byte()

            Dim result As Byte() = New Byte(length - 1) {}
            Dim rnd As New Random(seed)
            rnd.NextBytes(result)
            Return result
        End Function

#End Region

#Region " IDisposable Implementation "

        ''' <summary>
        ''' Deletes the temporary directory and all its content,
        ''' resetting the attributes of read-only, hidden and system files beforehand.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose

            If Me.isDisposed Then
                Exit Sub
            End If
            Me.isDisposed = True

            Dim extendedDirectoryPath As String = $"{TempDirectory.ExtendedPathPrefix}{Me.FullPath}"
            If Not Directory.Exists(extendedDirectoryPath) Then
                Exit Sub
            End If

            For Each filePath As String In Directory.EnumerateFiles(extendedDirectoryPath, "*", SearchOption.AllDirectories)
                Thread.CurrentThread.Join(0)
                File.SetAttributes(filePath, FileAttributes.Normal)
            Next

            Directory.Delete(extendedDirectoryPath, recursive:=True)
        End Sub

#End Region

    End Class

End Namespace

#End Region
