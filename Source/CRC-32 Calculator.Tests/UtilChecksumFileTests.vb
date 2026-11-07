#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Imports Crc32Calculator.Tests.Helpers

Imports DevCase.Core.Security.DataIntegrity.Checksum

#End Region

#Region " UtilChecksumFileTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for <see cref="UtilChecksum.ComputeCRC32OfFileAsync(String, IProgress(Of Double), CancellationToken)"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class UtilChecksumFileTests

#Region " Fields "

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

#Region " Checksum Correctness "

        ''' <summary>
        ''' Verifies the standard CRC-32 check value when computed from a file.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_StandardCheckValueFile_ReturnsCBF43926() As Task

            Dim filePath As String = Me.tempDir.CreateFile("check.txt", Encoding.ASCII.GetBytes("123456789"))

            Dim actual As String =
                Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None).ConfigureAwait(False)

            Assert.AreEqual("CBF43926", actual)
        End Function

        ''' <summary>
        ''' Verifies that the checksum of an empty file is <c>00000000</c>.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_EmptyFile_ReturnsZeroChecksum() As Task

            Dim filePath As String = Me.tempDir.CreateFile("empty.bin", Array.Empty(Of Byte)())

            Dim actual As String =
                Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None).ConfigureAwait(False)

            Assert.AreEqual("00000000", actual)
        End Function

        ''' <summary>
        ''' Verifies that the incremental (chunked) file checksum equals the checksum of the whole content
        ''' computed in a single call, for sizes around the internal buffer size boundaries
        ''' (4 KB, 8 KB and 16 KB buffers are selected for these sizes).
        ''' </summary>
        '''
        ''' <param name="length">
        ''' The file length, in bytes.
        ''' </param>
        <TestMethod>
        <DataRow(1)>
        <DataRow(4095)>
        <DataRow(4096)>
        <DataRow(4097)>
        <DataRow(8192 * 3 + 17)>
        <DataRow(1048576, DisplayName:="1 MB (4 KB buffer, 256 chunks)")>
        <DataRow(1048577, DisplayName:="1 MB + 1 byte (8 KB buffer, partial last chunk)")>
        <DataRow(10485761, DisplayName:="10 MB + 1 byte (16 KB buffer, partial last chunk)")>
        Public Async Function ComputeCRC32OfFileAsync_RandomContent_MatchesBytesAndReferenceChecksum(length As Integer) As Task

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile($"random_{length}.bin", length, content)

            Dim actual As String =
                Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None).ConfigureAwait(False)

            Assert.AreEqual(ReferenceCrc32.ComputeHex(content), actual, "Mismatch with the managed reference implementation.")
            Assert.AreEqual(UtilChecksum.ComputeCRC32OfBytes(content), actual, "Mismatch with ComputeCRC32OfBytes.")
        End Function

        ''' <summary>
        ''' Verifies that the same file processed concurrently by several operations yields the same checksum
        ''' (the file is opened with read sharing).
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_ConcurrentComputationsOfSameFile_ReturnSameChecksum() As Task

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile("concurrent.bin", 2 * 1048576, content)
            Dim expected As String = ReferenceCrc32.ComputeHex(content)

            Dim tasks As Task(Of String)() =
                Enumerable.Range(0, 4).
                           Select(Function(i As Integer) UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None)).
                           ToArray()

            Dim results As String() = Await Task.WhenAll(tasks).ConfigureAwait(False)

            For Each result As String In results
                Assert.AreEqual(expected, result)
            Next
        End Function

#End Region

