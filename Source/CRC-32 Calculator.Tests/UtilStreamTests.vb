#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports DevCase.Core.IO.FileSystem
Imports DevCase.Core.IO.Memory

#End Region

#Region " UtilStreamTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for <see cref="UtilStream.GetOptimalFileStreamBufferSize(Long)"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class UtilStreamTests

        ''' <summary>
        ''' Verifies the selected buffer size at each threshold boundary (exact threshold value and threshold + 1 byte).
        ''' </summary>
        '''
        ''' <param name="filesize">
        ''' The file size, in bytes.
        ''' </param>
        '''
        ''' <param name="expected">
        ''' The expected buffer size.
        ''' </param>
        <TestMethod>
        <DataRow(Long.MinValue, BufferSizes.Kb4, DisplayName:="Long.MinValue")>
        <DataRow(-1L, BufferSizes.Kb4, DisplayName:="Negative size")>
        <DataRow(0L, BufferSizes.Kb4, DisplayName:="Zero")>
        <DataRow(1L, BufferSizes.Kb4)>
        <DataRow(1048576L, BufferSizes.Kb4, DisplayName:="1 MB (exact)")>
        <DataRow(1048577L, BufferSizes.Kb8, DisplayName:="1 MB + 1")>
        <DataRow(10485760L, BufferSizes.Kb8, DisplayName:="10 MB (exact)")>
        <DataRow(10485761L, BufferSizes.Kb16, DisplayName:="10 MB + 1")>
        <DataRow(52428800L, BufferSizes.Kb16, DisplayName:="50 MB (exact)")>
        <DataRow(52428801L, BufferSizes.Kb32, DisplayName:="50 MB + 1")>
        <DataRow(104857600L, BufferSizes.Kb32, DisplayName:="100 MB (exact)")>
        <DataRow(104857601L, BufferSizes.Kb64, DisplayName:="100 MB + 1")>
        <DataRow(209715200L, BufferSizes.Kb64, DisplayName:="200 MB (exact)")>
        <DataRow(209715201L, BufferSizes.Kb128, DisplayName:="200 MB + 1")>
        <DataRow(314572800L, BufferSizes.Kb128, DisplayName:="300 MB (exact)")>
        <DataRow(314572801L, BufferSizes.Kb256, DisplayName:="300 MB + 1")>
        <DataRow(524288000L, BufferSizes.Kb256, DisplayName:="500 MB (exact)")>
        <DataRow(524288001L, BufferSizes.Kb512, DisplayName:="500 MB + 1")>
        <DataRow(1073741824L, BufferSizes.Kb512, DisplayName:="1 GB (exact)")>
        <DataRow(1073741825L, BufferSizes.Mb1, DisplayName:="1 GB + 1")>
        <DataRow(4294967296L, BufferSizes.Mb1, DisplayName:="4 GB")>
        <DataRow(Long.MaxValue, BufferSizes.Mb1, DisplayName:="Long.MaxValue")>
        Public Sub GetOptimalFileStreamBufferSize_ThresholdBoundaries_ReturnsExpectedBufferSize(filesize As Long, expected As BufferSizes)

            Dim actual As BufferSizes = UtilStream.GetOptimalFileStreamBufferSize(filesize)

            Assert.AreEqual(expected, actual)
        End Sub

        ''' <summary>
        ''' Verifies that the buffer size never decreases as the file size grows.
        ''' </summary>
        <TestMethod>
        Public Sub GetOptimalFileStreamBufferSize_IncreasingFileSizes_ReturnsNonDecreasingBufferSizes()

            Dim previous As Integer = 0
            Dim filesize As Long = 1L
            Do While filesize > 0L AndAlso filesize < (Long.MaxValue \ 2L)
                Dim current As Integer = UtilStream.GetOptimalFileStreamBufferSize(filesize)
                Assert.IsTrue(current >= previous, $"Buffer size decreased from {previous} to {current} at file size {filesize}.")

                previous = current
                filesize *= 2L
            Loop
        End Sub

        ''' <summary>
        ''' Verifies that the returned buffer size is always a valid, positive value for a <see cref="IO.FileStream"/>.
        ''' </summary>
        <TestMethod>
        Public Sub GetOptimalFileStreamBufferSize_AnyFileSize_ReturnsPositivePowerOfTwo()

            Dim sizes As Long() = {0L, 1L, 1048577L, 10485761L, 52428801L, 104857601L, 209715201L, 314572801L, 524288001L, 1073741825L}

            For Each filesize As Long In sizes
                Dim actual As Integer = UtilStream.GetOptimalFileStreamBufferSize(filesize)

                Assert.IsTrue(actual > 0, $"Buffer size must be positive (file size {filesize}).")
                Assert.AreEqual(0, actual And (actual - 1), $"Buffer size {actual} is not a power of two (file size {filesize}).")
            Next
        End Sub

    End Class

End Namespace

#End Region
