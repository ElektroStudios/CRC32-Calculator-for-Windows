#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

#End Region

#Region " ThemeApplierTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="ThemeApplier"/> module.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class ThemeApplierTests

#Region " Argument Validation "

        ''' <summary>
        ''' Verifies that a null root control throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub ApplyTheme_NullControl_ThrowsArgumentNullException()

            Assert.ThrowsExactly(Of ArgumentNullException)(Sub() ThemeApplier.ApplyTheme(DirectCast(Nothing, Control), useDarkTheme:=True))
        End Sub

        ''' <summary>
        ''' Verifies that a null tooltip throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub ApplyTheme_NullToolTip_ThrowsArgumentNullException()

            Assert.ThrowsExactly(Of ArgumentNullException)(Sub() ThemeApplier.ApplyTheme(DirectCast(Nothing, ToolTip), useDarkTheme:=True))
        End Sub

#End Region

#Region " Dark Theme "

        ''' <summary>
        ''' Verifies that the dark theme is applied to every kind of control and tool strip item of a form
        ''' that mirrors the main form of the application.
        ''' </summary>
        <TestMethod>
        Public Sub ApplyTheme_Dark_AppliesDarkColorsToAllElements()

            Using fixture As New FormFixture()

                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=True)

                Assert.AreEqual(DarkThemeColors.WindowBackground, fixture.Form.BackColor, "Form.BackColor")
                Assert.AreEqual(DarkThemeColors.Text, fixture.Form.ForeColor, "Form.ForeColor")

                Assert.AreEqual(DarkThemeColors.WindowBackground, fixture.Label.BackColor, "Label.BackColor")
                Assert.AreEqual(DarkThemeColors.Text, fixture.Label.ForeColor, "Label.ForeColor")

                Assert.AreEqual(DarkThemeColors.ControlBackground, fixture.ReadOnlyTextBox.BackColor, "TextBox.BackColor")
                Assert.AreEqual(DarkThemeColors.Text, fixture.ReadOnlyTextBox.ForeColor, "TextBox.ForeColor")
                Assert.AreEqual(BorderStyle.FixedSingle, fixture.ReadOnlyTextBox.BorderStyle, "TextBox.BorderStyle")

                Assert.AreEqual(FlatStyle.Flat, fixture.Button.FlatStyle, "Button.FlatStyle")
                Assert.IsFalse(fixture.Button.UseVisualStyleBackColor, "Button.UseVisualStyleBackColor")
                Assert.AreEqual(DarkThemeColors.ControlBackground, fixture.Button.BackColor, "Button.BackColor")
                Assert.AreEqual(DarkThemeColors.Border, fixture.Button.FlatAppearance.BorderColor, "Button.FlatAppearance.BorderColor")
                Assert.AreEqual(DarkThemeColors.ControlHover, fixture.Button.FlatAppearance.MouseOverBackColor, "Button.FlatAppearance.MouseOverBackColor")

                Assert.AreEqual(DarkThemeColors.Link, fixture.LinkLabel.LinkColor, "LinkLabel.LinkColor")

                Assert.IsInstanceOfType(Of DarkToolStripRenderer)(fixture.StatusStrip.Renderer, "StatusStrip.Renderer")
                Assert.AreEqual(DarkThemeColors.StatusBackground, fixture.StatusStrip.BackColor, "StatusStrip.BackColor")

                Assert.AreEqual(DarkThemeColors.SecondaryText, fixture.DimmedStatusLabel.ForeColor, "Dimmed status label keeps a dimmed (secondary) color.")
                Assert.AreEqual(DarkThemeColors.Text, fixture.StatusButton.ForeColor, "Status button inherits the strip text color.")
                Assert.IsFalse(ThemeApplierTests.IsExplicitlySet(fixture.StatusButton, NameOf(ToolStripItem.ForeColor)),
                               "The status button text color must keep being inherited (not explicitly set).")
                Assert.IsTrue(fixture.ProgressBar.UseDarkTheme, "ProgressBar.UseDarkTheme")
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that the controls hosted by tool strip items are not themed as standalone controls: 
        ''' none of their color properties is explicitly assigned.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' The values themselves may change, because a hosted control without explicit colors inherits 
        ''' the (ambient) colors of the tool strip that contains it; that is expected and harmless.
        ''' </remarks>
        <TestMethod>
        Public Sub ApplyTheme_Dark_DoesNotModifyControlsHostedByToolStripItems()

            Using fixture As New FormFixture()
                Dim hostedControl As Control = fixture.ProgressBar.ProgressBar
                Assert.IsFalse(ThemeApplierTests.IsExplicitlySet(hostedControl, NameOf(Control.BackColor)), "Precondition: BackColor must be inherited.")
                Assert.IsFalse(ThemeApplierTests.IsExplicitlySet(hostedControl, NameOf(Control.ForeColor)), "Precondition: ForeColor must be inherited.")

                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=True)

                Assert.IsFalse(ThemeApplierTests.IsExplicitlySet(hostedControl, NameOf(Control.BackColor)), "BackColor was explicitly assigned.")
                Assert.IsFalse(ThemeApplierTests.IsExplicitlySet(hostedControl, NameOf(Control.ForeColor)), "ForeColor was explicitly assigned.")
            End Using
        End Sub

#End Region

#Region " Restoration (Light Theme) "

        ''' <summary>
        ''' Verifies that switching back to the light theme restores exactly the original design:
        ''' the same values, and the same "explicitly set" versus "inherited/default" state of every modified property.
        ''' </summary>
        <TestMethod>
        Public Sub ApplyTheme_DarkThenLight_RestoresOriginalAppearanceExactly()

            Using fixture As New FormFixture()
                Dim before As String() = fixture.CaptureAppearance()
                Dim rendererTypeBefore As Type = fixture.StatusStrip.Renderer.GetType()

                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=True)
                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=False)

                CollectionAssert.AreEqual(before, fixture.CaptureAppearance())
                Assert.AreEqual(rendererTypeBefore, fixture.StatusStrip.Renderer.GetType(), "StatusStrip.Renderer")
                Assert.IsFalse(fixture.ProgressBar.UseDarkTheme, "ProgressBar.UseDarkTheme")
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that a read-only <see cref="TextBox"/> recovers its system gray background after the light theme is restored
        ''' (its background color is reset, not assigned).
        ''' </summary>
        <TestMethod>
        Public Sub ApplyTheme_DarkThenLight_ReadOnlyTextBoxRecoversSystemBackground()

            Using fixture As New FormFixture()
                Dim originalBackColor As Color = fixture.ReadOnlyTextBox.BackColor

                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=True)
                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=False)

                Assert.IsFalse(ThemeApplierTests.IsExplicitlySet(fixture.ReadOnlyTextBox, NameOf(Control.BackColor)))
                Assert.AreEqual(originalBackColor, fixture.ReadOnlyTextBox.BackColor)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that applying the dark theme several times does not overwrite the captured original state.
        ''' </summary>
        <TestMethod>
        Public Sub ApplyTheme_DarkTwiceThenLight_RestoresOriginalAppearance()

            Using fixture As New FormFixture()
                Dim before As String() = fixture.CaptureAppearance()

                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=True)
                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=True)
                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=False)

                CollectionAssert.AreEqual(before, fixture.CaptureAppearance())
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that several dark/light cycles (live theme switches) always end with the original design.
        ''' </summary>
        <TestMethod>
        Public Sub ApplyTheme_SeveralDarkLightCycles_RestoresOriginalAppearance()

            Using fixture As New FormFixture()
                Dim before As String() = fixture.CaptureAppearance()

                For cycle As Integer = 1 To 3
                    ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=True)
                    ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=False)

                    CollectionAssert.AreEqual(before, fixture.CaptureAppearance(), $"Mismatch after cycle #{cycle}.")
                Next
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that applying the light theme to a form that was never darkened leaves its design untouched.
        ''' </summary>
        <TestMethod>
        Public Sub ApplyTheme_LightOnUntouchedForm_DoesNotModifyAnything()

            Using fixture As New FormFixture()
                Dim before As String() = fixture.CaptureAppearance()
                Dim rendererBefore As ToolStripRenderer = fixture.StatusStrip.Renderer

                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=False)

                CollectionAssert.AreEqual(before, fixture.CaptureAppearance())
                Assert.AreSame(rendererBefore, fixture.StatusStrip.Renderer)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that a tool strip item added after the dark theme was applied (as the application does in the Load event)
        ''' inherits the dark text color once the tool strip lays it out, and that the light theme leaves it with an inherited color too.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' A tool strip item without an explicit text color inherits it from the tool strip it is placed on, 
        ''' and that placement happens during the tool strip layout (in the application, when the form is shown). 
        ''' The test form is never shown, so the layout is forced explicitly.
        ''' </remarks>
        <TestMethod>
        Public Sub ApplyTheme_ItemAddedAfterDarkTheme_InheritsColorsAndIsRestored()

            Using fixture As New FormFixture()
                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=True)

                Dim lateButton As New ToolStripButton("Late")
                fixture.StatusStrip.Items.Add(lateButton)
                fixture.StatusStrip.PerformLayout()

                Assert.AreEqual(ToolStripItemPlacement.Main, lateButton.Placement,
                                "Precondition: the item must be placed on the status strip after its layout.")
                Assert.AreEqual(DarkThemeColors.Text, lateButton.ForeColor)

                ThemeApplier.ApplyTheme(fixture.Form, useDarkTheme:=False)

                Assert.IsFalse(ThemeApplierTests.IsExplicitlySet(lateButton, NameOf(ToolStripItem.ForeColor)))
                Assert.AreEqual(fixture.StatusStrip.ForeColor, lateButton.ForeColor)
            End Using
        End Sub

