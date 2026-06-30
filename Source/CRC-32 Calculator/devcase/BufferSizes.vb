
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO

Imports DevCase.Core.IO.FileSystem

#End Region

#Region " Buffer Sizes "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.IO.Memory

    ''' <summary>
    ''' Specifies common buffer sizes to use for a disk or memory read/write operation.
    ''' <para></para>
    ''' To calculate the optimal read/write buffer size for a determined filesize, 
    ''' use the <see cref="UtilStream.GetOptimalFileStreamBufferSize"/> function.
    ''' </summary>
    ''' <remarks>
    ''' Microsoft article: <see href="https://docs.microsoft.com/en-us/windows/desktop/FileIO/file-buffering"/>
    ''' <para></para>
    ''' Hans Passant comment: <see href="https://stackoverflow.com/a/3034155/1248295"/>
    ''' </remarks>
    Public Enum BufferSizes As Integer

        ''' <summary>
        ''' A buffer size of 1 Kilobyte (1024 bytes)
        ''' </summary>
        Kb1 = 1024

        ''' <summary>
        ''' A buffer size of 2 Kilobytes (2048 bytes)
        ''' </summary>
        Kb2 = 2048

        ''' <summary>
        ''' A buffer size of 4 Kilobytes (4096 bytes)
        ''' </summary>
        Kb4 = 4096

        ''' <summary>
        ''' A buffer size of 8 Kilobytes (8192 bytes)
        ''' </summary>
        Kb8 = 8192

        ''' <summary>
        ''' A buffer size of 16 Kilobytes (16384 bytes)
        ''' </summary>
        Kb16 = 16384

        ''' <summary>
        ''' A buffer size of 32 Kilobytes (32768 bytes)
        ''' </summary>
        Kb32 = 32768

        ''' <summary>
        ''' A buffer size of 64 Kilobytes (65536 bytes)
        ''' </summary>
        Kb64 = 65536

        ''' <summary>
        ''' A buffer size of 128 Kilobytes (131072 bytes)
        ''' </summary>
        Kb128 = 131072

        ''' <summary>
        ''' A buffer size of 256 Kilobytes (262144 bytes)
        ''' </summary>
        Kb256 = 262144

        ''' <summary>
        ''' A buffer size of 512 Kilobytes (524288 bytes)
        ''' </summary>
        Kb512 = 524288

        ''' <summary>
        ''' A buffer size of 1 Megabyte (1048576 bytes)
        ''' </summary>
        Mb1 = 1048576

        ''' <summary>
        ''' A buffer size of 4 Kilobytes (4096 bytes)
        ''' <para></para>
        ''' This is the default buffer size of <see cref="FileStream"/> implementation.
        ''' </summary>
        ''' <remarks>
        ''' <see href="https://referencesource.microsoft.com/#mscorlib/system/io/filestream.cs,1d898d04082f2b0a"/>
        ''' </remarks>
        FileStreamDefault = BufferSizes.Kb4

        ''' <summary>
        ''' A buffer size of 4 Kilobytes (4096 bytes)
        ''' <para></para>
        ''' This is the default buffer size of <see cref="BufferedStream"/> implementation.
        ''' </summary>
        ''' <remarks>
        ''' <see href="https://referencesource.microsoft.com/mscorlib/system/io/bufferedstream.cs.html#279030f365f03047"/>
        ''' </remarks>
        BufferedStreamDefault = BufferSizes.Kb4

        ''' <summary>
        ''' A buffer size of 1 Kilobyte (1024 bytes)
        ''' <para></para>
        ''' This is the default buffer size of <see cref="StreamReader"/> implementation.
        ''' </summary>
        ''' <remarks>
        ''' <see href="https://referencesource.microsoft.com/mscorlib/system/io/streamreader.cs.html#6875bce072fdba23"/>
        ''' </remarks>
        StreamReaderDefault = BufferSizes.Kb1

        ''' <summary>
        ''' A buffer size of 1 Kilobyte (1024 bytes)
        ''' <para></para>
        ''' This is the default buffer size of <see cref="StreamWriter"/> implementation.
        ''' </summary>
        ''' <remarks>
        ''' <see href="https://referencesource.microsoft.com/mscorlib/system/io/streamwriter.cs.html#62bd8ad495f57b21"/>
        ''' </remarks>
        StreamWriterDefault = BufferSizes.Kb1

    End Enum

End Namespace

#End Region