#Region " Paths "

        ''' <summary>
        ''' Verifies that an Extended-Length path (\\?\ prefix) produces the same checksum as the standard path.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_ExtendedLengthPath_ReturnsSameChecksumAsStandardPath() As Task

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile("extended.bin", 5000, content)

            Dim standardResult As String =
                Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None).ConfigureAwait(False)
            Dim extendedResult As String =
                Await UtilChecksum.ComputeCRC32OfFileAsync(PathHelper.GetExtendedPath(filePath), Nothing, CancellationToken.None).ConfigureAwait(False)

            Assert.AreEqual(standardResult, extendedResult)
        End Function

        ''' <summary>
        ''' Verifies that a file whose path is longer than MAX_PATH (260 characters) can be processed
        ''' through its Extended-Length path, as the application does.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_PathLongerThanMaxPath_ComputesChecksumThroughExtendedPath() As Task

            Dim content As Byte() = Encoding.ASCII.GetBytes("123456789")
            Dim longFilePath As String = Me.tempDir.CreateLongPathFile(content)
            Assert.IsTrue(longFilePath.Length > 260, "Precondition: the test path must be longer than MAX_PATH.")

            Dim actual As String =
                Await UtilChecksum.ComputeCRC32OfFileAsync(PathHelper.GetExtendedPath(longFilePath), Nothing, CancellationToken.None).ConfigureAwait(False)

            Assert.AreEqual("CBF43926", actual)
        End Function

        ''' <summary>
        ''' Verifies that a file name with Unicode characters is processed correctly.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_UnicodeFileName_ComputesChecksum() As Task

            Dim filePath As String = Me.tempDir.CreateFile("Ñandú ünïcödé 日本語 ✓.txt", Encoding.ASCII.GetBytes("123456789"))

            Dim actual As String =
                Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None).ConfigureAwait(False)

            Assert.AreEqual("CBF43926", actual)
        End Function

#End Region

#Region " Progress Reporting "

        ''' <summary>
        ''' Verifies that progress is reported starting at 0%, ending at exactly 100%,
        ''' always within range and never decreasing.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_WithProgress_ReportsMonotonicValuesFromZeroToOneHundred() As Task

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile("progress.bin", 1048577, content)
            Dim progress As New RecordingProgress(Of Double)()

            Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, progress, CancellationToken.None).ConfigureAwait(False)

            Dim values As IReadOnlyList(Of Double) = progress.Values
            Assert.IsTrue(values.Count >= 3, $"Expected several progress reports, but got {values.Count}.")
            Assert.AreEqual(0.0, values.First(), "The first report must be 0%.")
            Assert.AreEqual(100.0, values.Last(), "The last report must be exactly 100%.")

            For index As Integer = 1 To values.Count - 1
                Assert.IsTrue(values(index) >= values(index - 1),
                              $"Progress decreased from {values(index - 1)} to {values(index)} at report #{index}.")
                Assert.IsTrue(values(index) >= 0.0 AndAlso values(index) <= 100.0,
                              $"Progress value {values(index)} is out of range.")
            Next
        End Function

        ''' <summary>
        ''' Verifies that a file smaller than the read buffer reports exactly 0% and 100%.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_FileSmallerThanBuffer_ReportsZeroAndOneHundred() As Task

            Dim filePath As String = Me.tempDir.CreateFile("small.txt", Encoding.ASCII.GetBytes("123456789"))
            Dim progress As New RecordingProgress(Of Double)()

            Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, progress, CancellationToken.None).ConfigureAwait(False)

            CollectionAssert.AreEqual(New Double() {0.0, 100.0}, progress.Values.ToArray())
        End Function

        ''' <summary>
        ''' Verifies that an empty file only reports the initial 0% (no division by zero is performed).
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_EmptyFile_ReportsOnlyInitialZero() As Task

            Dim filePath As String = Me.tempDir.CreateFile("empty.bin", Array.Empty(Of Byte)())
            Dim progress As New RecordingProgress(Of Double)()

            Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, progress, CancellationToken.None).ConfigureAwait(False)

            CollectionAssert.AreEqual(New Double() {0.0}, progress.Values.ToArray())
        End Function

        ''' <summary>
        ''' Verifies that a null progress provider is supported.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_NullProgress_DoesNotThrow() As Task

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile("noprogress.bin", 20000, content)

            Dim actual As String =
                Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None).ConfigureAwait(False)

            Assert.AreEqual(ReferenceCrc32.ComputeHex(content), actual)
        End Function

#End Region