#End Region

#Region " ToolTip "

        ''' <summary>
        ''' Verifies that the dark theme turns the tooltip into an owner-drawn tooltip,
        ''' and that the light theme restores the original system-drawn tooltip.
        ''' </summary>
        <TestMethod>
        Public Sub ApplyTheme_ToolTipDarkThenLight_TogglesOwnerDrawAndRestoresColors()

            Using container As New Container(),
                  tip As New ToolTip(container)

                Dim before As String() = ThemeApplierTests.CaptureAppearance(tip, NameOf(ToolTip.OwnerDraw), NameOf(ToolTip.BackColor), NameOf(ToolTip.ForeColor))

                ThemeApplier.ApplyTheme(tip, useDarkTheme:=True)

                Assert.IsTrue(tip.OwnerDraw)
                Assert.AreEqual(DarkThemeColors.StatusBackground, tip.BackColor)
                Assert.AreEqual(DarkThemeColors.Text, tip.ForeColor)

                ThemeApplier.ApplyTheme(tip, useDarkTheme:=False)

                CollectionAssert.AreEqual(before, ThemeApplierTests.CaptureAppearance(tip, NameOf(ToolTip.OwnerDraw), NameOf(ToolTip.BackColor), NameOf(ToolTip.ForeColor)))
            End Using
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Determines whether the specified property of an object is explicitly set (not inherited nor default).
        ''' </summary>
        '''
        ''' <param name="target">
        ''' The target object.
        ''' </param>
        '''
        ''' <param name="propertyName">
        ''' The property name.
        ''' </param>
        '''
        ''' <returns>
        ''' <see langword="True"/> if the property is explicitly set; otherwise, <see langword="False"/>.
        ''' </returns>
        Private Shared Function IsExplicitlySet(target As Object, propertyName As String) As Boolean

            Return TypeDescriptor.GetProperties(target)(propertyName).ShouldSerializeValue(target)
        End Function

        ''' <summary>
        ''' Captures the appearance of an object as a list of "Type.Property|IsExplicitlySet|Value" strings.
        ''' </summary>
        '''
        ''' <param name="target">
        ''' The target object.
        ''' </param>
        '''
        ''' <param name="propertyNames">
        ''' The names of the properties to capture.
        ''' </param>
        '''
        ''' <returns>
        ''' The captured appearance.
        ''' </returns>
        Friend Shared Function CaptureAppearance(target As Object, ParamArray propertyNames As String()) As String()

            Dim properties As PropertyDescriptorCollection = TypeDescriptor.GetProperties(target)
            Return propertyNames.Select(
                Function(name As String)
                    Dim descriptor As PropertyDescriptor = properties(name)
                    Return $"{target.GetType().Name}.{name}|{descriptor.ShouldSerializeValue(target)}|{descriptor.GetValue(target)}"
                End Function).ToArray()
        End Function

