#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Linq

Imports DevCase.Core.IO.Memory

#End Region

#Region " BufferSizesTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="BufferSizes"/> enumeration.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class BufferSizesTests

        ''' <summary>
        ''' Verifies that every named buffer size has the documented value, in bytes.
        ''' </summary>
        '''
        ''' <param name="value">
        ''' The enumeration value.
        ''' </param>
        '''
        ''' <param name="expectedBytes">
        ''' The expected size, in bytes.
        ''' </param>
        <TestMethod>
        <DataRow(BufferSizes.Kb1, 1024)>
        <DataRow(BufferSizes.Kb2, 2048)>
        <DataRow(BufferSizes.Kb4, 4096)>
        <DataRow(BufferSizes.Kb8, 8192)>
        <DataRow(BufferSizes.Kb16, 16384)>
        <DataRow(BufferSizes.Kb32, 32768)>
        <DataRow(BufferSizes.Kb64, 65536)>
        <DataRow(BufferSizes.Kb128, 131072)>
        <DataRow(BufferSizes.Kb256, 262144)>
        <DataRow(BufferSizes.Kb512, 524288)>
        <DataRow(BufferSizes.Mb1, 1048576)>
        Public Sub BufferSizes_NamedValues_HaveExpectedByteSize(value As BufferSizes, expectedBytes As Integer)

            Assert.AreEqual(expectedBytes, CInt(value))
        End Sub

        ''' <summary>
        ''' Verifies that the aliases of the .NET default buffer sizes match the reference source values.
        ''' </summary>
        '''
        ''' <param name="aliasName">
        ''' The name of the alias.
        ''' </param>
        '''
        ''' <param name="expectedBytes">
        ''' The expected size, in bytes.
        ''' </param>
        <TestMethod>
        <DataRow("FileStreamDefault", 4096)>
        <DataRow("BufferedStreamDefault", 4096)>
        <DataRow("StreamReaderDefault", 1024)>
        <DataRow("StreamWriterDefault", 1024)>
        Public Sub BufferSizes_DefaultAliases_MatchFrameworkDefaults(aliasName As String, expectedBytes As Integer)

            Dim value As BufferSizes = DirectCast([Enum].Parse(GetType(BufferSizes), aliasName), BufferSizes)

            Assert.AreEqual(expectedBytes, CInt(value))
        End Sub

        ''' <summary>
        ''' Verifies that every value of the enumeration is a positive power of two.
        ''' </summary>
        <TestMethod>
        Public Sub BufferSizes_AllValues_ArePositivePowersOfTwo()

            For Each value As BufferSizes In [Enum].GetValues(GetType(BufferSizes)).Cast(Of BufferSizes)()
                Dim bytes As Integer = CInt(value)

                Assert.IsTrue(bytes > 0, $"{value} must be positive.")
                Assert.AreEqual(0, bytes And (bytes - 1), $"{value} ({bytes}) is not a power of two.")
            Next
        End Sub

    End Class

End Namespace

#End Region
