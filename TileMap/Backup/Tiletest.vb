Imports System.IO

Public Class TileTest
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents Clock As System.Windows.Forms.Timer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.Clock = New System.Windows.Forms.Timer(Me.components)
        '
        'Clock
        '
        '
        'TileTest
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(440, 469)
        Me.Name = "TileTest"
        Me.Text = "Tile Map"

    End Sub

#End Region

    Dim Tileset As Bitmap    'Holds the tile pictures.

    Dim Backup As Bitmap     'Backup picture holds the map that we've just drawn.
    Dim GFX As Graphics     'Draws to the backup picture.

    Dim FormGFX As Graphics     'Draws to the form.

    Dim Map(,) As Integer       'The map is a 2D array of integers.

    Dim Charpics(3) As Bitmap        'Hold each frame in one of the array elements.
    Dim Cyc As Integer                  'This number tells which frame to draw.

    Dim CharacterLocation As Point
    Dim SymbolicTileloc As PointF       'Holds the tile position now: since the character can be partially off of a tile.
    'This is a PointF so that we can have decimal values representing portions of a tile.

    Const Tilesize As Integer = 16
    Const ViewTileCount As Integer = 7
    Const NumofTilesBetweenCharacterAndScreenEdge As Integer = (ViewTileCount - 1) \ 2
    Const Mapsizeintiles As Integer = 24


    Private Sub TileTest_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


        Tileset = New Bitmap("tileset.bmp")     'Put the bitmap into the tileset picture.

        Backup = New Bitmap(16 * ViewTileCount, 16 * ViewTileCount)           'Backup will be initialzed to an empty bitmap

        GFX = Graphics.FromImage(Backup)        'GFX draws onto the backup picture.
        FormGFX = Me.CreateGraphics()           'Draw to the form.

        Dim SR As StreamReader = New StreamReader("map.txt")
        Dim Ln As String
        Dim LX, LY As Integer
        Dim Chs As String
        'The map in the file is now 24x24, so the array must be initialized as:

        Map = New Integer(23, 23) {}

        For LY = 0 To 23
            Ln = SR.ReadLine()  'One row of tiles from the file.

            For LX = 0 To 23    'Goes through each column.

                Chs = Ln.Substring(LX, 1)   'Get a string with one character in it.

                Map(LX, LY) = Integer.Parse(Chs)  'Convert this string into its matching number.
                '"0" -> 0, "1" -> 1 ...
                'The number is put into the map.

            Next

        Next

        SR.Close()

        Me.CharacterLocation = New Point(16, 16)           'Character location now represents a pixel location on the map.
        Me.SymbolicTileloc = New PointF(1.0F, 1.0F)  'The point 16,16 corresponds to Tile 1,1
        'The formula for converting location to tile location would be multiplying the location's x and y by 16 and put them into
        'the symbolictilelocation.

        Charpics(0) = New Bitmap("Solagon.gif")
        Charpics(1) = New Bitmap("Solagon2.gif")
        Charpics(2) = New Bitmap("Solagon3.gif")
        Charpics(3) = New Bitmap("Solagon4.gif")
        'Load each frame one by one into the array.

        'Allow the keypresses to go directly to the form.
        Me.KeyPreview = True
        'Enable the clock.
        Clock.Start()

        Draw()      'Draw the map.

    End Sub

    Private Sub Draw()  'Draw map to the backup.

        'Drawing the map requires nested for loops.

        Dim LX, LY As Integer
        Dim Thisindex As Integer        'The index of the map at the point LX,LY.
        Dim Tile As Rectangle    'Holds the rectangle defining which tile we will draw.

        'There are a few things that we need to consider about the scrolling of the map before we apply these two things.
        'Since the map is scrolling, when a player is partially to another tile, there will be more than 7x7 tiles on the screen.
        'A partial row/column will be on both ends of the screen and they need to be drawn as well.
        'Also note that the screen should only scroll when the camera is not at one of the end.

        'To facilitate the scrolling, our camera will also be the pixel location of the upper left of the map.
        Dim CamX, CamY As Integer

        'The player will always be 3 tiles * 16 (tile size) = 48 from the left, so the Camera will always be the
        'player's location minus 48 (except of course, when the player is at the ends of the map.


        CamX = Me.CharacterLocation.X - Me.NumofTilesBetweenCharacterAndScreenEdge * Me.Tilesize
        'The camera must also remain in the range.
        If CamX < 0 Then
            CamX = 0
        ElseIf CamX > (Me.Mapsizeintiles - Me.ViewTileCount) * Me.Tilesize Then
            CamX = (Me.Mapsizeintiles - Me.ViewTileCount) * Me.Tilesize  'This is the calculated value of the camera's maximum location in the map.
        End If  'The View width is subtracted from the map width
        CamY = Me.CharacterLocation.Y - 48
        If CamY < 0 Then
            CamY = 0
        ElseIf CamY > (Me.Mapsizeintiles - Me.ViewTileCount) * Me.Tilesize Then
            CamY = (Me.Mapsizeintiles - Me.ViewTileCount) * Me.Tilesize
        End If

        Dim StartX, StartY, EndX, EndY As Integer

        'This is the tile that corresponds to the upper left corner on the display.
        StartX = CamX \ Me.Tilesize
        StartY = CamY \ Me.Tilesize

        Dim OffX, OffY As Integer
        'If the camera is 4 greater than a value that is the upperleft corner of a tile, then the tile
        'will be drawn -4 away from the tile position that would fit neatly within the map.
        OffX = CamX Mod Me.Tilesize
        OffY = CamY Mod Me.Tilesize

        If OffX = 0 Then
            EndX = StartX + Me.ViewTileCount - 1  'the last tile on the end is six tiles past the beginning tile.
        Else
            EndX = StartX + Me.ViewTileCount  'here it would be 7.
        End If
        If OffY = 0 Then
            EndY = StartY + Me.ViewTileCount - 1
        Else
            EndY = StartY + Me.ViewTileCount
        End If

        For LY = StartY To EndY
            For LX = StartX To EndX

                Thisindex = Map(LX, LY)
                'Get the index from the map.
                'The values of LX and LY already depend on the camera position, so no need to add it again.

                Tile = New Rectangle(Thisindex * Tilesize, 0, Tilesize, Tilesize)
                'Get the rectangle defining the tile.  (same)

                GFX.DrawImage(Tileset, (LX - StartX) * Tilesize - OffX, (LY - StartY) * Tilesize - OffY, Tile, GraphicsUnit.Pixel)
                'And then draw the tile onto the backup.

            Next
        Next


        'Map code.
        GFX.DrawImage(Charpics(Cyc), (CharacterLocation.X - CamX), (CharacterLocation.Y - CamY))
        'Draw the character to the display at the location... this also changes depending on the camera location, so
        'the camera location is subtracted from the character location.  The character only appears to remain centered.

        'At this point the whole map should reside on the backup bitmap.  The bitmap is not visible, so it must be made visible.
        FormGFX.DrawImage(Backup, Me.ClientRectangle())





    End Sub



    Private Sub Clock_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Clock.Tick
        Cyc = (Cyc + 1) Mod 4   'Cyc cycles through 0, 1, 2, 3, 0, 1, 2, 3, 0, 1 ...
        Draw()      'Draw.
    End Sub

    Private Sub TileTest_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles MyBase.Paint
        'FormGFX.SetClip(Me.ClientRectangle)
        FormGFX.DrawImage(Backup, Me.ClientRectangle())

    End Sub

    Private Sub TileTest_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown

        Dim Collided As Boolean

        'The character now moves four pixels instead of by a whole tile (16).
        'Collision detection always comes after this movement caused by the user.
        If e.KeyCode = Keys.Left Then
            CharacterLocation.Offset(-4, 0)
        ElseIf e.KeyCode = Keys.Up Then
            CharacterLocation.Offset(0, -4)
        ElseIf e.KeyCode = Keys.Right Then
            CharacterLocation.Offset(4, 0)
        ElseIf e.KeyCode = Keys.Down Then
            CharacterLocation.Offset(0, 4)
        End If

        'The character can be on, at most, four tiles at once.  So, in addition to the map boundary checks,
        'we'll also have to check the four tiles that the character is on for collision.
        'The upper left tile being, of course, the characterlocation \ tilesize (for X and Y).
        'however, we can use the SymbolicTileLoc to get the tile location as well, rounding up and rounding down with
        'Floor and Ceiling to get the tiles that the character is on.
        SymbolicTileloc.X = Convert.ToSingle(CharacterLocation.X / 16)
        SymbolicTileloc.Y = Convert.ToSingle(CharacterLocation.Y / 16)

        'Check for collisions after moving the character.
        If CharacterLocation.X < 0 OrElse CharacterLocation.X > Map.GetUpperBound(0) * 16 OrElse CharacterLocation.Y < 0 OrElse CharacterLocation.Y > Map.GetUpperBound(1) * 16 Then
            Collided = True
            'Don't ask why Ceiling and Floor return doubles.
        ElseIf Map(Convert.ToInt32(Math.Floor(SymbolicTileloc.X)), Convert.ToInt32(Math.Floor(SymbolicTileloc.Y))) = 3 Then
            Collided = True
        ElseIf Map(Convert.ToInt32(Math.Ceiling(SymbolicTileloc.X)), Convert.ToInt32(Math.Floor(SymbolicTileloc.Y))) = 3 Then
            Collided = True
        ElseIf Map(Convert.ToInt32(Math.Floor(SymbolicTileloc.X)), Convert.ToInt32(Math.Ceiling(SymbolicTileloc.Y))) = 3 Then
            Collided = True
        ElseIf Map(Convert.ToInt32(Math.Ceiling(SymbolicTileloc.X)), Convert.ToInt32(Math.Ceiling(SymbolicTileloc.Y))) = 3 Then
            Collided = True

        End If

        'If we have collded, then the character needs to move back immediately.
        If Collided Then
            'Take all of the keycodes and reverse their operation.
            If e.KeyCode = Keys.Left Then
                CharacterLocation.Offset(4, 0)
            ElseIf e.KeyCode = Keys.Up Then
                CharacterLocation.Offset(0, 4)
            ElseIf e.KeyCode = Keys.Right Then
                CharacterLocation.Offset(-4, 0)
            ElseIf e.KeyCode = Keys.Down Then
                CharacterLocation.Offset(0, -4)
            End If

            SymbolicTileloc.X = Convert.ToSingle(CharacterLocation.X / 16)
            SymbolicTileloc.Y = Convert.ToSingle(CharacterLocation.Y / 16)
        End If

        'Note that the keydown event can fire more than once over the 100 ms that the timer is not drawing.
    End Sub
End Class
