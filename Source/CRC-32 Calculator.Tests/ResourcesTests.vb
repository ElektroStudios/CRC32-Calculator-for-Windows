#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Drawing

#End Region

#Region " ResourcesTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for the embedded image resources (<c>My.Resources.Resource1</c>) used by the user interface.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class ResourcesTests

        ''' <summary>
        ''' Verifies that every image resource used by the user interface is embedded and can be loaded as a valid bitmap.
        ''' </summary>
        '''
        ''' <param name="resourceName">
        ''' The name of the resource.
        ''' </param>
        <TestMethod>
        <DataRow("default")>
        <DataRow("browse")>
        <DataRow("check")>
        <DataRow("clear")>
        <DataRow("copy")>
        <DataRow("prohibition")>
        <DataRow("warning")>
        Public Sub Resource1_ImageResource_IsEmbeddedAndLoadable(resourceName As String)

            Dim resource As Object = Global.My.Resources.Resource1.ResourceManager.GetObject(resourceName, Global.My.Resources.Resource1.Culture)

            Assert.IsNotNull(resource, $"The resource '{resourceName}' was not found.")
            Assert.IsInstanceOfType(Of Bitmap)(resource)

            Using image As Bitmap = DirectCast(resource, Bitmap)
                Assert.IsTrue(image.Width > 0 AndAlso image.Height > 0, $"The resource '{resourceName}' has an invalid size.")
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that the strongly-typed resource properties used by the user interface return valid bitmaps.
        ''' </summary>
        <TestMethod>
        Public Sub Resource1_StronglyTypedProperties_ReturnBitmaps()

            Dim images As Bitmap() = {
                Global.My.Resources.Resource1._default,
                Global.My.Resources.Resource1.browse,
                Global.My.Resources.Resource1.check,
                Global.My.Resources.Resource1.clear,
                Global.My.Resources.Resource1.copy,
                Global.My.Resources.Resource1.prohibition,
                Global.My.Resources.Resource1.warning
            }

            For index As Integer = 0 To images.Length - 1
                Assert.IsNotNull(images(index), $"Image #{index} is null.")
                images(index).Dispose()
            Next
        End Sub

    End Class

End Namespace

#End Region