#End Region

#Region " FormFixture "

        ''' <summary>
        ''' A form that mirrors the kind of controls and the designer settings of the main form of the application.
        ''' </summary>
        Private NotInheritable Class FormFixture : Implements IDisposable

            ''' <summary>
            ''' Gets the form (with an explicit <see cref="SystemColors.Control"/> background, as in the designer).
            ''' </summary>
            Friend ReadOnly Property Form As Form

            ''' <summary>
            ''' Gets a label without explicit colors.
            ''' </summary>
            Friend ReadOnly Property Label As Label

            ''' <summary>
            ''' Gets a read-only text box without explicit colors (system gray background).
            ''' </summary>
            Friend ReadOnly Property ReadOnlyTextBox As TextBox

            ''' <summary>
            ''' Gets a button that uses the visual style background color.
            ''' </summary>
            Friend ReadOnly Property Button As Button

            ''' <summary>
            ''' Gets a link label with the default link colors.
            ''' </summary>
            Friend ReadOnly Property LinkLabel As LinkLabel

            ''' <summary>
            ''' Gets a status strip with the default renderer.
            ''' </summary>
            Friend ReadOnly Property StatusStrip As StatusStrip

            ''' <summary>
            ''' Gets a status label with an explicit dimmed text color (as in the designer).
            ''' </summary>
            Friend ReadOnly Property DimmedStatusLabel As ToolStripStatusLabel

            ''' <summary>
            ''' Gets a status button without explicit colors.
            ''' </summary>
            Friend ReadOnly Property StatusButton As ToolStripButton

            ''' <summary>
            ''' Gets the custom progress bar.
            ''' </summary>
            Friend ReadOnly Property ProgressBar As TextToolStripProgressBar

            ''' <summary>
            ''' Initializes a new instance of the <see cref="FormFixture"/> class.
            ''' </summary>
            Friend Sub New()

                Me.Form = New Form() With {.BackColor = SystemColors.Control}
                Me.Label = New Label() With {.Text = "File"}
                Me.ReadOnlyTextBox = New TextBox() With {.ReadOnly = True, .Enabled = False}
                Me.Button = New Button() With {.Text = "Copy", .UseVisualStyleBackColor = True}
                Me.LinkLabel = New LinkLabel() With {.Text = "Link"}
                Me.StatusStrip = New StatusStrip()
                Me.DimmedStatusLabel = New ToolStripStatusLabel("Waiting...") With {.ForeColor = SystemColors.ControlDarkDark}
                Me.StatusButton = New ToolStripButton("About")
                Me.ProgressBar = New TextToolStripProgressBar()

                Me.StatusStrip.Items.AddRange(New ToolStripItem() {Me.DimmedStatusLabel, Me.StatusButton, Me.ProgressBar})
                Me.Form.Controls.AddRange(New Control() {Me.Label, Me.ReadOnlyTextBox, Me.Button, Me.LinkLabel, Me.StatusStrip})
            End Sub

            ''' <summary>
            ''' Captures the appearance of every element of the form.
            ''' </summary>
            '''
            ''' <returns>
            ''' The captured appearance.
            ''' </returns>
            Friend Function CaptureAppearance() As String()

                Dim result As New List(Of String)()
                result.AddRange(ThemeApplierTests.CaptureAppearance(Me.Form, "BackColor", "ForeColor"))
                result.AddRange(ThemeApplierTests.CaptureAppearance(Me.Label, "BackColor", "ForeColor"))
                result.AddRange(ThemeApplierTests.CaptureAppearance(Me.ReadOnlyTextBox, "BackColor", "ForeColor", "BorderStyle"))
                result.AddRange(ThemeApplierTests.CaptureAppearance(Me.Button, "BackColor", "ForeColor", "FlatStyle", "UseVisualStyleBackColor"))
                result.AddRange(ThemeApplierTests.CaptureAppearance(Me.Button.FlatAppearance, "BorderColor", "MouseOverBackColor", "MouseDownBackColor"))
                result.AddRange(ThemeApplierTests.CaptureAppearance(Me.LinkLabel, "BackColor", "ForeColor", "LinkColor", "ActiveLinkColor", "VisitedLinkColor", "DisabledLinkColor"))
                result.AddRange(ThemeApplierTests.CaptureAppearance(Me.StatusStrip, "BackColor", "ForeColor", "RenderMode"))
                result.AddRange(ThemeApplierTests.CaptureAppearance(Me.DimmedStatusLabel, "BackColor", "ForeColor"))
                result.AddRange(ThemeApplierTests.CaptureAppearance(Me.StatusButton, "BackColor", "ForeColor"))
                result.AddRange(ThemeApplierTests.CaptureAppearance(Me.ProgressBar, "UseDarkTheme"))
                Return result.ToArray()
            End Function

            ''' <summary>
            ''' Disposes the form and all its controls.
            ''' </summary>
            Public Sub Dispose() Implements IDisposable.Dispose
                Me.Form.Dispose()
            End Sub

        End Class

#End Region

    End Class

End Namespace

#End Region
