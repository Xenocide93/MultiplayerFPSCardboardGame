Realtime Reflections provides a comprehensive solution to all your reflection needs. Realtime Reflections provides two methods of reflections:
1. Planar Reflections (for Mirrors, floors, planar surfaces)
2. Cubemap (Spatial) reflections (for Cars, balls, cubes and any other non-planar models)

THIS IS THE UNITY 5 VERSION WHICH INCLUDES SUPPORT FOR REFLECTION PROBES AND FIXES THE STANDARD SHADER.

NOTE: There is no need to import any other package as all the dependencies of this package are included. All models are from 3dwarehouse.sketchup.com and can be used 
      commercially.

	  The Planar Reflections Shader has been made by Michael Collins.

Features: 
1. In both planar and cubemap reflections, you can set the texture or cubemap resolution according to your needs (grahics or performace)
2. These scripts can be used with any shader (eg. Lux Physically Based Shaders, etc.) having the _Cube property (for cubemap reflections) and _ReflectionTex property
   (for planar reflections).
3. This package includes a shader for planar reflections "Realtime Reflections/Planar Reflection" (in the Shaders folder). For cubemap reflections the inbuilt Reflective 
   Shaders will do.
4. Menu items are under Realtime Reflections for ease of access.
5. Cubemap reflections are handled by a child prefab called Reflection Manager and Planar reflections are handled by a script on the Gameobject.
6. The reflections update in scene view also so you can preview the reflections and how they look without.
7. Includes support for Reflection Probes and Unity 5's PBR System.

Content:
All content is under "Assets/Realtime Reflections". All other content is placed under their specific sub-folder.

How-to:
1. Cubemap reflections:
   a. Under the menu item Realtime Reflections under GameObject, there are two options: Add to Selected Object and Add to Main Camera.
      i. Add to Selected Object is useful when you have a stationary object or the placement of the Reflection Manager under a Camera doesn't produce accurate results.
         In other words, it will function a bit like Unity 5 Reflection Probes.
      ii. Add to Main Camera automatically adds the Manager Prefab to the Main Camera in your scene.
      
   b. Modify the settings in the Realtime Reflections Script under the Reflection Manager. If you are using the Standard Shader, then create a Reflection Probe with a 
      good enough size for all objects and add it to the Reflection Probes array. The script will then automatically set up the probe.
   
2. Planar Reflections:
   a. Under the menu item Realtime Reflections under Component, there is a sub menu Planar Reflections which has an item called Add to Selected Object. this automatically adds the 
      Planar Realtime Reflections script and sets the shader to the one provided with this package. You must bear in mind that if you want to use any other shader, it must
      have a _ReflectionTex property. You can modify the script properties to your desire. You can refer to the script and the shader for more reference.
      
Demos:
This package contains two demos in the subfolder "Scenes" under the base folder of this package. 
1. PlanarReflectionDemo: It gives a demo of only the planar reflection of this package. It contains a 3rd person controller from the standard assets and another 
   free-to-use model.
2. CompleteReflectionDemo: It demonstrates both planar as well as cubemap realtime reflections. The shader for the car is the inbuilt Reflective/Specular shader.

Known Issues:
1. The cubemap reflections don't work well for planar reflections. That is why I added a separate script for planar reflection ;)
2. If you use the scale tool to scale an object which has the planar reflections script, The reflection gets scaled to but using the scale property of the transform,
   it doesn't. I wonder why?
   
Version Log/Changelog (mm/dd/yy):
1.0 (9/10/14): Initial Release.
1.1 (5/23/15): Added Support for Unity 5

If any Suggestions/Bugs please leave a review or contact me!

EVIL STUDIOS