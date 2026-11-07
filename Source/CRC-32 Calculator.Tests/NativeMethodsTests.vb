#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Text

Imports Crc32Calculator.Tests.Helpers

Imports DevCase.Win32.Enums
Imports DevCase.Win32.NativeMethods

#End Region

#Region " NativeMethodsTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for the P/Invoke declarations of <see cref="NtDll"/> and <see cref="ShlwApi"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class NativeMethodsTests

#Region " NtDll.RtlComputeCrc32 "

        ''' <summary>
        ''' Verifies that chaining <see cref="NtDll.RtlComputeCrc32"/> calls over two consecutive chunks
        ''' (passing the previous result as the initial value) equals a single call over the whole data.
        ''' <para></para>
        ''' This is the property the incremental file checksum relies on.
        ''' </summary>
        '''
        ''' <param name="splitIndex">
        ''' The index where the data is split into two chunks.
        ''' </param>
        <TestMethod>
        <DataRow(0)>
        <DataRow(1)>
        <DataRow(500)>
        <DataRow(4096)>
        <DataRow(9999)>
        <DataRow(10000)>
        Public Sub RtlComputeCrc32_ChainedChunks_EqualsSingleCall(splitIndex As Integer)

            Dim data As Byte() = TempDirectory.CreateRandomBytes(10000, seed:=99)
            Dim firstChunk As Byte() = New Byte(splitIndex - 1) {}
            Dim secondChunk As Byte() = New Byte(data.Length - splitIndex - 1) {}
            Array.Copy(data, 0, firstChunk, 0, firstChunk.Length)
            Array.Copy(data, splitIndex, secondChunk, 0, secondChunk.Length)

            Dim singleCall As UInteger = NtDll.RtlComputeCrc32(0UI, data, CUInt(data.Length))
            Dim chained As UInteger = NtDll.RtlComputeCrc32(0UI, firstChunk, CUInt(firstChunk.Length))
            chained = NtDll.RtlComputeCrc32(chained, secondChunk, CUInt(secondChunk.Length))

            Assert.AreEqual(singleCall, chained)
            Assert.AreEqual(ReferenceCrc32.Compute(data), singleCall)
        End Sub

        ''' <summary>
        ''' Verifies that only the first <c>bufferLen</c> bytes of the buffer are processed,
        ''' as the file checksum relies on it when the last chunk is smaller than the buffer.
        ''' </summary>
        <TestMethod>
        Public Sub RtlComputeCrc32_BufferLengthSmallerThanArray_ProcessesOnlyPrefix()

            Dim buffer As Byte() = TempDirectory.CreateRandomBytes(4096, seed:=5)
            Const prefixLength As Integer = 1000

            Dim actual As UInteger = NtDll.RtlComputeCrc32(0UI, buffer, CUInt(prefixLength))

            Assert.AreEqual(ReferenceCrc32.Compute(0UI, buffer, 0, prefixLength), actual)
        End Sub

        ''' <summary>
        ''' Verifies that a zero length leaves the initial value unchanged.
        ''' </summary>
        '''
        ''' <param name="initialValue">
        ''' The initial CRC value.
        ''' </param>
        <TestMethod>
        <DataRow(0UI)>
        <DataRow(&HCBF43926UI)>
        <DataRow(UInteger.MaxValue)>
        Public Sub RtlComputeCrc32_ZeroLength_ReturnsInitialValue(initialValue As UInteger)

            Dim actual As UInteger = NtDll.RtlComputeCrc32(initialValue, New Byte() {1, 2, 3}, 0UI)

            Assert.AreEqual(initialValue, actual)
        End Sub

#End Region

#Region " ShlwApi.StrFormatByteSizeEx "

        ''' <summary>
        ''' Verifies that <see cref="ShlwApi.StrFormatByteSizeEx"/> succeeds (returns S_OK) and produces a non-empty string
        ''' for a wide range of values, using the same buffer configuration as the application.
        ''' </summary>
        '''
        ''' <param name="number">
        ''' The value to format.
        ''' </param>
        <TestMethod>
        <DataRow(0UL)>
        <DataRow(1UL)>
        <DataRow(1023UL)>
        <DataRow(1024UL)>
        <DataRow(1048576UL)>
        <DataRow(1073741824UL)>
        <DataRow(1099511627776UL)>
        <DataRow(ULong.MaxValue)>
        Public Sub StrFormatByteSizeEx_AnyValue_ReturnsSuccessAndNonEmptyString(number As ULong)

            Dim buffer As New StringBuilder(32)

            Dim hResult As Integer = ShlwApi.StrFormatByteSizeEx(number, StrFormatByteSizeFlags.Truncate, buffer, CUInt(buffer.Capacity))

            Assert.AreEqual(0, hResult, $"Unexpected HRESULT 0x{hResult:X8} for {number}.")
            Assert.IsFalse(String.IsNullOrWhiteSpace(buffer.ToString()), $"Empty result for {number}.")
        End Sub

        ''' <summary>
        ''' Verifies that the <see cref="StrFormatByteSizeFlags"/> values are honored:
        ''' 1535 bytes (1.499 KB) is truncated to "1.49 KB" but rounded to "1.50 KB"
        ''' (the decimal separator depends on the current locale).
        ''' </summary>
        <TestMethod>
        Public Sub StrFormatByteSizeEx_TruncateVersusRound_ProduceDifferentResults()

            Const number As ULong = 1535UL
            Dim truncated As New StringBuilder(32)
            Dim rounded As New StringBuilder(32)

            Assert.AreEqual(0, ShlwApi.StrFormatByteSizeEx(number, StrFormatByteSizeFlags.Truncate, truncated, CUInt(truncated.Capacity)))
            Assert.AreEqual(0, ShlwApi.StrFormatByteSizeEx(number, StrFormatByteSizeFlags.RoundToNearest, rounded, CUInt(rounded.Capacity)))

            StringAssert.Contains(truncated.ToString(), "49")
            StringAssert.Contains(rounded.ToString(), "50")
            Assert.AreNotEqual(truncated.ToString(), rounded.ToString())
        End Sub

        ''' <summary>
        ''' Verifies the documented numeric values of the <see cref="StrFormatByteSizeFlags"/> enumeration (SFBS_FLAGS).
        ''' </summary>
        <TestMethod>
        Public Sub StrFormatByteSizeFlags_Values_MatchWin32Definition()

            Assert.AreEqual(1, CInt(StrFormatByteSizeFlags.RoundToNearest))
            Assert.AreEqual(2, CInt(StrFormatByteSizeFlags.Truncate))
        End Sub

#End Region

    End Class

End Namespace

#End Region