#Region " Cancellation "

        ''' <summary>
        ''' Verifies that an already canceled token aborts the operation before reading any data.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_AlreadyCanceledToken_ThrowsOperationCanceledException() As Task

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile("canceled.bin", 10000, content)
            Dim progress As New RecordingProgress(Of Double)()

            Using cts As New CancellationTokenSource()
                cts.Cancel()

                Await Assert.ThrowsAsync(Of OperationCanceledException)(
                    Function() UtilChecksum.ComputeCRC32OfFileAsync(filePath, progress, cts.Token)).ConfigureAwait(False)
            End Using

            ' Only the initial 0% report is expected; no data has been processed.
            CollectionAssert.AreEqual(New Double() {0.0}, progress.Values.ToArray())
        End Function

        ''' <summary>
        ''' Verifies that canceling the token in the middle of the computation aborts the operation
        ''' before reaching 100%.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_CanceledDuringComputation_ThrowsOperationCanceledException() As Task

            Dim content As Byte() = Nothing
            ' 4 MB with a 8 KB buffer: 512 chunks, so the cancellation is requested long before the end.
            Dim filePath As String = Me.tempDir.CreateRandomFile("cancel_midway.bin", 4 * 1048576, content)

            Using cts As New CancellationTokenSource()
                Dim progress As New RecordingProgress(Of Double)(
                    Sub(value As Double)
                        If value > 0.0 Then
                            cts.Cancel()
                        End If
                    End Sub)

                Await Assert.ThrowsAsync(Of OperationCanceledException)(
                    Function() UtilChecksum.ComputeCRC32OfFileAsync(filePath, progress, cts.Token)).ConfigureAwait(False)

                Assert.IsTrue(progress.Values.Last() < 100.0, "The operation should have been aborted before completion.")
            End Using
        End Function

        ''' <summary>
        ''' Verifies that the file handle is released after a canceled operation,
        ''' so the file can be opened exclusively and deleted.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_AfterCancellation_ReleasesFileHandle() As Task

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile("released.bin", 1048576, content)

            Using cts As New CancellationTokenSource()
                Dim progress As New RecordingProgress(Of Double)(
                    Sub(value As Double)
                        If value > 0.0 Then
                            cts.Cancel()
                        End If
                    End Sub)

                Await Assert.ThrowsAsync(Of OperationCanceledException)(
                    Function() UtilChecksum.ComputeCRC32OfFileAsync(filePath, progress, cts.Token)).ConfigureAwait(False)
            End Using

            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                Assert.IsTrue(fs.CanWrite)
            End Using
            File.Delete(filePath)
            Assert.IsFalse(File.Exists(filePath))
        End Function

        ''' <summary>
        ''' Verifies that the file handle is released after a successful operation.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_AfterSuccess_ReleasesFileHandle() As Task

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile("released_ok.bin", 50000, content)

            Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None).ConfigureAwait(False)

            File.Delete(filePath)
            Assert.IsFalse(File.Exists(filePath))
        End Function

#End Region

#Region " Error Handling "

        ''' <summary>
        ''' Verifies that a non-existent file throws <see cref="FileNotFoundException"/>.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_NonExistentFile_ThrowsFileNotFoundException() As Task

            Dim filePath As String = Path.Combine(Me.tempDir.FullPath, "does_not_exist.bin")

            Await Assert.ThrowsAsync(Of FileNotFoundException)(
                Function() UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None)).ConfigureAwait(False)
        End Function

        ''' <summary>
        ''' Verifies that passing a directory path instead of a file path throws <see cref="FileNotFoundException"/>.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_DirectoryPath_ThrowsFileNotFoundException() As Task

            Await Assert.ThrowsAsync(Of FileNotFoundException)(
                Function() UtilChecksum.ComputeCRC32OfFileAsync(Me.tempDir.FullPath, Nothing, CancellationToken.None)).ConfigureAwait(False)
        End Function

        ''' <summary>
        ''' Verifies that a null file path throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_NullPath_ThrowsArgumentNullException() As Task

            Await Assert.ThrowsAsync(Of ArgumentNullException)(
                Function() UtilChecksum.ComputeCRC32OfFileAsync(Nothing, Nothing, CancellationToken.None)).ConfigureAwait(False)
        End Function

        ''' <summary>
        ''' Verifies that a file locked for writing by another handle (that does not allow write sharing to be denied)
        ''' throws <see cref="IOException"/>, because the file is opened with <see cref="FileShare.Read"/>.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_FileOpenedForWritingByAnotherHandle_ThrowsIOException() As Task

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile("locked.bin", 1000, content)

            Using writer As New FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)
                Await Assert.ThrowsAsync(Of IOException)(
                    Function() UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None)).ConfigureAwait(False)
            End Using
        End Function

        ''' <summary>
        ''' Verifies that a file already opened for reading by another handle (with read sharing) can be processed.
        ''' </summary>
        <TestMethod>
        Public Async Function ComputeCRC32OfFileAsync_FileOpenedForReadingByAnotherHandle_ComputesChecksum() As Task

            Dim content As Byte() = Nothing
            Dim filePath As String = Me.tempDir.CreateRandomFile("shared.bin", 1000, content)

            Using reader As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim actual As String =
                    Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, Nothing, CancellationToken.None).ConfigureAwait(False)

                Assert.AreEqual(ReferenceCrc32.ComputeHex(content), actual)
            End Using
        End Function

#End Region

    End Class

End Namespace

#End Region
