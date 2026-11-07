#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Text
Imports System.Text.RegularExpressions

Imports Crc32Calculator.Tests.Helpers

Imports DevCase.Core.Security.DataIntegrity.Checksum

#End Region

#Region " UtilChecksumBytesTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for <see cref="UtilChecksum.ComputeCRC32OfBytes(Byte())"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class UtilChecksumBytesTests

#Region " Known Test Vectors "

        ''' <summary>
        ''' Verifies the result against well-known, published CRC-32 (IEEE 802.3) test vectors.
        ''' </summary>
        '''
        ''' <param name="asciiInput">
        ''' The ASCII input string.
        ''' </param>
        '''
        ''' <param name="expectedCrc32">
        ''' The expected CRC-32 checksum.
        ''' </param>
        <TestMethod>
        <DataRow("", "00000000", DisplayName:="Empty string")>
        <DataRow("a", "E8B7BE43")>
        <DataRow("abc", "352441C2")>
        <DataRow("message digest", "20159D7F")>
        <DataRow("123456789", "CBF43926", DisplayName:="Standard CRC-32 check value")>
        <DataRow("The quick brown fox jumps over the lazy dog", "414FA339")>
        Public Sub ComputeCRC32OfBytes_KnownAsciiVectors_ReturnsExpectedChecksum(asciiInput As String, expectedCrc32 As String)

            Dim data As Byte() = Encoding.ASCII.GetBytes(asciiInput)

            Dim actual As String = UtilChecksum.ComputeCRC32OfBytes(data)

            Assert.AreEqual(expectedCrc32, actual)
        End Sub

        ''' <summary>
        ''' Verifies the result against well-known CRC-32 test vectors made of binary (non-printable) data.
        ''' </summary>
        '''
        ''' <param name="value">
        ''' The value of every byte of the input.
        ''' </param>
        '''
        ''' <param name="length">
        ''' The input length, in bytes.
        ''' </param>
        '''
        ''' <param name="expectedCrc32">
        ''' The expected CRC-32 checksum.
        ''' </param>
        <TestMethod>
        <DataRow(CByte(&H0), 1, "D202EF8D", DisplayName:="Single zero byte")>
        <DataRow(CByte(&H0), 32, "190A55AD", DisplayName:="32 zero bytes")>
        <DataRow(CByte(&HFF), 4, "FFFFFFFF", DisplayName:="4 bytes of 0xFF")>
        Public Sub ComputeCRC32OfBytes_KnownBinaryVectors_ReturnsExpectedChecksum(value As Byte, length As Integer, expectedCrc32 As String)

            Dim data As Byte() = New Byte(length - 1) {}
            For index As Integer = 0 To data.Length - 1
                data(index) = value
            Next

            Dim actual As String = UtilChecksum.ComputeCRC32OfBytes(data)

            Assert.AreEqual(expectedCrc32, actual)
        End Sub

#End Region

#Region " Output Format "

        ''' <summary>
        ''' Verifies that the result is always an uppercase, 8-digit hexadecimal string.
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_AnyInput_ReturnsUppercaseEightDigitHexString()

            Dim formatRegex As New Regex("^[0-9A-F]{8}$")

            For length As Integer = 0 To 64
                Dim data As Byte() = TempDirectory.CreateRandomBytes(length, seed:=length)

                Dim actual As String = UtilChecksum.ComputeCRC32OfBytes(data)

                Assert.IsTrue(formatRegex.IsMatch(actual), $"Unexpected format '{actual}' for length {length}.")
            Next
        End Sub

        ''' <summary>
        ''' Verifies that checksums lower than <c>0x10000000</c> are padded with leading zeros.
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_ChecksumWithLeadingZeros_IsZeroPadded()

            ' CRC-32 of ASCII "62" is 0x0012D20A.
            Dim data As Byte() = Encoding.ASCII.GetBytes("62")

            Dim actual As String = UtilChecksum.ComputeCRC32OfBytes(data)

            Assert.AreEqual("0012D20A", actual)
        End Sub

