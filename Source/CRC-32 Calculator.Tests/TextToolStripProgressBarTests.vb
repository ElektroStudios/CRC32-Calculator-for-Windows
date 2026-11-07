#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Reflection
Imports System.Runtime.ExceptionServices
Imports System.Windows.Forms

#End Region

#Region " TextToolStripProgressBarTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="TextToolStripProgressBar"/> control.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class TextToolStripProgressBarTests

#Region " Constants "

        ''' <summary>
        ''' The width, in pixels, of the bitmaps used to render the control.
        ''' </summary>
        Private Const RenderWidth As Integer = 200

        ''' <summary>
        ''' The height, in pixels, of the bitmaps used to render the control.
        ''' </summary>
        Private Const RenderHeight As Integer = 20

#End Region

#Region " CustomText Property "

        ''' <summary>
        ''' Verifies that the default custom text is "0%".
        ''' </summary>
        <TestMethod>
        Public Sub CustomText_NewInstance_DefaultsToZeroPercent()

            Using bar As New TextToolStripProgressBar()
                Assert.AreEqual("0%", bar.CustomText)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that the custom text round-trips any assigned value.
        ''' </summary>
        '''
        ''' <param name="value">
        ''' The value to assign.
        ''' </param>
        <TestMethod>
        <DataRow("57%")>
        <DataRow("100%")>
        <DataRow("")>
        <DataRow("Calculating... ñ ✓")>
        Public Sub CustomText_AssignedValue_ReturnsSameValue(value As String)

            Using bar As New TextToolStripProgressBar()
                bar.CustomText = value

                Assert.AreEqual(value, bar.CustomText)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that a null custom text is accepted and returned.
        ''' </summary>
        <TestMethod>
        Public Sub CustomText_AssignedNothing_ReturnsNothing()

            Using bar As New TextToolStripProgressBar()
                bar.CustomText = Nothing

                Assert.IsNull(bar.CustomText)
            End Using
        End Sub

#End Region

#Region " Construction "

        ''' <summary>
        ''' Verifies that the constructor enables owner drawing (and double buffering) on the hosted
        ''' <see cref="ProgressBar"/>, which is required for the custom text to be painted.
        ''' </summary>
        '''
        ''' <param name="style">
        ''' The control style to check.
        ''' </param>
        <TestMethod>
        <DataRow(ControlStyles.UserPaint)>
        <DataRow(ControlStyles.AllPaintingInWmPaint)>
        <DataRow(ControlStyles.OptimizedDoubleBuffer)>
        Public Sub Constructor_Always_EnablesOwnerDrawStylesOnHostedProgressBar(style As ControlStyles)

            Dim getStyleMethod As MethodInfo = GetType(Control).GetMethod("GetStyle", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(getStyleMethod, "Control.GetStyle method not found.")

            Using bar As New TextToolStripProgressBar()
                Dim isEnabled As Boolean = CBool(getStyleMethod.Invoke(bar.ProgressBar, New Object() {style}))

                Assert.IsTrue(isEnabled, $"The style '{style}' is not enabled on the hosted ProgressBar.")
            End Using
        End Sub

#End Region

#Region " Painting "

        ''' <summary>
        ''' Verifies that painting never throws, for any combination of value and custom text,
        ''' including a null or empty text and an empty (Minimum = Maximum) range.
        ''' </summary>
        '''
        ''' <param name="minimum">
        ''' The progress bar minimum.
        ''' </param>
        '''
        ''' <param name="maximum">
        ''' The progress bar maximum.
        ''' </param>
        '''
        ''' <param name="value">
        ''' The progress bar value.
        ''' </param>
        '''
        ''' <param name="text">
        ''' The custom text.
        ''' </param>
        <TestMethod>
        <DataRow(0, 100, 0, "0%", DisplayName:="Minimum value")>
        <DataRow(0, 100, 50, "50%", DisplayName:="Middle value")>
        <DataRow(0, 100, 100, "100%", DisplayName:="Maximum value")>
        <DataRow(0, 100, 1, "1%", DisplayName:="Tiny value")>
        <DataRow(10, 20, 15, "50%", DisplayName:="Non-zero minimum")>
        <DataRow(5, 5, 5, "N/A", DisplayName:="Empty range")>
        <DataRow(0, 100, 50, "", DisplayName:="Empty text")>
        <DataRow(0, 100, 50, Nothing, DisplayName:="Null text")>
        Public Sub Paint_AnyState_DoesNotThrow(minimum As Integer, maximum As Integer, value As Integer, text As String)

            Using bar As TextToolStripProgressBar = TextToolStripProgressBarTests.CreateBar(minimum, maximum, value, text),
                  bitmap As Bitmap = TextToolStripProgressBarTests.Render(bar)

                Assert.AreEqual(TextToolStripProgressBarTests.RenderWidth, bitmap.Width)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that the rendered progress at maximum differs from the rendered progress at minimum
        ''' (the progress chunks are painted).
        ''' </summary>
        <TestMethod>
        Public Sub Paint_MaximumVersusMinimumValue_ProducesDifferentImages()

            Using emptyBar As TextToolStripProgressBar = TextToolStripProgressBarTests.CreateBar(0, 100, 0, ""),
                  fullBar As TextToolStripProgressBar = TextToolStripProgressBarTests.CreateBar(0, 100, 100, ""),
                  emptyImage As Bitmap = TextToolStripProgressBarTests.Render(emptyBar),
                  fullImage As Bitmap = TextToolStripProgressBarTests.Render(fullBar)

                Assert.IsFalse(TextToolStripProgressBarTests.AreEqual(emptyImage, fullImage),
                               "The progress chunks were not painted.")
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that the custom text is painted over the bar.
        ''' </summary>
        <TestMethod>
        Public Sub Paint_WithCustomText_ProducesDifferentImageThanWithoutText()

            Using noTextBar As TextToolStripProgressBar = TextToolStripProgressBarTests.CreateBar(0, 100, 50, ""),
                  textBar As TextToolStripProgressBar = TextToolStripProgressBarTests.CreateBar(0, 100, 50, "50%"),
                  noTextImage As Bitmap = TextToolStripProgressBarTests.Render(noTextBar),
                  textImage As Bitmap = TextToolStripProgressBarTests.Render(textBar)

                Assert.IsFalse(TextToolStripProgressBarTests.AreEqual(noTextImage, textImage),
                               "The custom text was not painted.")
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that rendering the same state twice produces identical images (painting is deterministic).
        ''' </summary>
        <TestMethod>
        Public Sub Paint_SameStateTwice_ProducesIdenticalImages()

            Using bar As TextToolStripProgressBar = TextToolStripProgressBarTests.CreateBar(0, 100, 42, "42%"),
                  firstImage As Bitmap = TextToolStripProgressBarTests.Render(bar),
                  secondImage As Bitmap = TextToolStripProgressBarTests.Render(bar)

                Assert.IsTrue(TextToolStripProgressBarTests.AreEqual(firstImage, secondImage))
            End Using
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Creates a new <see cref="TextToolStripProgressBar"/> with the specified state.
        ''' </summary>
        '''
        ''' <param name="minimum">
        ''' The progress bar minimum.
        ''' </param>
        '''
        ''' <param name="maximum">
        ''' The progress bar maximum.
        ''' </param>
        '''
        ''' <param name="value">
        ''' The progress bar value.
        ''' </param>
        '''
        ''' <param name="text">
        ''' The custom text.
        ''' </param>
        '''
        ''' <returns>
        ''' The new progress bar.
        ''' </returns>
        Private Shared Function CreateBar(minimum As Integer, maximum As Integer, value As Integer, text As String) As TextToolStripProgressBar

            Dim bar As New TextToolStripProgressBar()
            bar.ProgressBar.Size = New Size(TextToolStripProgressBarTests.RenderWidth, TextToolStripProgressBarTests.RenderHeight)
            bar.Minimum = minimum
            bar.Maximum = maximum
            bar.Value = value
            bar.CustomText = text
            Return bar
        End Function

        ''' <summary>
        ''' Renders the hosted <see cref="ProgressBar"/> into a new bitmap by raising its Paint event
        ''' through the protected <see cref="Control"/>.OnPaint method.
        ''' </summary>
        '''
        ''' <param name="bar">
        ''' The progress bar to render.
        ''' </param>
        '''
        ''' <returns>
        ''' The rendered bitmap.
        ''' </returns>
        Private Shared Function Render(bar As TextToolStripProgressBar) As Bitmap

            Dim onPaintMethod As MethodInfo = GetType(Control).GetMethod("OnPaint", BindingFlags.Instance Or BindingFlags.NonPublic)
            If onPaintMethod Is Nothing Then
                Throw New InvalidOperationException("Control.OnPaint method not found.")
            End If

            Dim bitmap As New Bitmap(TextToolStripProgressBarTests.RenderWidth, TextToolStripProgressBarTests.RenderHeight, PixelFormat.Format32bppArgb)
            Try
                Using g As Graphics = Graphics.FromImage(bitmap)
                    g.Clear(Color.Magenta)
                    Dim clipRectangle As New Rectangle(0, 0, bitmap.Width, bitmap.Height)
                    Using e As New PaintEventArgs(g, clipRectangle)
                        Try
                            onPaintMethod.Invoke(bar.ProgressBar, New Object() {e})

                        Catch ex As TargetInvocationException When ex.InnerException IsNot Nothing
                            ' Surface the real exception thrown by the paint routine, preserving its stack trace.
                            ExceptionDispatchInfo.Capture(ex.InnerException).Throw()

                        End Try
                    End Using
                End Using

            Catch
                bitmap.Dispose()
                Throw

            End Try

            Return bitmap
        End Function

        ''' <summary>
        ''' Determines whether two bitmaps of the same size are pixel-identical.
        ''' </summary>
        '''
        ''' <param name="first">
        ''' The first bitmap.
        ''' </param>
        '''
        ''' <param name="second">
        ''' The second bitmap.
        ''' </param>
        '''
        ''' <returns>
        ''' <see langword="True"/> if both bitmaps are pixel-identical; otherwise, <see langword="False"/>.
        ''' </returns>
        Private Shared Function AreEqual(first As Bitmap, second As Bitmap) As Boolean

            If first.Size <> second.Size Then
                Return False
            End If

            For y As Integer = 0 To first.Height - 1
                For x As Integer = 0 To first.Width - 1
                    If first.GetPixel(x, y).ToArgb() <> second.GetPixel(x, y).ToArgb() Then
                        Return False
                    End If
                Next
            Next

            Return True
        End Function

#End Region

    End Class

End Namespace

#End Region
