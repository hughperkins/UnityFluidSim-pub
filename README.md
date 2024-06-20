# UnityFluidSim-pub

Code for my video on Real-time Eulerian simulation:

[![Real-time Eulerian fluid simulation on a Macbook Air, using GPU shaders](http://img.youtube.com/vi/x6mcua0HOJs/0.jpg)](https://youtu.be/x6mcua0HOJs)

To use:
- Load and run this with Unity
- Drawing:
    - '~' draws velocity that points in the same direction as you are moving the mouse
    - '<', '>', '^', 'V' draw velocity that moves left, right, up, down, respectively
    - 'O' draws 0 velocity
    - 'X' erases velocity
- 'persistent', for draw:
    - fixes the cell to specific velocity
- 'persistent' for color:
    - fixes the cell to specific ink color
- the various views selectable with the buttons on the right side of the Game view:
    - Design view. Ignore this for now. But if you load in a texture, this will show here
    - Cell Active: is the cell a static, fixed cell; or is it a fluid cell (white => fluid, black => fixed)
    - Color Src: if you add colored ink, any 'persistent' colored ink shows up here (non-persistent does not)
    - Moving Tex: renders velocity with moving texture
    - Vel HSV: renders velocity using HSV (hue is the direction, saturation is the speed)
    - Divergence: renders the divergence, green is positive, red is negative (you'll need to turn off projection to see anythign other than black)
- Many of the controls are in the Inspector:
  - select the 'Main Camera'
- to change the resolution, change "No Design Res Y", .e.g set to 80, or 480, or 8
- to turn projection on/off
    - in Inspector, on right hand side, toggle 'Run Project'
- to turn advection on/off
    - In Inspector, on right hand side, toggle 'Run Advect Velocity'
- to choose projection type, change dropdown 'Project Solver'
- to choose advection type, change dropdown 'Advecter'
- to turn arrows on/off, scroll down in the Inspector, and toggle 'Cell Arrows'

There's also other options you can experiment with :)