#End Region

#Region " Behavior "

        ''' <summary>
        ''' Verifies that the native implementation matches the managed reference implementation
        ''' for pseudo-random data of many different lengths.
        ''' </summary>
        '''
        ''' <param name="length">
        ''' The input length, in bytes.
        ''' </param>
        <TestMethod>
        <DataRow(1)>
        <DataRow(3)>
        <DataRow(4)>
        <DataRow(7)>
        <DataRow(8)>
        <DataRow(15)>
        <DataRow(16)>
        <DataRow(255)>
        <DataRow(1023)>
        <DataRow(4096)>
        <DataRow(65537)>
        <DataRow(1048576)>
        Public Sub ComputeCRC32OfBytes_RandomData_MatchesReferenceImplementation(length As Integer)

            Dim data As Byte() = TempDirectory.CreateRandomBytes(length, seed:=length)

            Dim actual As String = UtilChecksum.ComputeCRC32OfBytes(data)

            Assert.AreEqual(ReferenceCrc32.ComputeHex(data), actual)
        End Sub

        ''' <summary>
        ''' Verifies that the computation is deterministic.
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_SameInputTwice_ReturnsSameChecksum()

            Dim data As Byte() = TempDirectory.CreateRandomBytes(10000, seed:=42)

            Dim first As String = UtilChecksum.ComputeCRC32OfBytes(data)
            Dim second As String = UtilChecksum.ComputeCRC32OfBytes(data)

            Assert.AreEqual(first, second)
        End Sub

        ''' <summary>
        ''' Verifies that the input array is not modified by the computation.
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_AnyInput_DoesNotModifyInputArray()

            Dim data As Byte() = TempDirectory.CreateRandomBytes(1024, seed:=7)
            Dim snapshot As Byte() = DirectCast(data.Clone(), Byte())

            UtilChecksum.ComputeCRC32OfBytes(data)

            CollectionAssert.AreEqual(snapshot, data)
        End Sub

        ''' <summary>
        ''' Verifies that flipping any single bit of the input changes the checksum
        ''' (CRC-32 detects all single-bit errors).
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_SingleBitFlip_ChangesChecksum()

            Dim data As Byte() = TempDirectory.CreateRandomBytes(64, seed:=1234)
            Dim original As String = UtilChecksum.ComputeCRC32OfBytes(data)

            For byteIndex As Integer = 0 To data.Length - 1
                For bit As Integer = 0 To 7
                    Dim mask As Byte = CByte(1 << bit)
                    data(byteIndex) = data(byteIndex) Xor mask

                    Dim modified As String = UtilChecksum.ComputeCRC32OfBytes(data)

                    data(byteIndex) = data(byteIndex) Xor mask ' Restore the original byte.
                    Assert.AreNotEqual(original, modified, $"Bit {bit} of byte {byteIndex} was not detected.")
                Next
            Next
        End Sub

        ''' <summary>
        ''' Verifies that the byte order of the input matters (CRC-32 is not a simple sum).
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_TransposedBytes_ChangesChecksum()

            Dim original As Byte() = {1, 2, 3, 4}
            Dim transposed As Byte() = {2, 1, 3, 4}

            Assert.AreNotEqual(UtilChecksum.ComputeCRC32OfBytes(original), UtilChecksum.ComputeCRC32OfBytes(transposed))
        End Sub

        ''' <summary>
        ''' Verifies that trailing zero bytes change the checksum (the data length is relevant).
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_AppendedZeroByte_ChangesChecksum()

            Dim original As Byte() = {&HA, &HB, &HC}
            Dim padded As Byte() = {&HA, &HB, &HC, &H0}

            Assert.AreNotEqual(UtilChecksum.ComputeCRC32OfBytes(original), UtilChecksum.ComputeCRC32OfBytes(padded))
        End Sub

#End Region

    End Class

End Namespace

#End Region
